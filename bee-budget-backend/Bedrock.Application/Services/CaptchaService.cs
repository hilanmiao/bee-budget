using Bedrock.Application.DataTransferObjects;
using Bedrock.Application.Interfaces;
using Bedrock.Configuration;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;
using SkiaSharp;
using StackExchange.Redis;
using System.Text;

namespace Bedrock.Application.Services;

/// <summary>
/// 图形验证码服务实现类，基于 SkiaSharp 绘图并使用 Redis 分布式缓存存储验证码。
/// 支持多 Redis 实例隔离（通过配置 InstanceName），适用于集群部署环境。
/// </summary>
/// <remarks>
/// <para>
/// <strong>设计目标：</strong>
/// - 生成视觉清晰但机器难以识别的验证码图像（对抗 OCR）
/// - 防止重放攻击（验证成功后立即删除）
/// - 高并发下线程安全
/// - 兼容 .NET 8 + SkiaSharp v3.119.0 API 变更
/// </para>
/// 
/// <para>
/// <strong>技术栈说明：</strong>
/// - <see cref="IConnectionMultiplexer"/>：可选，用于未来扩展（如监控、事务等）
/// - <see cref="SkiaSharp"/>：高性能 2D 图形库，用于生成 PNG 图像
/// </para>
/// 
/// <para>
/// <strong>验证码策略：</strong>
/// - 长度：4 位字符
/// - 字符集：去除易混淆字符（0/O, 1/I/L）
/// - 尺寸：150x60 像素
/// - 有效期：5 分钟（<see cref="Expiry"/>）
/// - 存储：Redis 缓存，Key 格式为 {InstanceName}:captcha:{CaptchaId}
/// </para>
/// </remarks>
public class CaptchaService : ICaptchaService
{
    private readonly IConnectionMultiplexer _redis;
    private readonly IDatabase _redisDb; // Redis 数据库实例
    private readonly string _instanceName;

    // ========================
    // 验证码配置常量
    // ========================

    /// <summary>
    /// 验证码图像宽度（像素）。
    /// </summary>
    private const int Width = 150;

    /// <summary>
    /// 验证码图像高度（像素）。
    /// </summary>
    private const int Height = 60;

    /// <summary>
    /// 验证码字符数量。
    /// </summary>
    private const int CodeLength = 4;

    /// <summary>
    /// 验证码默认有效期（5分钟）。
    /// </summary>
    private static readonly TimeSpan Expiry = TimeSpan.FromMinutes(5);

    /// <summary>
    /// 构造函数，注入依赖项。
    /// </summary>
    /// <param name="redis">Redis 连接多路复用器，可用于高级操作（如批量删除、监控）。</param>
    /// <param name="redisConfig">Redis 配置选项，用于获取实例名称以实现多租户/多环境隔离。</param>
    /// <exception cref="ArgumentNullException">当任意参数为 null 时抛出。</exception>
    public CaptchaService(
        IConnectionMultiplexer redis,
        IOptions<RedisConfig> redisConfig)
    {
        _redis = redis ?? throw new ArgumentNullException(nameof(redis));
        _redisDb = _redis.GetDatabase();
        _instanceName = redisConfig.Value.InstanceName;
    }

