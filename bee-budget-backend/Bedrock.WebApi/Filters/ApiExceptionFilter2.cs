//using Bedrock.Infrastructure.Logging;
//using log4net;
//using Microsoft.AspNetCore.Mvc.Filters;
//using Microsoft.AspNetCore.Mvc;
//using System.Text.Json;

namespace Bedrock.WebApi.Filters
{
    //public class ApiExceptionFilter2 : IAsyncExceptionFilter
    //{
    //    private readonly RabbitMqLogPublisher _rabbitMqLogPublisher;
    //    private static readonly ILog log = LogManager.GetLogger(typeof(ApiExceptionFilter));

    //    public ApiExceptionFilter2(RabbitMqLogPublisher rabbitMqLogPublisher)
    //    {
    //        _rabbitMqLogPublisher = rabbitMqLogPublisher;
    //    }

    //    public async Task OnExceptionAsync(ExceptionContext context)
    //    {
    //        var requestId = Guid.NewGuid().ToString();
    //        var loggerName = context.HttpContext?.Request.RouteValues["controller"]?.ToString() ?? "UnknownController";
    //        var requestUrl = context.HttpContext?.Request.Path.Value;
    //        var requestMethod = context.HttpContext?.Request.Method;
    //        var exceptionMessage = context.Exception.Message;

    //        // 创建ApiLog对象
    //        var apiLog = new ApiLog
    //        {
    //            RequestId = requestId,
    //            LogLevel = "ERROR",
    //            Logger = loggerName,
    //            RequestMethod = requestMethod,
    //            RequestUrl = requestUrl,
    //            RequestHeaders = JsonSerializer.Serialize(context.HttpContext.Request.Headers),
    //            Exception = exceptionMessage,
    //            DurationMs = 0, // 在异常情况下可能无法准确计算持续时间
    //            Timestamp = DateTime.Now
    //        };

    //        // 记录错误日志到本地
    //        LogToLocal(apiLog);

    //        // 序列化ApiLog对象并发送到RabbitMQ
    //        var logMessageWrapper = new LogMessageWrapper<ApiLog>
    //        {
    //            LogType = nameof(ApiLog), // 日志类型，可以用来区分不同类型的日志
    //            Log = apiLog
    //        };

    //        var serializedLogMessage = JsonSerializer.Serialize(logMessageWrapper);
    //        await SendLogToQueueAsync(serializedLogMessage);

    //        // 构建响应
    //       // var response = new ApiResponse<object>(500, "An unexpected error occurred.", exceptionMessage);

    //        //context.Result = new JsonResult(response)
    //        //{
    //        //    StatusCode = 500 // 确保状态码设置正确
    //        //};
    //        //context.ExceptionHandled = true; // 标记异常已处理，防止进一步传播
    //    }

    //    private void LogToLocal(ApiLog apiLog)
    //    {
    //        // 使用Log4net记录日志
    //        log.Error($"API Error: {JsonSerializer.Serialize(apiLog)}");
    //    }

    //    private async Task SendLogToQueueAsync(string serializedLogMessage)
    //    {
    //        try
    //        {
    //            await _rabbitMqLogPublisher.PublishMessageAsync(serializedLogMessage, nameof(ApiLog));
    //        }
    //        catch (Exception ex)
    //        {
    //            // 记录发送日志失败的情况
    //            log.Error($"Failed to send log message to RabbitMQ: {ex.Message}");
    //        }
    //    }
    //}
}