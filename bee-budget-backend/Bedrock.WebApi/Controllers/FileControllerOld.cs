//using Bedrock.WebApi.Responses;
//using Microsoft.AspNetCore.Mvc;
//using SharpCompress.Archives;
//using SharpCompress.Common;
//using System.Security;

//namespace Bedrock.WebApi.Controllers
//{
//    /// <summary>
//    /// 文件上传与处理控制器。
//    /// </summary>
//    [ApiController]
//    [Route("api/file")]
//    public class FileController : ControllerBase
//    {
//        private readonly ILogger<FileController> _logger;
//        private readonly IWebHostEnvironment _webHostEnvironment;
//        private const long MaxFileSize = 1024 * 1024 * 1024; // 1024 MB

//        /// <summary>
//        /// 构造函数，注入 IWebHostEnvironment。
//        /// </summary>
//        /// <param name="logger">日志记录器。</param>
//        /// <param name="webHostEnvironment">用于获取 Web 根目录路径。</param>
//        public FileController(ILogger<FileController> logger, IWebHostEnvironment webHostEnvironment)
//        {
//            _logger = logger;
//            _webHostEnvironment = webHostEnvironment;
//        }

//        /// <summary>
//        /// 上传图片文件。
//        /// </summary>
//        /// <param name="file">要上传的图片文件。</param>
//        /// <returns>返回包含文件路径的 ApiResponse。</returns>
//        [HttpPost]
//        [Route("upload-image")]
//        public async Task<IActionResult> UploadImage(IFormFile file)
//        {
//            // 1. 基础验证
//            if (file == null || file.Length == 0)
//            {
//                return BadRequest(ApiResponse<string>.BadRequest("未选择图片"));
//            }

//            // 2. 扩展名验证 (防止伪造 MIME 类型)
//            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
//            var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();

//            if (string.IsNullOrEmpty(fileExtension) || !allowedExtensions.Contains(fileExtension))
//            {
//                return BadRequest(ApiResponse<string>.BadRequest("不支持的图片格式"));
//            }

//            // 3. 生成安全的文件名
//            // 保留原始扩展名，但移除文件名中可能包含的非法字符或路径遍历攻击 (../)
//            var safeFileName = Path.GetFileName(file.FileName);
//            var newFileName = $"{Guid.NewGuid()}-{safeFileName}";

//            // 构建相对路径和绝对路径
//            var relativeDir = "uploads/images";
//            var relativeFilePath = Path.Combine(relativeDir, newFileName);
//            var absoluteFilePath = Path.Combine(_webHostEnvironment.WebRootPath, relativeFilePath);

//            // 4. 【关键修复】安全地创建目录
//            // Path.GetDirectoryName 在路径为根目录时会返回 null，直接传入会导致 ArgumentNullException
//            var directoryPath = Path.GetDirectoryName(absoluteFilePath);

//            if (!string.IsNullOrEmpty(directoryPath))
//            {
//                Directory.CreateDirectory(directoryPath);
//            }
//            else
//            {
//                // 理论上 WebRootPath + uploads 不会是根目录，但为了防御性编程保留此分支
//                // 如果这里是 null，说明 absoluteFilePath 本身就是根目录，不需要创建子文件夹
//            }

//            // 5. 保存文件 (使用 using var 简化资源释放)
//            try
//            {
//                await using var stream = new FileStream(absoluteFilePath, FileMode.Create);
//                await file.CopyToAsync(stream);
//            }
//            catch (Exception ex)
//            {
//                // 记录日志 (实际项目中建议使用 ILogger)
//                _logger.LogError(ex, "文件保存失败: {FilePath}", absoluteFilePath);
//                return BadRequest(ApiResponse<string>.BadRequest($"文件保存失败: {ex.Message}"));
//            }

//            // 6. 构建返回 URL
//            // 统一使用正斜杠 (/)，并去除可能重复的斜杠
//            // Path.Combine 在 Windows 上使用反斜杠，Web URL 需要正斜杠
//            var urlPath = "/" + relativeFilePath.Replace("\\", "/");

//            // 确保不会出现 "//uploads" 这种情况 (如果 relativeFilePath 开头意外带了 /)
//            urlPath = urlPath.Replace("//", "/");

//            return Ok(ApiResponse<string>.OperationSuccess(urlPath));
//        }

//        /// <summary>
//        /// 上传普通文件。
//        /// </summary>
//        /// <param name="file">要上传的文件。</param>
//        /// <returns>返回包含文件路径的 ApiResponse。</returns>
//        [HttpPost]
//        [RequestSizeLimit(MaxFileSize)]
//        [RequestFormLimits(MultipartBodyLengthLimit = MaxFileSize)]
//        [Route("upload-file")]
//        public async Task<IActionResult> UploadFile(IFormFile file)
//        {
//            // 1. 基础验证
//            if (file == null || file.Length == 0)
//            {
//                return BadRequest(ApiResponse<string>.BadRequest("未选择文件"));
//            }

