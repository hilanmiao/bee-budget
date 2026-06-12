using Bedrock.Application.Interfaces;
using Bedrock.Application.ValueObjects;
using Bedrock.Configuration;
using Bedrock.Application.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StackExchange.Redis;
using System.Text.Json;

namespace Bedrock.Infrastructure.Repositories
{
    /// <summary>
    /// 🚀 基于 Redis 的认证令牌存储与管理仓库（适配 SessionInfoVO / RefreshTokenMetaVO）
    /// 负责访问令牌（AccessToken）与刷新令牌（RefreshToken）的持久化、验证、清理与撤销。
    /// 
    /// 🌟 核心能力升级：
    /// - ✅ 使用 Redis 存储 AccessToken 会话信息（SessionInfoVO）和 RefreshToken 元数据（RefreshTokenMetaVO）
    /// - ✅ 支持 AccessToken 有效性验证 + 惰性清理过期会话（性能优化）
    /// - ✅ 支持 RefreshToken 验证（返回完整元数据）与安全撤销
    /// - ✅ 支持按用户批量撤销所有活跃令牌（登出/风控）
    /// - ✅ 使用 Redis Set 管理会话与 refreshToken 集合，支持高效查询与清理
    /// - ✅ 使用 MGET / KeyDeleteAsync 等批量操作，提升高并发吞吐量
    /// 
    /// 🔐 安全设计：
    /// - 所有 Key 使用命名空间隔离：{InstanceName}:{UserId}:xxx，避免多租户/环境冲突
    /// - refreshToken 不直接暴露 userId，而是封装在 RefreshTokenMetaVO 中，增强安全性
    /// - 支持设备/IP/UserAgent 记录，便于“我的设备”管理和异地登录告警
    /// 
    /// ⚡ 性能优化：
    /// - 惰性清理：在验证时顺带清理过期会话，避免额外定时任务开销
    /// - 批量读写：减少 Redis 往返次数，提升并发性能
    /// - Fire-and-forget 异步清理：不阻塞主请求路径
    /// 
    /// 💡 设计亮点：
    /// - 会话（Session）与令牌元数据（Metadata）职责分离，结构清晰
    /// - 支持未来扩展：如 LastAccessedAt 更新、设备绑定、自动踢出等
    /// - 不负责 JWT 签发、用户登录、权限校验 —— 专注存储与验证，符合单一职责
    /// 
    /// 📌 Redis Key 设计规范：
    /// - AccessToken 会话：{InstanceName}:{UserId}:sessions:{SessionId} → 存储 SessionInfoVO
    /// - 用户会话集合：{InstanceName}:{UserId}:userSessions → Set of SessionId
    /// - RefreshToken 元数据：{InstanceName}:{userId}:refreshTokens:{RefreshTokenId} → 存储 RefreshTokenMetaVO
    /// - 用户 refreshToken 集合：{InstanceName}:{UserId}:userRefreshTokens → Set of RefreshTokenId (字符串)
    /// </summary>
    public class AuthRepository : IAuthRepository
    {
        private readonly ILogger<AuthRepository> _logger;
        private readonly IConnectionMultiplexer _redis;
        private readonly IDatabase _redisDb;
        private readonly string _instanceName;

        /// <summary>
        /// 构造函数注入 Redis 连接与配置。
        /// </summary>
        /// <param name="logger">日志记录器。</param>
        /// <param name="redis">Redis 连接多路复用器，用于获取数据库实例。</param>
        /// <param name="redisConfig">Redis 配置选项，包含实例名称（用于 Key 命名空间隔离）。</param>
        public AuthRepository(ILogger<AuthRepository> logger, IConnectionMultiplexer redis, IOptions<RedisConfig> redisConfig)
        {
            _logger = logger;
            _redis = redis ?? throw new ArgumentNullException(nameof(redis));
            _redisDb = _redis.GetDatabase();
            _instanceName = redisConfig?.Value?.InstanceName ?? throw new ArgumentNullException(nameof(redisConfig));
        }