    /// <summary>
    /// 生成一个新的图形验证码，包含唯一标识、Base64 编码图像和创建时间。
    /// </summary>
    /// <returns>
    /// 一个包含以下信息的 <see cref="CaptchaDto"/>：
    /// <list type="bullet">
    ///   <item><description><c>CaptchaId</c>：验证码唯一标识（32位无横线 GUID）</description></item>
    ///   <item><description><c>ImageBase64</c>：PNG 格式的 Base64 编码图像（data URL 格式）</description></item>
    ///   <item><description><c>CreatedAt</c>：UTC 时间戳</description></item>
    /// </list>
    /// </returns>
    /// <remarks>
    /// <para>
    /// <strong>生成流程：</strong>
    /// 1. 调用 <see cref="GenerateRandomCode"/> 生成 4 位随机字符
    /// 2. 创建 32 位无横线 GUID 作为 <c>CaptchaId</c>
    /// 3. 在后台线程调用 <see cref="DrawCaptchaImage(string)"/> 绘制图像（避免阻塞主线程）
    /// 4. 将图像编码为 PNG 并转为 Base64 data URL
    /// 5. 使用 <see cref="IDistributedCache.SetStringAsync(string, string, DistributedCacheEntryOptions)"/>
    ///    将验证码明文存入 Redis，设置 5 分钟过期时间
    /// </para>
    /// 
    /// <para>
    /// <strong>兼容性说明：</strong>
    /// 当前使用 <c>canvas.DrawText(...)</c> 而非 <c>SKTextBlob</c>，是因为 SkiaSharp v3.119.0 中
    /// <c>SKTextBlobBuilder.Positions</c> 变为只读 Span，无法动态设置单字符偏移。
    /// 故采用逐字符绘制方式，牺牲部分性能换取稳定性。
    /// </para>
    /// </remarks>
    /// <exception cref="OperationCanceledException">缓存操作超时或被取消。</exception>
    public async Task<CaptchaDto> GenerateAsync()
    {
        var code = GenerateRandomCode();
        var captchaId = Guid.NewGuid().ToString("N"); // 32位无横线UUID，紧凑且唯一

        // 图像绘制为 CPU 密集型任务，使用 Task.Run 避免阻塞 I/O 线程
        var imageBytes = await Task.Run(() => DrawCaptchaImage(code));
        var imageBase64 = "data:image/png;base64," + Convert.ToBase64String(imageBytes);

        // 存入分布式缓存，支持自动序列化与过期
        await _redisDb.StringSetAsync(GetCacheKey(captchaId), code, Expiry);

        return new CaptchaDto
        {
            CaptchaId = captchaId,
            ImageBase64 = imageBase64,
            CreatedAt = DateTime.UtcNow
        };
    }

    /// <summary>
    /// 验证用户输入的验证码是否与服务器端存储的匹配。
    /// </summary>
    /// <param name="captchaId">由 <see cref="GenerateAsync"/> 返回的验证码唯一标识。</param>
    /// <param name="inputCode">用户在前端输入的验证码文本。</param>
    /// <returns>
    /// 验证通过返回 <c>true</c>；否则返回 <c>false</c>。
    /// 验证成功后，该验证码将从缓存中移除，防止重放攻击。
    /// </returns>
    /// <remarks>
    /// <para>
    /// <strong>验证逻辑：</strong>
    /// - 输入为空或 null → 返回 false
    /// - 缓存中无此 ID 或已过期 → 返回 false
    /// - 忽略大小写比较（<see cref="StringComparison.OrdinalIgnoreCase"/>）
    /// - 验证成功后立即调用 <see cref="IDistributedCache.RemoveAsync(string)"/> 删除缓存项
    /// </para>
    /// 
    /// <para>
    /// <strong>安全设计：</strong>
    /// 一次性使用机制确保即使验证码被截获也无法重复使用（anti-replay）。
    /// 删除操作异步执行，不影响响应速度。
    /// </para>
    /// </remarks>
    public async Task<bool> ValidateAsync(string captchaId, string inputCode)
    {
        if (string.IsNullOrWhiteSpace(captchaId) || string.IsNullOrWhiteSpace(inputCode))
            return false;

        var cacheKey = GetCacheKey(captchaId);
        var storedCode = await _redisDb.StringGetAsync(cacheKey);

        if (string.IsNullOrWhiteSpace(storedCode))
            return false;

        var isValid = string.Equals(storedCode, inputCode.Trim(), StringComparison.OrdinalIgnoreCase);

        if (isValid)
        {
            // 验证成功后立即删除缓存，防止重放攻击（replay attack）
            // 使用 RemoveAsync 而非 Set + 过期，确保即时失效
            await _redisDb.KeyDeleteAsync(cacheKey);
        }

        return isValid;
    }

