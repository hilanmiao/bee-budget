using Bedrock.Application.Interfaces;
using Bedrock.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StackExchange.Redis;

namespace Bedrock.Infrastructure.Scheduling
{
    public class TokenCleanupScheduler
    {
        private readonly ILogger<TokenCleanupScheduler> _logger;
        private readonly IConnectionMultiplexer _redis;
        private readonly string _instanceName;
        private string _jobId = "tokenCleanup";
        private readonly ITokenCleanupService _tokenCleanupService;


        public TokenCleanupScheduler(IConnectionMultiplexer redis, IOptions<RedisConfig> redisConfig, ILogger<TokenCleanupScheduler> logger, ITokenCleanupService tokenCleanupService)
        {
            _redis = redis ?? throw new ArgumentNullException(nameof(redis));
            _instanceName = redisConfig?.Value?.InstanceName ?? throw new ArgumentNullException(nameof(redisConfig));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _tokenCleanupService = tokenCleanupService ?? throw new ArgumentNullException(nameof(tokenCleanupService));
        }

        public void RunTask()
        {
            var redisDb = _redis.GetDatabase();

            // 检查任务状态 run\pause
            var cacheKey = $"{_instanceName}:hangfire:job:{_jobId}";
            var redisValue = redisDb.StringGetAsync(cacheKey).GetAwaiter().GetResult();
            if (redisValue != "run")
            {
                _logger.LogInformation("任务 [{JobId}] 已暂停，跳过执行", _jobId);
                return;
            }

            _logger.LogInformation("开始执行任务 [{JobId}]", _jobId);

            // 执行任务逻辑
            _tokenCleanupService.CleanupExpiredTokensAsync().GetAwaiter().GetResult();
            _tokenCleanupService.CleanupExpiredRefreshTokensAsync().GetAwaiter().GetResult();
        }
    }
}
