using Bedrock.Application.Interfaces;
using Bedrock.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Bedrock.Application.Services
{
    /// <summary>
    /// 后台定时任务服务，用于周期性清理过期的访问令牌（Access Token）和刷新令牌（Refresh Token）。
    /// 该服务以 <see cref="BackgroundService"/> 形式运行，通过创建独立的 DI 作用域来安全使用 Scoped 服务。
    /// </summary>
    public class TokenCleanupBackgroundService : BackgroundService
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly ILogger<TokenCleanupBackgroundService> _logger;
        private readonly TimeSpan _interval;

        /// <summary>
        /// 初始化令牌清理后台服务。
        /// </summary>
        /// <param name="serviceScopeFactory">
        /// 用于在后台任务中创建新的依赖注入作用域，从而安全解析 Scoped 服务（如 <see cref="ITokenCleanupService"/>）。
        /// </param>
        /// <param name="logger">结构化日志记录器，用于记录任务执行状态与异常。</param>
        /// <param name="cleanupOptions">
        /// 可选的清理任务配置，用于自定义执行间隔；若未提供，则默认每 30 分钟执行一次。
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// 当 <paramref name="serviceScopeFactory"/> 或 <paramref name="logger"/> 为 <see langword="null"/> 时抛出。
        /// </exception>
        public TokenCleanupBackgroundService(
            IServiceScopeFactory serviceScopeFactory,
            ILogger<TokenCleanupBackgroundService> logger,
            IOptions<TokenCleanupConfig>? cleanupOptions = null)
        {
            _serviceScopeFactory = serviceScopeFactory ?? throw new ArgumentNullException(nameof(serviceScopeFactory));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _interval = cleanupOptions?.Value?.CleanupInterval ?? TimeSpan.FromMinutes(30);
        }

        /// <inheritdoc/>
        /// <remarks>
        /// 该方法在后台服务启动后周期性执行清理任务。
        /// 每次执行都会创建新的 DI 作用域，确保 Scoped 服务（如数据库上下文）的生命周期正确管理。
        /// </remarks>
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation(
                "Token cleanup background service is starting. Cleanup interval: {Interval} minutes.",
                _interval.TotalMinutes);

            // 立即执行一次清理（可选，可根据需求移除）
            await PerformCleanupAsync(stoppingToken);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    // 等待指定间隔（支持取消）
                    await Task.Delay(_interval, stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    // 当服务停止时，Task.Delay 会抛出此异常，属于正常行为
                    _logger.LogInformation("Token cleanup background service delay was canceled.");
                    break;
                }

                if (!stoppingToken.IsCancellationRequested)
                {
                    await PerformCleanupAsync(stoppingToken);
                }
            }

            _logger.LogInformation("Token cleanup background service is stopping.");
        }

        /// <summary>
        /// 在独立的 DI 作用域中执行令牌清理逻辑。
        /// </summary>
        /// <param name="cancellationToken">用于响应服务停止请求的取消令牌。</param>
        private async Task PerformCleanupAsync(CancellationToken cancellationToken)
        {
            try
            {
                // 创建新的作用域以解析 Scoped 服务
                using var scope = _serviceScopeFactory.CreateScope();
                var cleanupService = scope.ServiceProvider.GetRequiredService<ITokenCleanupService>();

                _logger.LogDebug("Starting expired token cleanup.");

                await cleanupService.CleanupExpiredTokensAsync();
                await cleanupService.CleanupExpiredRefreshTokensAsync();

                _logger.LogInformation("Expired token cleanup completed successfully.");
            }
            catch (Exception ex) when (!(ex is OperationCanceledException))
            {
                // 记录异常但不中断后台服务（避免整个宿主崩溃）
                _logger.LogError(ex, "An error occurred during token cleanup.");
            }
        }
    }
}