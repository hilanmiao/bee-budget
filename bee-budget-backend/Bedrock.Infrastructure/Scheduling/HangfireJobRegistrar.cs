using Bedrock.Configuration;
using Hangfire;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StackExchange.Redis;
using System.Linq.Expressions;

namespace Bedrock.Infrastructure.Scheduling
{
    /// <summary>
    /// Hangfire 定时任务注册后台服务。
    /// 在应用程序完全启动后，自动向 Hangfire 注册所有预定义的周期性任务。
    /// 同时初始化任务在 Redis 中的运行状态（"run" 或 "pause"）。
    /// </summary>
    public class HangfireJobRegistrationBackgroundService : BackgroundService
    {
        private readonly ILogger<HangfireJobRegistrationBackgroundService> _logger;
        private readonly IConfiguration _configuration;
        private readonly IConnectionMultiplexer _redis;
        private readonly string _instanceName;
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly IHostApplicationLifetime _applicationLifetime;

        /// <summary>
        /// 初始化 <see cref="HangfireJobRegistrationBackgroundService"/> 实例。
        /// </summary>
        /// <param name="redis">Redis 连接复用器，用于状态存储。</param>
        /// <param name="redisConfig">Redis 配置选项，用于获取实例名称。</param>
        /// <param name="logger">日志记录器。</param>
        /// <param name="serviceScopeFactory">用于创建依赖注入作用域的服务工厂。</param>
        /// <param name="applicationLifetime">应用程序生命周期管理器，用于确保在启动完成后执行任务注册。</param>
        /// <exception cref="ArgumentNullException">当任一必要参数为 null 时抛出。</exception>
        public HangfireJobRegistrationBackgroundService(
            ILogger<HangfireJobRegistrationBackgroundService> logger,
            IConfiguration configuration,
            IOptions<RedisConfig> redisConfig,
            IConnectionMultiplexer redis,
            IServiceScopeFactory serviceScopeFactory,
            IHostApplicationLifetime applicationLifetime)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _configuration = configuration;
            _redis = redis ?? throw new ArgumentNullException(nameof(redis));
            _instanceName = redisConfig?.Value?.InstanceName
                ?? throw new ArgumentNullException(nameof(redisConfig), "Redis 配置中的 InstanceName 不能为空。");
            _serviceScopeFactory = serviceScopeFactory ?? throw new ArgumentNullException(nameof(serviceScopeFactory));
            _applicationLifetime = applicationLifetime ?? throw new ArgumentNullException(nameof(applicationLifetime));
        }

