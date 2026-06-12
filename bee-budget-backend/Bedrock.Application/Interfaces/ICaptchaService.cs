using Bedrock.Application.DataTransferObjects;

namespace Bedrock.Application.Interfaces
{
    /// <summary>
    /// 图形验证码服务接口，用于生成和验证一次性验证码。
    /// </summary>
    public interface ICaptchaService
    {
        /// <summary>
        /// 生成一个新的图形验证码。
        /// </summary>
        /// <returns>
        /// 返回包含验证码唯一标识（CaptchaId）、Base64 编码图像和创建时间的 <see cref="CaptchaDto"/>。
        /// 图像格式为 PNG，以 data URL 形式返回（data:image/png;base64,...）。
        /// </returns>
        /// <remarks>
        /// 生成的验证码将存储在分布式缓存中，默认有效期为 5 分钟。
        /// 验证码字符由无歧义字符集组成（如：23456789ABCDEFGHJKLMNPQRSTUVWXYZ），避免 0/O、1/I/L 等易混淆字符。
        /// </remarks>
        Task<CaptchaDto> GenerateAsync();

        /// <summary>
        /// 验证用户输入的验证码是否正确。
        /// </summary>
        /// <param name="captchaId">验证码的唯一标识符（通常由 GenerateAsync 返回）。</param>
        /// <param name="inputCode">用户输入的验证码文本。</param>
        /// <returns>
        /// 验证成功返回 true；否则返回 false。
        /// 验证成功后，该验证码将从缓存中移除，防止重放攻击。
        /// </returns>
        /// <remarks>
        /// 验证过程忽略大小写。
        /// 如果 captchaId 不存在或已过期，返回 false。
        /// 输入为空或 null 时，直接返回 false。
        /// </remarks>
        Task<bool> ValidateAsync(string captchaId, string inputCode);
    }
}

