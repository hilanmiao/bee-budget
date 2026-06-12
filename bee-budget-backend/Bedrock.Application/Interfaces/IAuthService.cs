using Bedrock.Application.DataTransferObjects;
using Bedrock.Application.ValueObjects;
using System.Security.Claims;

namespace Bedrock.Application.Interfaces
{
    /// <summary>
    /// 🔐 认证服务接口契约 —— 定义用户登录、令牌管理、权限获取等完整认证生命周期操作。
    /// 
    /// 🌟 核心能力契约：
    /// - ✅ AuthenticateAsync：支持三重验证（用户+密码+验证码）→ 签发 AccessToken + RefreshToken
    /// - ✅ RefreshTokenAsync：安全轮换机制 → 旧Token立即撤销，新Token全新生成
    /// - ✅ RevokeRefreshTokenAsync：支持单设备登出
    /// - ✅ ForceLogoutAsync：支持全设备踢出（改密/风控/管理员操作）
    /// - ✅ ValidateAccessTokenAsync：中间件/网关层调用，验证请求合法性
    /// - ✅ GetAuthInfoAsync：前端动态权限渲染（角色+权限集合）
    /// 
    /// 🔐 安全设计契约（实现者必须遵守）：
    /// - 所有 RefreshToken 操作必须验证有效性 → 获取 UserId（通过 RefreshTokenMetadataVO）
    /// - 登录/刷新时应记录 Device/IP/UserAgent（虽不体现在接口，但实现层应支持）
    /// - 撤销操作必须幂等（重复调用不报错）
    /// - 密码错误必须统一提示“用户名或密码错误”，防止枚举攻击
    /// 
    /// 💡 扩展设计引导（未来增强方向）：
    /// - 可增加 GetActiveSessionsAsync(userId) → 返回设备/IP/登录时间列表
    /// - 可增加 RevokeSessionAsync(sessionId) → 精准踢出某设备
    /// - 可增加 SetTokenTTL(userId, minutes) → 临时缩短某用户令牌有效期（用于风控）
    /// 
    /// 🔄 与前端协作协议：
    /// 1. 登录 → 存储 AccessToken + RefreshToken
    /// 2. 请求携带 Bearer Token
    /// 3. 401 → 调用 RefreshToken → 失败则跳转登录页
    /// 4. 前端应实现“自动刷新”机制，避免用户感知中断
    /// </summary>
    public interface IAuthService
    {
        /// <summary>
        /// 🔐 用户登录认证并签发新令牌对（AccessToken + RefreshToken）。
        /// 
        /// 🔄 流程契约：
        /// 1. 验证码校验 → 2. 用户名校验 → 3. 密码哈希比对 → 4. 生成新令牌 → 5. 持久化
        /// 
        /// ⚠️ 安全契约：
        /// - 密码错误必须统一返回“用户名或密码错误”
        /// - 验证码错误应独立提示
        /// - 建议记录登录设备/IP/UserAgent（实现层扩展）
        /// 
        /// 💡 扩展建议：
        /// - LoginDto 可扩展为支持手机号/邮箱/第三方登录
        /// </summary>
        /// <param name="loginDto">登录请求数据（含用户名、密码、验证码）</param>
        /// <param name="clientInfoVO">客户端信息（设备、IP、UserAgent）</param>
        /// <returns>包含 AccessToken、RefreshToken、ExpiresAt 的响应对象</returns>
        /// <exception cref="ArgumentException">参数缺失或验证码错误</exception>
        /// <exception cref="SecurityTokenException">内部令牌签发失败</exception>
        Task<LoginResponseDto> AuthenticateAsync(LoginDto loginDto, ClientInfoVO clientInfoVO);

