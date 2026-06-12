using Bedrock.Application.ValueObjects;
using System;
using System.Threading.Tasks;

namespace Bedrock.Application.Interfaces
{
    /// <summary>
    /// 🚀 认证令牌管理仓储接口（适配 SessionInfoVO / RefreshTokenMetadataVO）
    /// 定义访问令牌（AccessToken）与刷新令牌（RefreshToken）的持久化、验证、清理与撤销契约。
    /// 
    /// 🌟 核心能力：
    /// - ✅ 验证 AccessToken 有效性（支持惰性清理过期会话）
    /// - ✅ 验证 RefreshToken 有效性（返回完整元数据，便于设备管理与安全审计）
    /// - ✅ 撤销单个 RefreshToken（清理元数据 + 用户集合）
    /// - ✅ 批量撤销用户所有令牌（强制登出 / 修改密码 / 安全风控）
    /// - ✅ 创建或更新令牌对（AccessToken + RefreshToken）
    /// 
    /// 🔐 安全设计契约：
    /// - 所有操作必须隔离用户数据（通过 UserId 隔离）
    /// - refreshToken 不暴露 userId 于外部，而是封装在 RefreshTokenMetadataVO 中
    /// - 支持设备/IP/UserAgent 扩展字段，便于“我的设备”管理
    /// 
    /// ⚡ 性能契约：
    /// - 实现层应支持批量操作（MGET / KeyDeleteAsync）减少 Redis 往返
    /// - 支持惰性清理，避免额外定时任务开销
    /// - Fire-and-forget 异步清理不阻塞主请求路径
    /// 
    /// 💡 设计原则：
    /// - 单一职责：只负责令牌存储与验证，不负责 JWT 签发或权限校验
    /// - 可扩展性：支持未来添加 LastAccessedAt、设备绑定、自动踢出等功能
    /// - 命名空间隔离：实现层必须使用 {InstanceName} 避免多环境冲突
    /// 
    /// 📌 Redis Key 设计建议（供实现层参考）：
    /// - AccessToken 会话：{InstanceName}:{UserId}:loginSessions:{SessionId} → SessionInfoVO
    /// - 用户会话集合：{InstanceName}:{UserId}:userSessions → Set of SessionId
    /// - RefreshToken 元数据：{InstanceName}:refreshToken:{RefreshToken} → RefreshTokenMetadataVO
    /// - 用户 refreshToken 集合：{InstanceName}:{UserId}:refreshTokens → Set of RefreshToken (字符串)
    /// </summary>
    public interface IAuthRepository
    {
        /// <summary>
        /// 🔄 验证访问令牌（AccessToken）的有效性。
        /// 实现层应在验证过程中执行“惰性清理”：自动移除已过期或损坏的会话数据。
        /// 
        /// ⚡ 性能建议：使用 MGET 批量读取所有会话，避免逐个查询。
        /// 🧹 清理策略：发现过期或无效会话 → 异步从 Set 和 Key 中移除（Fire-and-forget）。
        /// 
        /// 📌 注意：不更新 LastAccessedAt —— 如需“活跃会话”功能，应在 API 中间件中单独更新。
        /// </summary>
        /// <param name="userId">用户唯一标识（long 类型）。</param>
        /// <param name="accessToken">待验证的访问令牌字符串。</param>
        /// <returns>若找到匹配且未过期的会话，返回 true；否则返回 false。</returns>
        Task<bool> ValidateAccessTokenAsync(long userId, string accessToken);

