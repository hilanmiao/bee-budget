using Bedrock.Application.Interfaces;
using Bedrock.Application.ValueObjects;
using Bedrock.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StackExchange.Redis;
using System.Text.Json;

namespace Bedrock.Application.Services
{
    /// <summary>
    /// 后台令牌清理服务（定时任务）
    /// 定期扫描 Redis 中所有用户，清理过期的 AccessToken 会话和 RefreshToken 元数据。
    /// </summary>
    /// <remarks>
    /// <para><b>核心职责</b>：作为“惰性清理”的兜底机制，确保即使用户长期不活跃，过期令牌也能被回收，防止 Redis 内存泄漏。</para>
    /// <para><b>扫描策略</b>：通过 <c>SCAN</c> 命令遍历模式 <c>{InstanceName}:*:userSessions</c> 获取所有活跃用户。</para>
    /// <para><b>清理粒度</b>：对每个用户，分别清理其会话（Session）和刷新令牌（RefreshToken）中已过期或损坏的条目。</para>
    /// <para><b>调度频率</b>：默认每 30 分钟执行一次，可通过 <see cref="TokenCleanupConfig.CleanupInterval"/> 调整。</para>
    /// <para><b>关键限制</b>：仅适用于单机或主从 Redis 架构；<b>不支持 Redis Cluster</b>，因 <c>KeysAsync</c> 无法跨分片扫描。</para>
    /// <para><b>设计原则</b>：异步非阻塞、批量操作、异常隔离（单用户失败不影响整体）、结构化日志便于监控告警。</para>
    /// <para><b>使用方式</b>：由后台服务（如 <see cref="TokenCleanupBackgroundService"/>）或手动触发调用。</para>
    /// </remarks>
    public class TokenCleanupService: ITokenCleanupService
    {
        // ========== 🧱 字段定义 ==========

        private readonly IConnectionMultiplexer _redis;
        private readonly ILogger<TokenCleanupService> _logger;
        private readonly string _instanceName;

        // ========== 🚪 构造函数 ==========

        /// <summary>
        /// 初始化令牌清理服务。
        /// </summary>
        /// <param name="redis">Redis 连接复用器，用于访问数据库和服务器信息。</param>
        /// <param name="redisConfig">Redis 配置，用于获取实例名称（作为 Key 前缀）。</param>
        /// <param name="logger">结构化日志记录器。</param>
        /// <exception cref="ArgumentNullException">当必要依赖为 null 时抛出。</exception>
        public TokenCleanupService(
            IConnectionMultiplexer redis,
            IOptions<RedisConfig> redisConfig,
            ILogger<TokenCleanupService> logger)
        {
            _redis = redis ?? throw new ArgumentNullException(nameof(redis));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _instanceName = redisConfig?.Value?.InstanceName ?? throw new ArgumentNullException(nameof(redisConfig));
        }

        // ========== 🧹 主清理方法 ==========

