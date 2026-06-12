using System.Diagnostics;

namespace Bedrock.Application.Interfaces
{
    /// <summary>
    /// API 日志记录服务的接口。
    /// </summary>
    public interface IApiLogHelper
    {
        /// <summary>
        /// 记录 API 请求日志。
        /// </summary>
        /// <param name="logger">log4net 日志对象。</param>
        /// <param name="responseBody">响应体内容。</param>
        /// <param name="stopwatch">计时器，用于记录请求耗时。</param>
        /// <param name="loggerName">日志记录器名称。</param>
        /// <param name="requestBody">请求体内容（可选）。</param>
        //void LogApiRequest(ILog logger, object responseBody, Stopwatch stopwatch, string loggerName, object? requestBody = null);
    }

}