        /// <summary>
        /// 🔄 验证访问令牌（AccessToken）的有效性，并在验证过程中执行“惰性清理”：
        /// 自动移除已过期或损坏的会话数据，避免积累垃圾数据。
        /// 
        /// ⚡ 性能优化：使用 MGET 批量读取所有会话，避免逐个查询 Redis。
        /// 🧹 清理策略：发现过期或无效会话 → 异步从 Set 和 Key 中移除（Fire-and-forget）。
        /// 
        /// 📌 注意：不更新 LastAccessedAt —— 如需“活跃会话”功能，应在 API 中间件中单独更新。
        /// </summary>
        /// <param name="userId">用户唯一标识（long 类型）。</param>
        /// <param name="accessToken">待验证的访问令牌字符串。</param>
        /// <returns>若找到匹配且未过期的会话，返回 true；否则返回 false。</returns>
        public async Task<bool> ValidateAccessTokenAsync(long userId, string accessToken)
        {
            if (string.IsNullOrWhiteSpace(accessToken)) return false;

            var userSessionsSetKey = $"{_instanceName}:{userId}:userSessions"; // 用户会话 ID 集合 Key
            var sessionIds = await _redisDb.SetMembersAsync(userSessionsSetKey); // 获取所有活跃会话 ID

            var userRefreshTokenSetKey = $"{_instanceName}:{userId}:userRefreshTokens"; // 用户 refreshToken ID 集合 Key
            var refreshTokenIds = await _redisDb.SetMembersAsync(userRefreshTokenSetKey); // 获取所有活跃 refreshToken ID

            if (!sessionIds.Any()) return false; // 无任何会话，直接返回

            // 🚀 批量构建会话缓存 Key 并一次性读取（MGET）
            var sessionKeys = sessionIds.Select(id => (RedisKey)$"{_instanceName}:{userId}:sessions:{id}").ToArray();
            var sessionValues = await _redisDb.StringGetAsync(sessionKeys); // 批量获取

            var refreshTokenKeys = refreshTokenIds.Select(id => (RedisKey)$"{_instanceName}:{userId}:refreshTokens:{id}").ToArray();
            var refreshTokenValues = await _redisDb.StringGetAsync(refreshTokenKeys); // 批量获取

            var cleanupTasks = new List<Task>(); // 收集清理任务
            bool tokenValid = false;

            for (int i = 0; i < sessionValues.Length; i++)
            {
                // StackExchange.Redis 保证 MGET 返回顺序与输入 keys 顺序一致，所以 redisValues[i] 对应 sessionIds[i] 是安全的
                var redisValue = sessionValues[i];
                var sessionId = sessionIds[i].ToString();

                if (!redisValue.IsNullOrEmpty)
                {
                    var sessionInfoString = redisValue.ToString();
                    var sessionInfo = JsonSerializer.Deserialize<SessionInfoVO>(sessionInfoString); // ✅ 使用新 VO

                    // ✅ 情况1：当前会话有效且 AccessToken 匹配 → 标记为有效（不立即返回，先完成清理）
                    if (sessionInfo != null &&
                        sessionInfo.AccessToken == accessToken &&
                        DateTime.UtcNow < sessionInfo.ExpiresAt)
                    {
                        tokenValid = true;
                    }

                    // 🧹 情况2：会话已过期 或 反序列化失败 → 触发惰性清理
                    if (sessionInfo == null || DateTime.UtcNow >= sessionInfo.ExpiresAt)
                    {
                        var sessionKey = $"{_instanceName}:{userId}:sessions:{sessionId}";
                        cleanupTasks.Add(Task.WhenAll(
                            _redisDb.SetRemoveAsync(userSessionsSetKey, sessionId), // 从集合中移除
                            _redisDb.KeyDeleteAsync(sessionKey)                 // 删除会话数据
                        ));
                    }
                }
                else
                {
                    // 🧹 redisValue 为空：缓存已过期但 sessionId 仍在集合中 → 清理残留
                    cleanupTasks.Add(_redisDb.SetRemoveAsync(userSessionsSetKey, sessionId));
                }
            }

            for (int i = 0; i < refreshTokenValues.Length; i++)
            {
                // StackExchange.Redis 保证 MGET 返回顺序与输入 keys 顺序一致，所以 redisValues[i] 对应 refreshTokenIds[i] 是安全的
                var redisValue = refreshTokenValues[i];
                var refreshTokenId = refreshTokenIds[i].ToString();

                if (!redisValue.IsNullOrEmpty)
                {
                    var refreshTokenString = redisValue.ToString();
                    var refreshTokenMeta = JsonSerializer.Deserialize<RefreshTokenMetaVO>(refreshTokenString); // ✅ 使用新 VO

                    // 🧹 情况2：refreshToken已过期 或 反序列化失败 → 触发惰性清理
                    if (refreshTokenMeta == null || DateTime.UtcNow >= refreshTokenMeta.ExpiresAt)
                    {
                        var refreshTokenKey = $"{_instanceName}:{userId}:refreshTokens:{refreshTokenId}";
                        cleanupTasks.Add(Task.WhenAll(
                            _redisDb.SetRemoveAsync(userRefreshTokenSetKey, refreshTokenId), // 从集合中移除
                            _redisDb.KeyDeleteAsync(refreshTokenKey)                 // 删除refreshToken数据
                        ));
                    }
                }
                else
                {
                    // 🧹 redisValue 为空：缓存已过期但 refreshTokenId 仍在集合中 → 清理残留
                    cleanupTasks.Add(_redisDb.SetRemoveAsync(userRefreshTokenSetKey, refreshTokenId));
                }
            }

            // 🚀 并发执行所有清理任务（Fire-and-forget，不阻塞主流程）
            if (cleanupTasks.Any())
            {
                _ = Task.Run(async () =>
                {
                    try
                    {
                        await Task.WhenAll(cleanupTasks);
                    }
                    catch (Exception ex)
                    {
                        // 📝 建议：注入 ILogger 记录清理失败日志
                        _logger.LogWarning(ex, "惰性清理过期会话失败，用户ID: {UserId}", userId);
                    }
                });
            }

            return tokenValid;
        }

