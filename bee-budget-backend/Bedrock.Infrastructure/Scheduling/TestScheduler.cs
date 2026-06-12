using Bedrock.Application.Interfaces;
using Bedrock.Configuration;
using Bedrock.Core.Entities;
using Bedrock.Application.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bedrock.Infrastructure.Scheduling
{
    public class TestScheduler
    {
        private readonly ILogger<TestScheduler> _logger;
        private readonly IConnectionMultiplexer _redis;
        private readonly string _instanceName;
        private string _jobId = "test";
        private readonly ITestTaskService _testTaskService;


        public TestScheduler(IConnectionMultiplexer redis, IOptions<RedisConfig> redisConfig, ILogger<TestScheduler> logger, ITestTaskService testTaskService)
        {
            _redis = redis ?? throw new ArgumentNullException(nameof(redis));
            _instanceName = redisConfig?.Value?.InstanceName ?? throw new ArgumentNullException(nameof(redisConfig));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _testTaskService = testTaskService ?? throw new ArgumentNullException(nameof(testTaskService));
        }

        public void RunTask(string name)
        {
            var redisDb = _redis.GetDatabase();

            // 检查任务状态 run\pause
            var cacheKey = $"{_instanceName}:hangfire:job:{_jobId}";
            //var redisValue = await redisDb.StringGetAsync(cacheKey);
            var redisValue = redisDb.StringGetAsync(cacheKey).GetAwaiter().GetResult(); // 同步等待
            if (redisValue != "run")
            {
                _logger.LogInformation("任务 [{JobId}] 已暂停，跳过执行", _jobId);
                return;
            }

            _logger.LogInformation("开始执行任务 [{JobId}]", _jobId);

            // 执行任务逻辑
            //await _testTaskService.SayHello(name); // 官方明确建议：周期性任务的方法应为 void 或同步方法
            // 同步调用异步服务（在 Hangfire 任务中是安全的）
            _testTaskService.SayHello(name).GetAwaiter().GetResult();
        }
    }
}