        /// <summary>
        /// 执行一次完整的过期令牌清理流程。
        /// </summary>
        /// <remarks>
        /// <para>1. 使用 <see cref="IServer.KeysAsync"/> 安全扫描匹配 <c>{InstanceName}:*:userSessions</c> 的键（内部使用 <c>SCAN</c>，非阻塞）。</para>
        /// <para>2. 对每个用户 ID，分别清理其会话和刷新令牌。</para>
        /// <para>3. 每处理一个用户后插入 1ms 延迟，防止高频操作压垮 Redis（适用于用户量大的场景）。</para>
        /// <para><b>⚠️ 注意</b>：此方法在 Redis Cluster 下无效，因 <c>SCAN</c> 仅作用于单节点。集群环境需改用事件驱动或维护全局用户列表。</para>
        /// </remarks>
        /// <exception cref="InvalidOperationException">当无可用 Redis 服务器时抛出。</exception>
        public async Task CleanupExpiredTokensAsync()
        {
            var startTime = DateTime.UtcNow;
            _logger.LogInformation("开始执行后台令牌清理...");

            var redisDb = _redis.GetDatabase();
            var dbIndex = redisDb.Database;

            var userCount = 0;
            var cleanedSessions = 0;
            //var cleanedRefreshTokens = 0;

            // 构建 SCAN 模式：{InstanceName}:*:userSessions
            var pattern = $"{_instanceName}:*:userSessions";

            // 获取第一个可用的 Redis 服务器（用于执行 KeysAsync）
            var server = GetConnectedServer();

            // 使用 KeysAsync（StackExchange.Redis ≥2.0 内部自动使用 SCAN）
            // pageSize=100：平衡内存占用与网络往返次数
            IAsyncEnumerable<RedisKey> keysAsync;
            try
            {
                keysAsync = server.KeysAsync(
                    database: dbIndex,
                    pattern: pattern,
                    pageSize: 100,
                    flags: CommandFlags.None);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取 KeysAsync 枚举器失败");
                return;
            }

            // 异步遍历所有匹配的用户会话键
            await foreach (var key in keysAsync)
            {
                var keyStr = key.ToString();
                var parts = keyStr.Split(':');

                // 提取 UserId（格式：{InstanceName}:{UserId}:userSessions）
                if (parts.Length < 3 || !long.TryParse(parts[1], out var userId))
                {
                    _logger.LogWarning("无法解析用户ID，跳过清理：{Key}", keyStr);
                    continue;
                }

                userCount++;

                // 清理会话与刷新令牌（独立方法，便于测试和复用）
                cleanedSessions += await CleanupExpiredSessionsForUserAsync(redisDb, userId);
                //cleanedRefreshTokens += await CleanupExpiredRefreshTokensForUserAsync(redisDb, userId);

                // 防御性延迟：降低 Redis 压力（尤其在用户量大时）
                await Task.Delay(1);
            }

            var duration = DateTime.UtcNow - startTime;
            //_logger.LogInformation(
            //    "后台令牌清理完成，共扫描 {UserCount} 个用户，清理 {SessionCount} 个会话 + {RefreshTokenCount} 个刷新令牌，耗时 {DurationMs}ms",
            //    userCount, cleanedSessions, cleanedRefreshTokens, duration.TotalMilliseconds);
            _logger.LogInformation(
               "后台令牌清理完成，共扫描 {UserCount} 个用户会话，清理 {SessionCount} 个会话，耗时 {DurationMs}ms",
               userCount, cleanedSessions, duration.TotalMilliseconds);
        }

        public async Task CleanupExpiredRefreshTokensAsync()
        {
            var startTime = DateTime.UtcNow;
            _logger.LogInformation("开始执行后台刷新令牌清理...");

            var redisDb = _redis.GetDatabase();
            var dbIndex = redisDb.Database;

            var userCount = 0;
            //var cleanedSessions = 0;
            var cleanedRefreshTokens = 0;

            // 构建 SCAN 模式：{InstanceName}:*:userSessions
            var pattern = $"{_instanceName}:*:userRefreshTokens";

            // 获取第一个可用的 Redis 服务器（用于执行 KeysAsync）
            var server = GetConnectedServer();

            // 使用 KeysAsync（StackExchange.Redis ≥2.0 内部自动使用 SCAN）
            // pageSize=100：平衡内存占用与网络往返次数
            IAsyncEnumerable<RedisKey> keysAsync;
            try
            {
                keysAsync = server.KeysAsync(
                    database: dbIndex,
                    pattern: pattern,
                    pageSize: 100,
                    flags: CommandFlags.None);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取 KeysAsync 枚举器失败");
                return;
            }

            // 异步遍历所有匹配的用户会话键
            await foreach (var key in keysAsync)
            {
                var keyStr = key.ToString();
                var parts = keyStr.Split(':');

                // 提取 UserId（格式：{InstanceName}:{UserId}:userSessions）
                if (parts.Length < 3 || !long.TryParse(parts[1], out var userId))
                {
                    _logger.LogWarning("无法解析用户ID，跳过清理：{Key}", keyStr);
                    continue;
                }

                userCount++;

                // 清理会话与刷新令牌（独立方法，便于测试和复用）
                //cleanedSessions += await CleanupExpiredSessionsForUserAsync(redisDb, userId);
                cleanedRefreshTokens += await CleanupExpiredRefreshTokensForUserAsync(redisDb, userId);

                // 防御性延迟：降低 Redis 压力（尤其在用户量大时）
                await Task.Delay(1);
            }

            var duration = DateTime.UtcNow - startTime;
            //_logger.LogInformation(
            //    "后台令牌清理完成，共扫描 {UserCount} 个用户，清理 {SessionCount} 个会话 + {RefreshTokenCount} 个刷新令牌，耗时 {DurationMs}ms",
            //    userCount, cleanedSessions, cleanedRefreshTokens, duration.TotalMilliseconds);
            _logger.LogInformation(
               "后台令牌清理完成，共扫描 {UserCount} 个用户刷新令牌，清理 {cleanedRefreshTokens} 个刷新令牌，耗时 {DurationMs}ms",
               userCount, cleanedRefreshTokens, duration.TotalMilliseconds);
        }