        /// <summary>
        /// 🔄 验证刷新令牌（RefreshToken）的有效性，并返回其完整元数据（RefreshTokenMetaVO）。
        /// 
        /// 🆕 升级点：不再只返回 userId，而是返回完整元数据（设备/IP/创建时间等），便于安全审计和设备管理。
        /// ⏳ 自动过期：依赖 Redis Key TTL，无需手动检查 ExpiresAt。
        /// 
        /// 📌 注意：如需记录“最后使用时间”，应在刷新令牌接口中手动更新 LastUsedAt 并重新 Set。
        /// </summary>
        /// <param name="userId">用户唯一标识（long 类型）。</param>
        /// <param name="refreshTokenId">待验证的刷新令牌的唯一标识。</param>
        /// <returns>若令牌有效，返回 RefreshTokenMetaVO；否则返回 null。</returns>
        public async Task<RefreshTokenMetaVO?> ValidateRefreshTokenAsync(long userId, string refreshTokenId)
        {
            if (string.IsNullOrWhiteSpace(refreshTokenId)) return null;

            var cacheKey = $"{_instanceName}:{userId}:refreshTokens:{refreshTokenId}"; // refreshToken 元数据 Key
            var redisValue = await _redisDb.StringGetAsync(cacheKey);

            if (redisValue.IsNullOrEmpty) // RedisValue 有专门的 IsNullOrEmpty 属性，比转 string 更高效
            {
                return null;
            }
            string jsonString = redisValue.ToString();

            return JsonSerializer.Deserialize<RefreshTokenMetaVO>(jsonString); // ✅ 返回完整元数据
        }

