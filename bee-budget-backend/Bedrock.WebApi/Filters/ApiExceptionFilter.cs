using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.IdentityModel.Tokens;
using Bedrock.Infrastructure.Helpers;
using Bedrock.WebApi.Responses;

namespace Bedrock.WebApi.Filters
{
    /// <summary>
    /// 自定义API异常过滤器，用于全局处理API请求中的异常。
    /// 通过捕获未处理的异常并返回标准化的错误响应，提升API的健壮性和用户体验。
    /// </summary>
    public class ApiExceptionFilter : IExceptionFilter
    {
        // 获取日志记录器实例，用于记录异常信息
        //private static readonly ILog log = LogManager.GetLogger(typeof(ApiExceptionFilter));

        /// <summary>
        /// 当发生未处理的异常时调用此方法。
        /// 捕获异常、生成标准化错误响应，并记录详细日志。
        /// </summary>
        /// <param name="context">提供有关当前HTTP请求和响应的信息。</param>
        public void OnException(ExceptionContext context)
        {
            // 初始化错误响应对象
            ApiResponse<object> errorResponse;

            // 根据不同的异常类型生成相应的错误响应
            switch (context.Exception)
            {
                case ArgumentException argumentException:
                    // 参数异常，通常由于无效输入引起
                    errorResponse = ApiResponse<object>.BadRequest(argumentException.Message);
                    break;
                case SecurityTokenException securityTokenException:
                    // 安全令牌异常，可能由于无效或过期的JWT引起
                    errorResponse = ApiResponse<object>.Unauthorized(securityTokenException.Message);
                    break;
                case UnauthorizedAccessException unauthorizedAccessException:
                    // 未经授权访问异常，表示用户没有访问权限
                    errorResponse = ApiResponse<object>.Unauthorized(unauthorizedAccessException.Message);
                    break;
                case KeyNotFoundException keyNotFoundException:
                    // 键未找到异常，通常表示资源不存在
                    errorResponse = ApiResponse<object>.NotFound(keyNotFoundException.Message);
                    break;
                case InvalidOperationException invalidOperationException:
                    // 操作无效异常，通常表示服务器遇到了未曾预料的状态
                    errorResponse = ApiResponse<object>.Error(500, invalidOperationException.Message);
                    break;
                default:
                    // 默认情况下处理所有其他类型的异常
                    // 返回500内部服务器错误
                    errorResponse = ApiResponse<object>.Error(500, "一个未处理的API请求异常发生。");
                    break;
            }

            // 使用控制台输出错误信息
            ConsoleHelper.Error($"API Exception: {context.Exception.Message}");

            // 使用log4net记录详细的异常信息，包括堆栈跟踪
            //log.Error($"API Exception: {context.Exception.Message}", context.Exception);

            // 设置ActionResult为包含错误信息的对象结果，并指定状态码
            context.Result = new ObjectResult(errorResponse)
            {
                StatusCode = errorResponse.Code
            };

            // 标记异常已处理，防止MVC框架进一步处理该异常
            context.ExceptionHandled = true;
        }
    }
}