//            // 2. 扩展名验证 (防止伪造 MIME 类型)
//            var allowedExtensions = new[] {
//                ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".webp", // 图片
//                ".pdf", ".doc", ".docx", ".xls", ".xlsx", ".ppt", ".pptx", // 文档
//                ".txt", ".md", ".json", ".xml", ".csv", // 文本
//                ".zip", ".rar", ".7z" // 压缩包 (如果允许单独上传)
//            };
//            var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();

//            if (string.IsNullOrEmpty(fileExtension) || !allowedExtensions.Contains(fileExtension))
//            {
//                return BadRequest(ApiResponse<string>.BadRequest("不支持的文件格式"));
//            }

//            // 3. 生成安全的文件名
//            // 保留原始扩展名，但移除文件名中可能包含的非法字符或路径遍历攻击 (../)
//            var safeFileName = Path.GetFileName(file.FileName);
//            var newFileName = $"{Guid.NewGuid()}-{safeFileName}";

//            // 构建相对路径和绝对路径
//            var relativeDir = "uploads/files";
//            var relativeFilePath = Path.Combine(relativeDir, newFileName);
//            var absoluteFilePath = Path.Combine(_webHostEnvironment.WebRootPath, relativeFilePath);

//            // 4. 【关键修复】安全地创建目录
//            // Path.GetDirectoryName 在路径为根目录时会返回 null，直接传入会导致 ArgumentNullException
//            var directoryPath = Path.GetDirectoryName(absoluteFilePath);

//            if (!string.IsNullOrEmpty(directoryPath))
//            {
//                Directory.CreateDirectory(directoryPath);
//            }
//            else
//            {
//                // 理论上 WebRootPath + uploads 不会是根目录，但为了防御性编程保留此分支
//                // 如果这里是 null，说明 absoluteFilePath 本身就是根目录，不需要创建子文件夹
//            }

//            // 5. 保存文件 (使用 using var 简化资源释放)
//            try
//            {
//                await using var stream = new FileStream(absoluteFilePath, FileMode.Create);
//                await file.CopyToAsync(stream);
//            }
//            catch (Exception ex)
//            {
//                // 记录日志 (实际项目中建议使用 ILogger)
//                _logger.LogError(ex, "文件保存失败: {FilePath}", absoluteFilePath);
//                return BadRequest(ApiResponse<string>.BadRequest($"文件保存失败: {ex.Message}"));
//            }

//            // 6. 构建返回 URL
//            // 统一使用正斜杠 (/)，并去除可能重复的斜杠
//            // Path.Combine 在 Windows 上使用反斜杠，Web URL 需要正斜杠
//            var urlPath = "/" + relativeFilePath.Replace("\\", "/");

//            // 确保不会出现 "//uploads" 这种情况 (如果 relativeFilePath 开头意外带了 /)
//            urlPath = urlPath.Replace("//", "/");

//            return Ok(ApiResponse<string>.OperationSuccess(urlPath));
//        }

//        /// <summary>
//        /// 上传并解压 ZIP 或 RAR 文件（使用 SharpCompress）。
//        /// </summary>
//        /// <param name="file">要上传的 ZIP 或 RAR 文件。</param>
//        /// <returns>返回包含解压目录路径的 ApiResponse。</returns>
//        [HttpPost]
//        [RequestSizeLimit(MaxFileSize)]
//        [RequestFormLimits(MultipartBodyLengthLimit = MaxFileSize)]
//        [Route("upload-and-unzip")]
//        public async Task<IActionResult> UploadAndUnzip(IFormFile file)
//        {
//            // 1. 基础验证
//            if (file == null || file.Length == 0)
//                return BadRequest(ApiResponse<string>.BadRequest("未选择压缩文件"));

//            // 2. 格式验证
//            var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();
//            if (fileExtension != ".zip" && fileExtension != ".rar")
//                return BadRequest(ApiResponse<string>.BadRequest("仅支持 .zip 和 .rar 格式"));

//            // 3. 生成临时文件名并保存
//            var safeOriginalName = Path.GetFileName(file.FileName);
//            var archiveFileName = $"{Guid.NewGuid()}-{safeOriginalName}";

//            var relativeUploadDir = "uploads/temp_archives";
//            var relativeArchivePath = Path.Combine(relativeUploadDir, archiveFileName);

//            if (string.IsNullOrEmpty(_webHostEnvironment.WebRootPath))
//                return BadRequest(ApiResponse<string>.BadRequest($"服务器配置错误：WebRootPath 为空"));


//            var absoluteArchivePath = Path.Combine(_webHostEnvironment.WebRootPath, relativeArchivePath);

//            // 确保临时目录存在
//            var archiveDirPath = Path.GetDirectoryName(absoluteArchivePath);
//            if (!string.IsNullOrEmpty(archiveDirPath))
//                Directory.CreateDirectory(archiveDirPath);

//            // 保存上传的压缩包
//            try
//            {
//                await using var stream = new FileStream(absoluteArchivePath, FileMode.Create, FileAccess.Write, FileShare.None);
//                await file.CopyToAsync(stream);
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex, "保存压缩文件失败");
//                return BadRequest(ApiResponse<string>.BadRequest($"保存文件失败: {ex.Message}"));
//            }