        /// <summary>
        /// 🗑️ 撤销指定的刷新令牌（RefreshToken）。
        /// 同时从用户 refreshToken 集合中移除，并删除其元数据。
        /// 
        /// 🔄 流程：
        /// 1. 验证 refreshToken 获取元数据（含 UserId）
        /// 2. 删除 refreshToken 元数据 Key
        /// 3. 从用户 refreshToken 集合中移除该令牌
        /// 
        /// 📌 安全设计：即使元数据已过期，也尝试清理集合残留（防御性编程）。
        /// </summary>
        /// <param name="userId">用户唯一标识（long 类型）。</param>
        /// <param name="refreshTokenId">要撤销的刷新令牌的唯一标识。</param>
        public async Task RevokeRefreshTokenAsync(long userId, string refreshTokenId)
        {
            if (string.IsNullOrWhiteSpace(refreshTokenId)) return;

            //var metadata = await ValidateRefreshTokenAsync(userId, refreshTokenId); // ✅ 获取完整元数据
            var cacheKey = $"{_instanceName}:{userId}:refreshTokens:{refreshTokenId}";

            // 🗑️ 删除元数据（即使 metadata 为 null 也尝试删除，清理残留）
            await _redisDb.KeyDeleteAsync(cacheKey);

            // 🗑️ 从用户 refreshToken 集合中移除（即使 metadata 为 null，也尝试清理）
            //if (metadata != null)
            //{
            var refreshTokenSetKey = $"{_instanceName}:{userId}:userRefreshTokens";
            await _redisDb.SetRemoveAsync(refreshTokenSetKey, refreshTokenId); // ✅ 存的是 refreshTokenId
            //}
        }

        /// <summary>
        /// 🧨 撤销指定用户的所有令牌（包括所有 AccessToken 会话和 RefreshToken）。
        /// 用于“强制登出”、“修改密码后踢出”、“安全风控”等场景。
        /// 
        /// 🔄 清理流程：
        /// 1. 获取用户所有 refreshToken → 批量删除其元数据 Key
        /// 2. 删除用户 refreshToken 集合 Key
        /// 3. 获取用户所有会话 ID → 批量删除会话数据 Key
        /// 4. 删除用户会话集合 Key
        /// 
        /// ⚡ 性能优化：使用批量 KeyDeleteAsync，避免循环单次删除。
        /// </summary>
        /// <param name="userId">用户唯一标识（long 类型）。</param>
        public async Task RevokeAllTokensForUserAsync(long userId)
        {
            // ========== 1️⃣ 删除所有 RefreshToken 相关数据 ==========
            var refreshTokenSetKey = $"{_instanceName}:{userId}:userRefreshTokens";
            var refreshTokenIds = await _redisDb.SetMembersAsync(refreshTokenSetKey);

            if (refreshTokenIds.Length > 0)
            {
                // 🚀 批量构建 refreshToken 元数据 Key（格式：{instanceName}:refreshTokens:{token}）
                var refreshTokenCacheKeys = refreshTokenIds
                    .Select(refreshTokenId => (RedisKey)$"{_instanceName}:{userId}:refreshTokens:{refreshTokenId}")
                    .ToArray();

                // 🚀 批量删除所有 refreshToken 元数据
                await _redisDb.KeyDeleteAsync(refreshTokenCacheKeys);
            }
            // 🗑️ 删除 refreshToken 集合本身（无论是否有成员，安全删除）
            await _redisDb.KeyDeleteAsync(refreshTokenSetKey);

            // ========== 2️⃣ 删除所有登录会话相关数据 ==========
            var sessionsSetKey = $"{_instanceName}:{userId}:userSessions";
            var sessionIds = await _redisDb.SetMembersAsync(sessionsSetKey);

            if (sessionIds.Length > 0)
            {
                // 🚀 批量构建会话缓存 Key（格式：{instanceName}:{userId}:sessions:{sessionId}）
                var sessionCacheKeys = sessionIds
                    .Select(sessionId => (RedisKey)$"{_instanceName}:{userId}:sessions:{sessionId}")
                    .ToArray();

                // 🚀 批量删除所有会话数据
                await _redisDb.KeyDeleteAsync(sessionCacheKeys);
            }

            // 🗑️ 删除会话集合本身（无论是否有成员，安全删除）
            await _redisDb.KeyDeleteAsync(sessionsSetKey);
        }