    #region 私有辅助方法 (Private Helpers)

    /// <summary>
    /// 生成指定长度的随机验证码字符串。
    /// </summary>
    /// <returns>一个由无歧义字符组成的 4 位大写字母数字组合。</returns>
    /// <remarks>
    /// <para>
    /// 字符集：<c>23456789ABCDEFGHJKLMNPQRSTUVWXYZ</c>
    /// </para>
    /// <list type="bullet">
    ///   <item><description>排除 0 和 O（视觉相似）</description></item>
    ///   <item><description>排除 1、I 和 L（易混淆）</description></item>
    ///   <item><description>排除小写字母（降低 OCR 识别率）</description></item>
    /// </list>
    /// 
    /// <para>
    /// 使用 <see cref="Random.Shared"/> 确保线程安全，避免高并发下 new Random() 因相同种子产生重复序列。
    /// </para>
    /// </remarks>
    private string GenerateRandomCode()
    {
        const string chars = "23456789ABCDEFGHJKLMNPQRSTUVWXYZ";
        var sb = new StringBuilder();
        for (int i = 0; i < CodeLength; i++)
        {
            sb.Append(chars[Random.Shared.Next(chars.Length)]);
        }
        return sb.ToString();
    }

    /// <summary>
    /// 使用 SkiaSharp 绘制验证码图像。
    /// </summary>
    /// <param name="code">要绘制的验证码文本（如 "A7K2"）。</param>
    /// <returns>PNG 格式的图像字节数组。</returns>
    /// <remarks>
    /// <para>
    /// <strong>绘制步骤：</strong>
    /// 1. 创建 150x60 的透明画布，填充白色背景
    /// 2. 添加 10 个浅灰色随机圆点作为背景装饰（颜色范围 200-255，避免干扰文字）
    /// 3. 使用 Arial Bold 字体逐字符绘制文本，每个字符有随机偏移（-5~+5px X, -8~+8px Y）
    /// 4. 添加 6 条中灰色干扰线（RGB 100-150）
    /// 5. 添加 60 个随机分布的灰色噪点
    /// 6. 导出为 PNG（质量 100%）
    /// </para>
    /// 
    /// <para>
    /// <strong>抗 OCR 技巧：</strong>
    /// - 字符随机偏移模拟手写抖动
    /// - 干扰线与噪点增加图像复杂度
    /// - 背景圆不规则分布防止模板匹配
    /// </para>
    /// </remarks>
    private byte[] DrawCaptchaImage(string code)
    {
        // 1. 创建绘图表面 (Surface) 和画布 (Canvas)
        using var surface = SKSurface.Create(new SKImageInfo(Width, Height));
        using var canvas = surface.Canvas;

        // 清空画布为白色背景
        canvas.Clear(SKColors.White);

        // ------------------------
        // 2. 背景装饰：绘制浅色随机圆
        // ------------------------
        using var bgPaint = new SKPaint
        {
            Style = SKPaintStyle.Fill,
            IsAntialias = true // 背景圆也开启抗锯齿，更平滑
        };

        for (int i = 0; i < 10; i++)
        {
            // 生成随机浅灰色/彩色
            bgPaint.Color = new SKColor(
                (byte)Random.Shared.Next(200, 255),
                (byte)Random.Shared.Next(200, 255),
                (byte)Random.Shared.Next(200, 255)
            );

            canvas.DrawCircle(
                Random.Shared.Next(Width),
                Random.Shared.Next(Height),
                Random.Shared.Next(15, 30),
                bgPaint
            );
        }

        // ------------------------
        // 3. 文本绘制 (核心修复部分)
        // ------------------------

        // A. 加载字体文件 (Typeface)
        using var typeface = SKTypeface.FromFamilyName(
            "Arial",
            weight: SKFontStyleWeight.Bold,
            width: SKFontStyleWidth.Normal,
            slant: SKFontStyleSlant.Upright
        );

        // B. 创建 SKFont 对象并设置所有字体属性
        // ✅ 修复：原本在 SKPaint 上的 TextSize, Typeface, TextEncoding 现在都在 SKFont 上
        using var font = new SKFont(typeface, 36)
        {
            Edging = SKFontEdging.Antialias,      // 边缘抗锯齿
        };

        // C. 创建 SKPaint 对象，仅用于控制颜色和渲染风格
        // ✅ 修复：移除了 TextSize, Typeface, TextEncoding 属性
        using var textPaint = new SKPaint
        {
            Color = SKColors.Black,
            IsAntialias = true,
            // 不再需要设置字体相关属性，它们由 'font' 对象接管
        };

        float xPosition = 30; // 起始 X 偏移
        float yPosition = 45; // 基准 Y 位置 (基线)

        foreach (var c in code)
        {
            // 模拟手写抖动：X 轴轻微偏移，Y 轴较大偏移
            float x = xPosition + Random.Shared.Next(-5, 5);
            float y = yPosition + Random.Shared.Next(-8, 8);

            string charStr = c.ToString();

            // ✅ 修复：使用新的 DrawText 重载
            // 参数顺序：(文本, X, Y, 对齐方式, 字体对象, 画笔对象)
            // SKTextAlign.Left 确保 x 坐标是字符的左边缘，符合原有逻辑
            canvas.DrawText(charStr, x, y, SKTextAlign.Left, font, textPaint);

            // 计算下一个字符的位置
            // ✅ 修复：MeasureText 现在是 SKFont 的方法
            float charWidth = font.MeasureText(charStr);
            xPosition += charWidth + Random.Shared.Next(-2, 6);
        }

        // ------------------------
        // 4. 干扰线
        // ------------------------
        using var linePaint = new SKPaint
        {
            StrokeWidth = 1.5f,
            IsAntialias = true,
            Style = SKPaintStyle.Stroke // 明确指定为描边模式
        };

        for (int i = 0; i < 6; i++)
        {
            linePaint.Color = new SKColor(
                (byte)Random.Shared.Next(100, 150),
                (byte)Random.Shared.Next(100, 150),
                (byte)Random.Shared.Next(100, 150)
            );

            canvas.DrawLine(
                Random.Shared.Next(Width), Random.Shared.Next(Height),
                Random.Shared.Next(Width), Random.Shared.Next(Height),
                linePaint
            );
        }

        // ------------------------
        // 5. 噪点 (优化：复用 Paint 对象)
        // ------------------------
        using var pointPaint = new SKPaint
        {
            IsAntialias = false, // 噪点通常不需要抗锯齿，性能更好
            StrokeCap = SKStrokeCap.Round // 让点看起来更圆润
        };

        for (int i = 0; i < 60; i++)
        {
            pointPaint.Color = new SKColor(
                (byte)Random.Shared.Next(100, 150),
                (byte)Random.Shared.Next(100, 150),
                (byte)Random.Shared.Next(100, 150)
            );

            // DrawPoint 实际上是一个极短的线段或点，取决于 StrokeCap
            canvas.DrawPoint(
                Random.Shared.Next(Width),
                Random.Shared.Next(Height),
                pointPaint
            );
        }

        // ------------------------
        // 6. 导出为 PNG 字节数组
        // ------------------------
        using var image = surface.Snapshot();
        using var data = image.Encode(SKEncodedImageFormat.Png, 100); // 质量 100

        return data.ToArray();
    }

    /// <summary>
    /// 构造 Redis 缓存键，支持多实例隔离。
    /// </summary>
    /// <param name="captchaId">验证码唯一标识。</param>
    /// <returns>格式为 {InstanceName}:captcha:{CaptchaId} 的缓存键。</returns>
    /// <example>
    /// 若 InstanceName = "prod"，CaptchaId = "abc123"，则返回 "prod:captcha:abc123"
    /// </example>
    private string GetCacheKey(string captchaId) => $"{_instanceName}:captcha:{captchaId}";

    #endregion
}