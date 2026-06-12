using Bedrock.Application.DataTransferObjects;
using Bedrock.Application.Interfaces;
using Bedrock.WebApi.Responses;
using Microsoft.AspNetCore.Mvc;
using SharpCompress.Archives;
using SharpCompress.Common;
using System.Security;

namespace Bedrock.WebApi.Controllers
{
    /// <summary>
    /// 文件上传与处理控制器。
    /// </summary>
    [ApiController]
    [Route("api/file")]
    public class FileController : ControllerBase
    {
        private readonly ILogger<FileController> _logger;
        private readonly IFileService _fileService;
        private const long MaxFileSize = 1024 * 1024 * 1024; // 1024 MB

        /// <summary>
        /// 构造函数，注入 IWebHostEnvironment。
        /// </summary>
        /// <param name="logger">日志记录器。</param>
        /// <param name="fileService">文件服务接口，提供文件上传和处理功能。</param>
        public FileController(ILogger<FileController> logger, IFileService fileService)
        {
            _logger = logger;
            _fileService = fileService;
        }

        /// <summary>
        /// 上传图片文件。
        /// </summary>
        /// <param name="file">要上传的图片文件。</param>
        /// <returns>返回包含文件路径的 ApiResponse。</returns>
        [HttpPost]
        [Route("upload-image")]
        public async Task<IActionResult> UploadImage(IFormFile file)
        {
            var url = await _fileService.UploadImageAsync(file);
            var response = ApiResponse<string>.OperationSuccess(url);

            return Ok(response);
        }

        /// <summary>
        /// 上传通用文件。
        /// </summary>
        /// <param name="file">要上传的文件。</param>
        /// <returns>返回包含文件路径的 ApiResponse。</returns>
        [HttpPost]
        [RequestSizeLimit(MaxFileSize)]
        [RequestFormLimits(MultipartBodyLengthLimit = MaxFileSize)]
        [Route("upload-file")]
        public async Task<IActionResult> UploadFile(IFormFile file)
        {
            var url = await _fileService.UploadFileAsync(file);
            var response = ApiResponse<string>.OperationSuccess(url);

            return Ok(response);
        }

        /// <summary>
        /// 上传并解压 ZIP 或 RAR 文件（使用 SharpCompress）。
        /// </summary>
        /// <param name="file">要上传的 ZIP 或 RAR 文件。</param>
        /// <returns>返回包含解压目录路径的 ApiResponse。</returns>
        [HttpPost]
        [RequestSizeLimit(MaxFileSize)]
        [RequestFormLimits(MultipartBodyLengthLimit = MaxFileSize)]
        [Route("upload-and-unzip")]
        public async Task<IActionResult> UploadAndUnzip(IFormFile file)
        {
            var url = await _fileService.UploadAndUnzipAsync(file);
            var response = ApiResponse<string>.OperationSuccess(url);

            return Ok(response);
        }

    }
}