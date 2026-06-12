using Bedrock.WebApi.Responses;
using Bedrock.Infrastructure.Helpers;
using Microsoft.IdentityModel.Tokens;

namespace Bedrock.WebApi.Middleware
{
    /// <summary>
    /// 全局异常处理中间件，用于捕获并处理整个应用程序中的未处理异常。
    /// 通过统一处理所有未捕获的异常，确保API返回标准化的错误响应，提升用户体验和系统健壮性。
    /// </summary>
    public class GlobalExceptionHandlingMiddleware
    {
        private readonly ILogger<GlobalExceptionHandlingMiddleware> _logger;
        private readonly RequestDelegate _next;

        /// <summary>
        /// 构造函数注入下一个中间件委托。
        /// </summary>
        /// <param name="logger">日志记录器。</param>
        /// <param name="next">下一个中间件在管道中的委托。</param>
        public GlobalExceptionHandlingMiddleware(ILogger<GlobalExceptionHandlingMiddleware> logger, RequestDelegate next)
        {
            _logger = logger;
            _next = next;
        }

        /// <summary>
        /// 异常处理中间件的核心逻辑，负责处理每个HTTP请求。
        /// 在调用下一个中间件时捕获未处理的异常，并记录日志后返回适当的错误响应。
        /// </summary>
        /// <param name="context">当前HTTP上下文。</param>
        /// <returns>异步任务。</returns>
        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                // 调用下一个中间件
                await _next(context);
            }
            catch (Exception ex)
            {
                ConsoleHelper.Error($"一个未处理的全局异常发生：{ex.Message}");

                // 记录异常信息
                _logger.LogError(ex, "一个未处理的全局异常发生");

                // 调用处理异常的方法
                await HandleExceptionAsync(context, ex);
            }
        }

        /// <summary>
        /// 处理异常并返回适当的HTTP响应。
        /// 根据异常类型设置状态码和错误响应内容。
        /// </summary>
        /// <param name="context">当前HTTP上下文。</param>
        /// <param name="exception">发生的异常。</param>
        /// <returns>异步任务。</returns>
        private static Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            // 初始化状态码为内部服务器错误
            var statusCode = StatusCodes.Status500InternalServerError;
            var apiResponse = ApiResponse<object>.Error(statusCode, "一个未处理的全局异常发生。");

            // 使用 switch 语句匹配不同类型的异常
            switch (exception)
            {
                case ArgumentException argumentException:
                    statusCode = StatusCodes.Status400BadRequest;
                    apiResponse = ApiResponse<object>.BadRequest(argumentException.Message);
                    break;
                case SecurityTokenException securityTokenException:
                    statusCode = StatusCodes.Status401Unauthorized;
                    apiResponse = ApiResponse<object>.Unauthorized(securityTokenException.Message);
                    break;
                case UnauthorizedAccessException unauthorizedAccessException:
                    statusCode = StatusCodes.Status401Unauthorized;
                    apiResponse = ApiResponse<object>.Unauthorized(unauthorizedAccessException.Message);
                    break;
                case KeyNotFoundException notFoundException:
                    statusCode = StatusCodes.Status404NotFound;
                    apiResponse = ApiResponse<object>.NotFound(notFoundException.Message);
                    break;
                case InvalidOperationException invalidOperationException:
                    statusCode = StatusCodes.Status500InternalServerError;
                    apiResponse = ApiResponse<object>.Error(statusCode, invalidOperationException.Message);
                    break;
                // 可以继续添加其他类型的异常处理...
                default:
                    // 默认情况保持不变
                    break;
            }

            // 设置响应的内容类型和状态码
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = statusCode;

            // 返回序列化后的响应
            return context.Response.WriteAsJsonAsync(apiResponse);
        }
    }

    /// <summary>
    /// 扩展方法，用于简化全局异常处理中间件的注册。
    /// </summary>
    public static class MiddlewareExtensions
    {
        /// <summary>
        /// 注册全局异常处理中间件的扩展方法。
        /// </summary>
        /// <param name="app">应用构建器。</param>
        /// <returns>配置后的应用构建器。</returns>
        public static IApplicationBuilder UseGlobalExceptionHandling(this IApplicationBuilder app)
        {
            return app.UseMiddleware<GlobalExceptionHandlingMiddleware>();
        }
    }
}