        /// <summary>
        /// 🔄 使用 RefreshToken 刷新获取新令牌对。
        /// 
        /// 🔄 流程契约：
        /// 1. 验证 RefreshToken 有效性 → 获取 UserId
        /// 2. 生成新 AccessToken + 新 RefreshToken
        /// 3. 撤销旧 RefreshToken
        /// 4. 持久化新令牌对
        /// 
        /// 🔐 安全契约：
        /// - 旧 RefreshToken 必须立即撤销（防重放）
        /// - 新 RefreshToken 必须全新生成（不复用）
        /// - 建议记录“刷新设备/IP”用于安全审计
        /// </summary>
        /// <param name="userId">用户唯一标识（long 类型）。</param>
        /// <param name="refreshTokenId">客户端提供的刷新令牌的唯一标识</param>
        /// <param name="clientInfoVO">客户端信息（设备、IP、UserAgent）</param>
        /// <returns>包含新 AccessToken、新 RefreshTokenId、新 ExpiresAt 的响应对象</returns>
        /// <exception cref="ArgumentException">RefreshTokenId 为空</exception>
        /// <exception cref="SecurityTokenException">RefreshToken 无效或过期</exception>
        /// <exception cref="KeyNotFoundException">用户不存在</exception>
        Task<LoginResponseDto> RefreshTokenAsync(long userId, string refreshTokenId, ClientInfoVO clientInfoVO);

        /// <summary>
        /// ✅ 验证访问令牌（AccessToken）是否有效。
        /// 用于 API 中间件或网关层校验请求合法性。
        /// 
        /// 🔄 流程契约：
        /// - 支持惰性清理过期会话
        /// - 不更新 LastAccessedAt（如需活跃会话功能，应在中间件单独更新）
        /// </summary>
        /// <param name="userId">从 JWT Claims 中解析出的用户ID</param>
        /// <param name="accessToken">请求头中的 Bearer Token</param>
        /// <returns>令牌有效返回 true，否则 false</returns>
        /// <exception cref="ArgumentException">参数无效</exception>
        Task<bool> ValidateAccessTokenAsync(long userId, string accessToken);

        /// <summary>
        /// 🗑️ 撤销指定的 RefreshToken。
        /// 用于用户主动登出或安全风控场景。
        /// 
        /// 🔐 安全契约：
        /// - 必须先验证有效性 → 获取 UserId → 再执行撤销
        /// - 撤销操作必须幂等（重复调用不报错）
        /// </summary>
        /// <param name="userId">用户唯一标识（long 类型）。</param>
        /// <param name="refreshTokenId">要撤销的刷新令牌的唯一标识</param>
        /// <exception cref="ArgumentException">RefreshTokenId 为空</exception>
        /// <exception cref="SecurityTokenException">RefreshToken 无效</exception>
        Task RevokeRefreshTokenAsync(long userId, string refreshTokenId);

        /// <summary>
        /// 🧨 强制登出：撤销指定用户的所有活跃令牌。
        /// 用于“修改密码后踢出”、“管理员强制下线”、“安全风控”等场景。
        /// 
        /// ⚡ 性能契约：
        /// - 实现层应使用批量删除提升性能
        /// - 不阻塞主请求路径（Fire-and-forget 可选）
        /// </summary>
        /// <param name="userId">用户唯一标识</param>
        /// <exception cref="ArgumentException">UserId 无效</exception>
        /// <exception cref="KeyNotFoundException">用户不存在</exception>
        Task ForceLogoutAsync(long userId);

        /// <summary>
        /// 📋 获取用户授权信息（角色 + 权限）。
        /// 用于前端“动态路由”、“按钮权限”、“菜单渲染”。
        /// 
        /// 🌟 权限规则契约：
        /// - 角色含 "admin" → 返回 ["*:*:*"]（超级管理员）
        /// - 否则 → 返回菜单权限集合（如 "user:list", "role:create"）
        /// 
        /// ⚡ 性能建议：
        /// - 建议实现层增加缓存（如 MemoryCache / Redis）→ Key: authinfo:{userId}
        /// - 缓存过期策略：用户登出时清除，或定时刷新
        /// </summary>
        /// <param name="userId">用户ID</param>
        /// <returns>包含用户信息、角色列表、权限列表的 DTO</returns>
        /// <exception cref="ArgumentException">UserId 无效</exception>
        /// <exception cref="KeyNotFoundException">用户不存在</exception>
        Task<AuthInfoDto> GetAuthInfoAsync(long userId);
    }
}