        /// <summary>
        /// 后台服务执行入口。等待应用程序启动完成后，注册 Hangfire 周期性任务。
        /// </summary>
        /// <param name="stoppingToken">用于通知服务应停止的取消令牌。</param>
        /// <returns>表示异步操作的任务。</returns>
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // 等待应用程序完全启动后再执行任务注册
            _applicationLifetime.ApplicationStarted.Register(async () =>
            {
                try
                {
                    _logger.LogInformation("开始注册 Hangfire 周期性任务...");

                    // 创建作用域以解析 Scoped 服务（如 IRecurringJobManager）
                    using var scope = _serviceScopeFactory.CreateScope();
                    var recurringJobManager = scope.ServiceProvider.GetRequiredService<IRecurringJobManager>();

                    await RegisterRecurringJobsAsync(recurringJobManager, stoppingToken);

                    _logger.LogInformation("Hangfire 周期性任务注册完成。");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "注册 Hangfire 任务时发生异常。");
                    throw; // 可根据需要决定是否让宿主崩溃
                }
            });

            // 保持后台服务运行（无实际工作循环，仅用于生命周期管理）
            await Task.Delay(Timeout.Infinite, stoppingToken);
        }

        /// <summary>
        /// 实际执行 Hangfire 周期性任务注册的逻辑。
        /// </summary>
        /// <param name="recurringJobManager">Hangfire 周期性任务管理器。</param>
        /// <param name="cancellationToken">取消令牌。</param>
        /// <returns>表示异步操作的任务。</returns>
        private async Task RegisterRecurringJobsAsync(IRecurringJobManager recurringJobManager, CancellationToken cancellationToken)
        {
            // 测试任务，每天 01:00 执行一次
            await RegisterRecurringJobAsync<TestScheduler>(recurringJobManager, jobId: "test", methodCall: x => x.RunTask("Hangfire"), cronExpression: "0 1 * * *");
            // TokenCleanup 任务，每天 01:10 执行一次
            await RegisterRecurringJobAsync<TokenCleanupScheduler>(recurringJobManager, jobId: "tokenCleanup", methodCall: x => x.RunTask(), cronExpression: "10 1 * * *");
        }

        /// <summary>
        /// 注册一个 Hangfire 周期性任务，并初始化其 Redis 状态。
        /// </summary>
        /// <typeparam name="T">执行任务的服务类型（必须是具体类，不能是接口）。</typeparam>
        /// <param name="recurringJobManager">Hangfire 周期性任务管理器。</param>
        /// <param name="jobId">任务唯一标识符。</param>
        /// <param name="methodCall">要执行的方法调用表达式（如 x => x.RunTask("arg")）。</param>
        /// <param name="cronExpression">Cron 表达式。</param>
        /// <param name="timeZone">时区，默认为中国标准时间。</param>
        /// <param name="queueName">队列名称，默认为 "default"。</param>
        /// <param name="cancellationToken">取消令牌。</param>
        /// <returns>表示异步操作的任务。</returns>
        private async Task RegisterRecurringJobAsync<T>(
            IRecurringJobManager recurringJobManager,
            string jobId,
            Expression<Action<T>> methodCall,
            string cronExpression,
            TimeZoneInfo? timeZone = null,
            string queueName = "default",
            CancellationToken cancellationToken = default)
            where T : class
        {
            if (string.IsNullOrWhiteSpace(jobId))
                throw new ArgumentException("任务 ID 不能为空。", nameof(jobId));
            if (string.IsNullOrWhiteSpace(cronExpression))
                throw new ArgumentException("Cron 表达式不能为空。", nameof(cronExpression));

            // 优先使用传入的时区，否则从配置读取，最后兜底 IANA 默认值
            if (timeZone == null)
            {
                var tzId = _configuration["AppSettings:TimeZoneId"] ?? "Asia/Shanghai";
                try
                {
                    timeZone = TimeZoneInfo.FindSystemTimeZoneById(tzId);
                }
                catch (TimeZoneNotFoundException ex)
                {
                    throw new InvalidOperationException(
                        $"配置 'AppSettings:TimeZoneId' 的值 '{tzId}' 不是有效的 IANA 时区ID。" +
                        $"请使用标准 IANA 格式，如 'Asia/Shanghai'、'UTC'。", ex);
                }
            }

            var redisDb = _redis.GetDatabase();
            var redisKey = $"{_instanceName}:hangfire:job:{jobId}";

            // 初始化 Redis 状态（仅当不存在时）
            var exists = await redisDb.KeyExistsAsync(redisKey, flags: CommandFlags.PreferReplica);
            if (!exists)
            {
                await redisDb.StringSetAsync(redisKey, "run", flags: CommandFlags.FireAndForget);
                _logger.LogInformation("已初始化任务 '{JobId}' 的 Redis 状态为 'run'。", jobId);
            }

            // 注册 Hangfire 任务
            var options = new RecurringJobOptions
            {
                TimeZone = timeZone,
                QueueName = queueName
            };

            recurringJobManager.AddOrUpdate(jobId, methodCall, cronExpression, options);

            _logger.LogInformation(
                "已注册周期性任务：{JobId}，Cron：{Cron}，时区：{TimeZone}，队列：{Queue}",
                jobId, cronExpression, timeZone.Id, queueName);
        }
    }
}