        // ========== 🔌 辅助方法：获取可用 Redis 服务器 ==========

        /// <summary>
        /// 获取第一个处于连接状态的 Redis 服务器实例。
        /// </summary>
        /// <returns>已连接的 <see cref="IServer"/> 实例。</returns>
        /// <exception cref="InvalidOperationException">当所有端点均未连接时抛出。</exception>
        /// <remarks>
        /// 此方法用于支持 <c>KeysAsync</c>，该操作必须在具体服务器上执行。
        /// 在主从架构中，通常选择主节点；但 KeysAsync 为只读操作，从节点亦可。
        /// </remarks>
        private IServer GetConnectedServer()
        {
            foreach (var endpoint in _redis.GetEndPoints())
            {
                var server = _redis.GetServer(endpoint);
                if (server.IsConnected)
                    return server;
            }
            throw new InvalidOperationException("未找到可用的 Redis 服务器");
        }

        // ========== 🧹 清理会话方法 ==========

        /// <summary>
        /// 清理指定用户的过期会话（AccessToken）及其在 Set 中的引用。
        /// </summary>
        /// <param name="redisDb">Redis 数据库操作接口。</param>
        /// <param name="userId">目标用户唯一标识。</param>
        /// <returns>成功清理的会话数量。</returns>
        /// <remarks>
        /// <para>1. 从 Set <c>{InstanceName}:{userId}:userSessions</c> 获取所有会话 ID。</para>
        /// <para>2. 批量读取会话数据（格式为 <see cref="SessionInfoVO"/>）。</para>
        /// <para>3. 若数据损坏（JSON 反序列化失败）或已过期（<c>ExpiresAt ≤ UtcNow</c>），则删除会话键并从 Set 中移除 ID。</para>
        /// <para>4. 若 Set 中存在无对应键的“残留 ID”，也一并清理（防御性编程）。</para>
        /// <para>5. 操作以批次（最大 50）提交，减少 Redis 命令往返次数。</para>
        /// </remarks>
        private async Task<int> CleanupExpiredSessionsForUserAsync(IDatabase redisDb, long userId)
        {
            var sessionsSetKey = $"{_instanceName}:{userId}:userSessions";
            var sessionIds = await redisDb.SetMembersAsync(sessionsSetKey, flags: CommandFlags.None);

            if (!sessionIds.Any()) return 0;

            // 构建所有会话键（用于批量读取）
            var sessionKeys = sessionIds.Select(id => (RedisKey)$"{_instanceName}:{userId}:sessions:{id}").ToArray();
            var sessionValues = await redisDb.StringGetAsync(sessionKeys, flags: CommandFlags.None);

            var cleanupTasks = new List<Task>();
            var cleanedCount = 0;
            const int MaxBatchSize = 50;

            for (int i = 0; i < sessionValues.Length; i++)
            {
                var redisValue = sessionValues[i];
                var sessionId = sessionIds[i].ToString();

                if (!redisValue.IsNullOrEmpty)
                {
                    SessionInfoVO? sessionInfo = null;
                    try
                    {
                        sessionInfo = JsonSerializer.Deserialize<SessionInfoVO>(redisValue.ToString());
                    }
                    catch (JsonException ex)
                    {
                        _logger.LogWarning(ex, "会话数据反序列化失败，用户ID：{UserId}，会话ID：{SessionId}", userId, sessionId);
                    }

                    // 过期或无效数据：清理键 + 从 Set 移除
                    if (sessionInfo == null || DateTime.UtcNow >= sessionInfo.ExpiresAt)
                    {
                        var sessionKey = $"{_instanceName}:{userId}:sessions:{sessionId}";
                        // 创建清理任务（但不立即执行！）
                        cleanupTasks.Add(Task.WhenAll(
                            redisDb.SetRemoveAsync(sessionsSetKey, sessionId, flags: CommandFlags.None),
                            redisDb.KeyDeleteAsync(sessionKey, flags: CommandFlags.None)
                        ));
                        cleanedCount++;
                    }
                }
                else
                {
                    // 残留 ID：仅从 Set 中移除（无对应键）
                    cleanupTasks.Add(redisDb.SetRemoveAsync(sessionsSetKey, sessionId, flags: CommandFlags.None));
                    cleanedCount++;
                }

                // 批量执行，避免任务列表过大
                if (cleanupTasks.Count >= MaxBatchSize)
                {
                    await ExecuteCleanupBatchAsync(cleanupTasks, userId, "会话");
                    cleanupTasks.Clear();
                }
            }

            if (cleanupTasks.Any())
            {
                await ExecuteCleanupBatchAsync(cleanupTasks, userId, "会话");
            }

            return cleanedCount;
        }

