namespace Bedrock.WebApi.Responses
{
    /// <summary>
    /// 通用的API响应结果类，用于封装API请求的结果。
    /// </summary>
    /// <typeparam name="T">响应数据的类型。</typeparam>
    public class ApiResponse<T>
    {
        /// <summary>
        /// 表示操作是否成功。
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// HTTP状态码，表示请求的状态。
        /// </summary>
        public int Code { get; set; }

        /// <summary>
        /// 返回的消息，通常用于描述错误或成功的详情。
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// 响应的数据，可以是任何类型。
        /// </summary>
        public T? Data { get; set; }

        // 私有构造函数，防止外部直接实例化
        private ApiResponse(int code, string message, T? data, bool success)
        {
            Code = code;
            Message = message;
            Data = data;
            Success = success;
        }

        /// <summary>
        /// 创建一个表示操作成功的响应。
        /// </summary>
        /// <param name="data">响应的数据。</param>
        /// <param name="message">可选的成功消息，默认为"Operation successful."。</param>
        /// <returns>包含成功信息的ApiResponse对象。</returns>
        public static ApiResponse<T> OperationSuccess(T? data, string message = "操作成功。")
        {
            return new ApiResponse<T>(200, message, data, true);
        }


        /// <summary>
        /// 创建一个表示操作失败的响应。（通信成功，但业务失败）
        /// </summary>
        /// <param name="data">响应的数据。</param>
        /// <param name="message">可选的失败消息，默认为"Operation failure."。</param>
        /// <returns>包含失败信息的ApiResponse对象。</returns>
        public static ApiResponse<T> OperationFailure(T? data, string message = "操作失败。")
        {
            return new ApiResponse<T>(200, message, data, false);
        }


        /// <summary>
        /// 创建一个表示通用错误的响应。
        /// </summary>
        /// <param name="statusCode">HTTP状态码。</param>
        /// <param name="message">错误消息。</param>
        /// <returns>包含错误信息的ApiResponse对象。</returns>
        public static ApiResponse<T> Error(int statusCode, string message)
        {
            return new ApiResponse<T>(statusCode, message, default, false);
        }

        /// <summary>
        /// 创建一个表示未授权访问的响应。
        /// </summary>
        /// <param name="message">可选的错误消息，默认为"Unauthorized"。</param>
        /// <returns>包含未授权访问信息的ApiResponse对象。</returns>
        public static ApiResponse<T> Unauthorized(string message = "未授权。")
        {
            return new ApiResponse<T>(401, message, default, false);
        }

        /// <summary>
        /// 创建一个表示禁止访问的响应。
        /// </summary>
        /// <param name="message">可选的错误消息，默认为"Forbidden"。</param>
        /// <returns>包含禁止访问信息的ApiResponse对象。</returns>
        public static ApiResponse<T> Forbidden(string message = "禁止访问。")
        {
            return new ApiResponse<T>(403, message, default, false);
        }

        /// <summary>
        /// 创建一个表示资源未找到的响应。
        /// </summary>
        /// <param name="message">可选的错误消息，默认为"Resource not found."。</param>
        /// <returns>包含资源未找到信息的ApiResponse对象。</returns>
        public static ApiResponse<T> NotFound(string message = "资源未找到。")
        {
            return new ApiResponse<T>(404, message, default, false);
        }

        /// <summary>
        /// 创建一个表示请求无效的响应。
        /// </summary>
        /// <param name="message">可选的错误消息，默认为"Bad request"。</param>
        /// <returns>包含请求无效信息的ApiResponse对象。</returns>
        public static ApiResponse<T> BadRequest(string message = "请求无效。")
        {
            return new ApiResponse<T>(400, message, default, false);
        }
    }
}