        /// <summary>
        /// 🆕 创建或更新用户的访问令牌（AccessToken）和刷新令牌（RefreshToken）。
        /// 同时持久化 SessionInfoVO 和 RefreshTokenMetaVO 到 Redis。
        /// 
        /// 🔄 存储结构：
        /// - AccessToken 会话 → SessionInfoVO → Key: {InstanceName}:{UserId}:sessions:{SessionId}
        /// - RefreshToken 元数据 → RefreshTokenMetaVO → Key: {InstanceName}:refreshTokens:{RefreshTokenId}
        /// - 用户会话集合 → Set Add SessionId
        /// - 用户 refreshToken 集合 → Set Add RefreshTokenId
        /// 
        /// ⏳ 自动过期：根据 expiresAt / refreshTokenExpiresAt 设置 Redis Key TTL。
        /// 📌 扩展建议：在 Controller 或 Middleware 中补充 Device / IpAddress / UserAgent。
        /// </summary>
        /// <param name="userId">用户唯一标识。</param>
        /// <param name="newAccessToken">新签发的访问令牌。</param>
        /// <param name="newRefreshTokenId">新签发的刷新令牌的唯一标识。</param>
        /// <param name="loginTime">登录/令牌创建时间（UTC）。</param>
        /// <param name="expiresAt">访问令牌过期时间（UTC）。</param>
        /// <param name="refreshTokenExpiresAt">刷新令牌过期时间（UTC）。</param>
        /// <param name="clientInfoVO">客户端信息（Device, IpAddress, UserAgent）。</param>
        public async Task UpdateTokenAsync(
            long userId,
            string newAccessToken,
            string newRefreshTokenId,
            DateTime loginTime,
            DateTime expiresAt,
            DateTime refreshTokenExpiresAt,
            ClientInfoVO clientInfoVO)
        {
            if (string.IsNullOrWhiteSpace(newAccessToken) || string.IsNullOrWhiteSpace(newRefreshTokenId))
                throw new ArgumentException("AccessToken 和 RefreshTokenID 不能为空");

            // ✅ 创建并序列化 SessionInfoVO（访问令牌会话信息）
            var sessionId = Guid.NewGuid().ToString(); // 生成唯一会话 ID
            var sessionInfo = new SessionInfoVO
            {
                UserId = userId,
                AccessToken = newAccessToken,
                LoginTime = loginTime,
                ExpiresAt = expiresAt,
                SessionId = sessionId,
                // 📌 扩展点：可在 Controller 中补充 Device, IpAddress, UserAgent
                Device = clientInfoVO.Device,
                IpAddress = clientInfoVO.IpAddress,
                UserAgent = clientInfoVO.UserAgent,
                RefreshTokenId = newRefreshTokenId
            };
            var serializedSessionInfo = JsonSerializer.Serialize(sessionInfo);
            var sessionKey = $"{_instanceName}:{userId}:sessions:{sessionId}";
            // ➕ 添加到用户会话
            await _redisDb.StringSetAsync(sessionKey, serializedSessionInfo, expiresAt - DateTime.UtcNow);

            // ➕ 添加到用户会话集合（存储的是 sessionId，便于遍历删除）
            var sessionsSetKey = $"{_instanceName}:{userId}:userSessions";
            await _redisDb.SetAddAsync(sessionsSetKey, sessionId);

            // ✅ 创建并序列化 RefreshTokenMetaVO（刷新令牌元数据）
            var refreshTokenMetadata = new RefreshTokenMetaVO
            {
                UserId = userId,
                CreatedAt = loginTime,
                ExpiresAt = refreshTokenExpiresAt,
                // 📌 扩展点：可在 Controller 中补充 Device, IpAddress, UserAgent, LastUsedAt
                LastUsedAt = loginTime,
                Device = clientInfoVO.Device,
                IpAddress = clientInfoVO.IpAddress,
                UserAgent = clientInfoVO.UserAgent,
                SessionId = sessionId
            };
            var serializedRefreshTokenMetadata = JsonSerializer.Serialize(refreshTokenMetadata);
            var refreshTokenKey = $"{_instanceName}:{userId}:refreshTokens:{newRefreshTokenId}"; // Key 使用 refreshToken 字符串
            // ➕ 添加到用户 refreshToken
            await _redisDb.StringSetAsync(refreshTokenKey, serializedRefreshTokenMetadata, refreshTokenExpiresAt - DateTime.UtcNow);

            // ➕ 添加到用户 refreshToken 集合（存储的是 refreshTokenId，便于遍历删除）
            var refreshTokenSetKey = $"{_instanceName}:{userId}:userRefreshTokens";
            await _redisDb.SetAddAsync(refreshTokenSetKey, newRefreshTokenId);


        }
    }
}