//            // 4. 准备解压目录
//            var folderName = Path.GetFileNameWithoutExtension(archiveFileName);
//            var relativeUnzipDir = Path.Combine("uploads/unzipped", folderName);
//            var absoluteUnzipDir = Path.Combine(_webHostEnvironment.WebRootPath, relativeUnzipDir);

//            Directory.CreateDirectory(absoluteUnzipDir);

//            // 规范化目标路径，用于后续的路径遍历检查 (关键!)
//            var normalizedTargetDir = Path.GetFullPath(absoluteUnzipDir);
//            if (!normalizedTargetDir.EndsWith(Path.DirectorySeparatorChar.ToString()))
//                normalizedTargetDir += Path.DirectorySeparatorChar;

//            bool success = false; // 标记是否完全成功

//            try
//            {
//                using var archive = ArchiveFactory.OpenArchive(absoluteArchivePath);

//                foreach (var entry in archive.Entries)
//                {
//                    // 跳过目录项
//                    if (entry.IsDirectory) continue;

//                    // 检查文件名是否为空
//                    if (string.IsNullOrEmpty(entry.Key))
//                        throw new InvalidOperationException("压缩包内包含无效的空文件名");

//                    // 【安全检查 1】拒绝绝对路径
//                    if (Path.IsPathRooted(entry.Key))
//                        throw new SecurityException($"检测到绝对路径攻击: {entry.Key}");

//                    // 【安全检查 2】拒绝危险扩展名 (黑名单)
//                    //var entryExt = Path.GetExtension(entry.Key).ToLowerInvariant();
//                    //if (DangerousExtensions.Contains(entryExt))
//                    //    throw new SecurityException($"检测到危险文件类型: {entry.Key} (.{entryExt})");

//                    // 构建完整提取路径
//                    var extractPath = Path.Combine(absoluteUnzipDir, entry.Key);

//                    // 规范化提取路径
//                    string normalizedExtractPath;
//                    try
//                    {
//                        normalizedExtractPath = Path.GetFullPath(extractPath);
//                    }
//                    catch (Exception)
//                    {
//                        throw new InvalidOperationException($"非法的文件路径格式: {entry.Key}");
//                    }

//                    // 【安全检查 3】路径遍历检查 (核心防御)
//                    // 确保最终路径必须以目标目录开头
//                    if (!normalizedExtractPath.StartsWith(normalizedTargetDir, StringComparison.OrdinalIgnoreCase))
//                        throw new SecurityException($"检测到路径遍历攻击: {entry.Key}");

//                    // 创建子目录
//                    var entryDir = Path.GetDirectoryName(normalizedExtractPath);
//                    if (!string.IsNullOrEmpty(entryDir))
//                        Directory.CreateDirectory(entryDir);

//                    // 执行解压
//                    using var entryStream = entry.OpenEntryStream();
//                    using var fileStream = new FileStream(normalizedExtractPath, FileMode.Create, FileAccess.Write, FileShare.None);
//                    await entryStream.CopyToAsync(fileStream);
//                }

//                success = true; // 只有循环完整结束且无异常，才标记为成功
//            }
//            catch (InvalidFormatException ex)
//            {
//                _logger.LogWarning(ex, "压缩文件格式无效");
//                return BadRequest(ApiResponse<string>.BadRequest($"无效的压缩文件格式: {ex.Message}"));
//            }
//            catch (SecurityException ex)
//            {
//                _logger.LogWarning(ex, "安全拦截: {Message}", ex.Message);
//                return BadRequest(ApiResponse<string>.BadRequest($"安全拦截: {ex.Message}. 请检查压缩包内容。"));
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex, "解压过程出错");
//                return BadRequest(ApiResponse<string>.BadRequest($"解压失败: {ex.Message}. 请确保压缩包未损坏且不包含加密文件。"));
//            }
//            finally
//            {
//                // 1. 无论如何，删除上传的临时压缩包
//                CleanupFile(absoluteArchivePath);

//                // 2. 【严格模式回滚】：如果未完全成功，删除整个解压目录
//                // 这保证了：要么拿到完整的文件夹，要么什么都没有
//                if (!success && Directory.Exists(absoluteUnzipDir))
//                {
//                    try
//                    {
//                        Directory.Delete(absoluteUnzipDir, true);
//                    }
//                    catch (Exception ex)
//                    {
//                        _logger.LogWarning(ex, "清理失败的解压目录时出错: {Dir}", absoluteUnzipDir);
//                    }
//                }
//            }

//            // 5. 返回成功结果
//            var resultUrl = "/" + relativeUnzipDir.Replace("\\", "/");
//            resultUrl = resultUrl.Replace("//", "/");

//            return Ok(ApiResponse<string>.OperationSuccess(resultUrl));
//        }

//        // 辅助方法：清理上传的源文件
//        private void CleanupFile(string filePath)
//        {
//            try
//            {
//                if (System.IO.File.Exists(filePath))
//                {
//                    System.IO.File.Delete(filePath);
//                }
//            }
//            catch
//            {
//                // 记录日志：无法删除临时文件
//            }
//        }
//    }
//}