        /// <summary>
        /// 🔄 验证刷新令牌（RefreshToken）的有效性，并返回其完整元数据（RefreshTokenMetadataVO）。
        /// 
        /// 🆕 升级点：不再只返回 userId，而是返回完整元数据（设备/IP/创建时间等），便于安全审计和设备管理。
        /// ⏳ 自动过期：依赖实现层的 TTL 机制，无需手动检查 ExpiresAt。
        /// 
        /// 📌 注意：如需记录“最后使用时间”，应在刷新令牌接口中手动更新 LastUsedAt 并重新持久化。
        /// </summary>
        /// <param name="userId">用户唯一标识（long 类型）。</param>
        /// <param name="refreshTokenId">待验证的刷新令牌的唯一标识。</param>
        /// <returns>若令牌有效，返回 RefreshTokenMetadataVO；否则返回 null。</returns>
        Task<RefreshTokenMetaVO?> ValidateRefreshTokenAsync(long userId, string refreshTokenId); // ✅ 返回值升级！

        /// <summary>
        /// 🗑️ 撤销指定的刷新令牌（RefreshToken）。
        /// 同时从用户 refreshToken 集合中移除，并删除其元数据。
        /// 
        /// 🔄 流程契约：
        /// 1. 获取 refreshToken 对应的元数据（含 UserId）
        /// 2. 删除 refreshToken 元数据 Key
        /// 3. 从用户 refreshToken 集合中移除该令牌
        /// 
        /// 📌 安全契约：即使元数据已过期，也应尝试清理集合残留（防御性编程）。
        /// </summary>
        /// <param name="userId">用户唯一标识（long 类型）。</param>
        /// <param name="refreshTokenId">要撤销的刷新令牌的唯一标识。</param>
        Task RevokeRefreshTokenAsync(long userId, string refreshTokenId);

        /// <summary>
        /// 🧨 撤销指定用户的所有令牌（包括所有 AccessToken 会话和 RefreshToken）。
        /// 用于“强制登出”、“修改密码后踢出”、“安全风控”等场景。
        /// 
        /// 🔄 清理流程契约：
        /// 1. 获取用户所有 refreshToken → 批量删除其元数据 Key
        /// 2. 删除用户 refreshToken 集合 Key
        /// 3. 获取用户所有会话 ID → 批量删除会话数据 Key
        /// 4. 删除用户会话集合 Key
        /// 
        /// ⚡ 性能契约：应使用批量 KeyDeleteAsync，避免循环单次删除。
        /// </summary>
        /// <param name="userId">用户唯一标识（long 类型）。</param>
        Task RevokeAllTokensForUserAsync(long userId);

        /// <summary>
        /// 🆕 创建或更新用户的访问令牌（AccessToken）和刷新令牌（RefreshToken）。
        /// 同时持久化 SessionInfoVO 和 RefreshTokenMetadataVO。
        /// 
        /// 🔄 存储结构契约：
        /// - AccessToken 会话 → SessionInfoVO → Key: {InstanceName}:{UserId}:loginSessions:{SessionId}
        /// - RefreshToken 元数据 → RefreshTokenMetadataVO → Key: {InstanceName}:refreshToken:{RefreshToken}
        /// - 用户会话集合 → Set Add SessionId
        /// - 用户 refreshToken 集合 → Set Add RefreshToken
        /// 
        /// ⏳ 自动过期：根据 expiresAt / refreshTokenExpiresAt 设置 Key TTL。
        /// 📌 扩展建议：在 Controller 或 Middleware 中补充 Device / IpAddress / UserAgent。
        /// </summary>
        /// <param name="userId">用户唯一标识。</param>
        /// <param name="newAccessToken">新签发的访问令牌。</param>
        /// <param name="newRefreshTokenId">新签发的刷新令牌的唯一标识。</param>
        /// <param name="loginTime">登录/令牌创建时间（UTC）。</param>
        /// <param name="expiresAt">访问令牌过期时间（UTC）。</param>
        /// <param name="refreshTokenExpiresAt">刷新令牌过期时间（UTC）。</param>
        /// <param name="clientInfoVO">客户端信息（Device / IpAddress / UserAgent）。</param>
        Task UpdateTokenAsync(
            long userId,
            string newAccessToken,
            string newRefreshTokenId,
            DateTime loginTime,
            DateTime expiresAt,
            DateTime refreshTokenExpiresAt,
            ClientInfoVO clientInfoVO);
    }
}