        // ========== 🧹 清理刷新令牌方法 ==========

        /// <summary>
        /// 清理指定用户的过期刷新令牌（RefreshToken）及其在 Set 中的引用。
        /// </summary>
        /// <param name="redisDb">Redis 数据库操作接口。</param>
        /// <param name="userId">目标用户唯一标识。</param>
        /// <returns>成功清理的刷新令牌数量。</returns>
        /// <remarks>
        /// 逻辑与会话清理高度对称，数据结构为 <see cref="RefreshTokenMetaVO"/>。
        /// 同样处理反序列化失败、过期、残留 ID 三种情况。
        /// </remarks>
        private async Task<int> CleanupExpiredRefreshTokensForUserAsync(IDatabase redisDb, long userId)
        {
            var refreshTokenSetKey = $"{_instanceName}:{userId}:userRefreshTokens";
            var refreshTokenIds = await redisDb.SetMembersAsync(refreshTokenSetKey, flags: CommandFlags.None);

            if (!refreshTokenIds.Any()) return 0;

            // 构建所有刷新令牌键（用于批量读取）
            var refreshTokenKeys = refreshTokenIds.Select(id => (RedisKey)$"{_instanceName}:{userId}:refreshTokens:{id}").ToArray();
            var refreshTokenValues = await redisDb.StringGetAsync(refreshTokenKeys, flags: CommandFlags.None);

            var cleanupTasks = new List<Task>();
            var cleanedCount = 0;
            const int MaxBatchSize = 50;

            for (int i = 0; i < refreshTokenValues.Length; i++)
            {
                var redisValue = refreshTokenValues[i];
                var refreshTokenId = refreshTokenIds[i].ToString();

                if (!redisValue.IsNullOrEmpty)
                {
                    RefreshTokenMetaVO? refreshTokenMeta = null;
                    try
                    {
                        refreshTokenMeta = JsonSerializer.Deserialize<RefreshTokenMetaVO>(redisValue.ToString());
                    }
                    catch (JsonException ex)
                    {
                        _logger.LogWarning(ex, "刷新令牌数据反序列化失败，用户ID：{UserId}，令牌ID：{RefreshTokenId}", userId, refreshTokenId);
                    }

                    // 过期或无效：清理键 + 从 Set 移除
                    if (refreshTokenMeta == null || DateTime.UtcNow >= refreshTokenMeta.ExpiresAt)
                    {
                        var refreshTokenKey = $"{_instanceName}:{userId}:refreshTokens:{refreshTokenId}";
                        // 创建清理任务（但不立即执行！）
                        cleanupTasks.Add(Task.WhenAll(
                            redisDb.SetRemoveAsync(refreshTokenSetKey, refreshTokenId, flags: CommandFlags.None),
                            redisDb.KeyDeleteAsync(refreshTokenKey, flags: CommandFlags.None)
                        ));
                        cleanedCount++;
                    }
                }
                else
                {
                    // 残留 ID：仅从 Set 中移除
                    cleanupTasks.Add(redisDb.SetRemoveAsync(refreshTokenSetKey, refreshTokenId, flags: CommandFlags.None));
                    cleanedCount++;
                }

                // 批量执行
                if (cleanupTasks.Count >= MaxBatchSize)
                {
                    await ExecuteCleanupBatchAsync(cleanupTasks, userId, "刷新令牌");
                    cleanupTasks.Clear();
                }
            }

            if (cleanupTasks.Any())
            {
                await ExecuteCleanupBatchAsync(cleanupTasks, userId, "刷新令牌");
            }

            return cleanedCount;
        }

        // ========== 🧩 批量执行辅助方法 ==========

        /// <summary>
        /// 批量执行清理任务，并捕获异常以避免中断整体流程。
        /// </summary>
        /// <param name="tasks">待执行的 Redis 操作任务列表。</param>
        /// <param name="userId">关联的用户 ID（用于日志追踪）。</param>
        /// <param name="entityType">实体类型（如“会话”或“刷新令牌”），用于日志描述。</param>
        /// <remarks>
        /// 即使部分操作失败，其余任务仍会继续执行。错误仅记录，不抛出。
        /// 这是“尽力而为”清理策略的体现，符合后台任务的容错设计。
        /// </remarks>
        private async Task ExecuteCleanupBatchAsync(List<Task> tasks, long userId, string entityType)
        {
            try
            {
                await Task.WhenAll(tasks);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "批量清理用户 {UserId} 的 {EntityType} 时发生错误", userId, entityType);
            }
        }
    }
}