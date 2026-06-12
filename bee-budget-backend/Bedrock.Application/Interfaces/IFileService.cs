using Microsoft.AspNetCore.Http;

namespace Bedrock.Application.Interfaces
{
    /// <summary>
    /// 定义文件上传与处理的核心服务接口。
    /// 用于解耦文件操作逻辑，使控制器不直接依赖具体实现（如本地存储）。
    /// </summary>
    public interface IFileService
    {
        /// <summary>
        /// 上传图片文件并返回可访问的 URL 路径。
        /// </summary>
        /// <param name="file">待上传的图片文件（IFormFile）。</param>
        /// <returns>返回相对于 Web 根目录的 URL 路径（如 "/uploads/images/xxx.png"）。</returns>
        /// <exception cref="ArgumentException">当文件为空、格式不支持或保存失败时抛出。</exception>
        Task<string> UploadImageAsync(IFormFile file);

        /// <summary>
        /// 上传通用文件（文档、压缩包等）并返回 URL 路径。
        /// </summary>
        /// <param name="file">待上传的文件。</param>
        /// <returns>返回相对于 Web 根目录的 URL 路径。</returns>
        /// <exception cref="ArgumentException">当文件无效或格式不被允许时抛出。</exception>
        Task<string> UploadFileAsync(IFormFile file);

        /// <summary>
        /// 上传 ZIP 或 RAR 压缩包，自动解压并返回解压后目录的 URL 路径。
        /// </summary>
        /// <param name="file">待上传的 .zip 或 .rar 文件。</param>
        /// <returns>返回解压后文件夹的相对 URL 路径（如 "/uploads/unzipped/xxx/"）。</returns>
        /// <exception cref="ArgumentException">当文件非压缩包、损坏或包含危险路径时抛出。</exception>
        Task<string> UploadAndUnzipAsync(IFormFile file);
    }
}