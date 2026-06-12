using Bedrock.Application.Interfaces;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using SharpCompress.Archives;
using System.Security;
using System.Text.RegularExpressions;


namespace Bedrock.Application.Services
{
    /// <summary>
    /// IFileService 的本地文件系统实现。
    /// 负责处理文件上传、安全验证、目录创建及压缩包解压。
    /// </summary>
    public class FileService : IFileService
    {
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly ILogger<FileService> _logger;

        // 允许的图片扩展名
        private static readonly string[] AllowedImageExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".webp" };

        // 允许的通用文件扩展名（含文档、文本、压缩包）
        private static readonly string[] AllowedFileExtensions = {
            // 图片
            ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".webp",
            // 文档
            ".pdf", ".doc", ".docx", ".xls", ".xlsx", ".ppt", ".pptx",
            // 文本
            ".txt", ".md", ".json", ".xml", ".csv",
            // 压缩包
            ".zip", ".rar", ".7z"
        };

        public FileService(IWebHostEnvironment webHostEnvironment, ILogger<FileService> logger)
        {
            _webHostEnvironment = webHostEnvironment ?? throw new ArgumentNullException(nameof(webHostEnvironment));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// 上传图片文件并返回可访问的 URL 路径。
        /// </summary>
        /// <param name="file">待上传的图片文件（IFormFile）。</param>
        /// <returns>返回相对于 Web 根目录的 URL 路径（如 "/uploads/images/xxx.png"）。</returns>
        /// <exception cref="ArgumentException">当文件为空、格式不支持或保存失败时抛出。</exception>
        public async Task<string> UploadImageAsync(IFormFile file)
        {
            ValidateFile(file, AllowedImageExtensions);
            return await SaveFileAsync(file, "uploads/images");
        }

        /// <summary>
        /// 上传通用文件（文档、压缩包等）并返回 URL 路径。
        /// </summary>
        /// <param name="file">待上传的文件。</param>
        /// <returns>返回相对于 Web 根目录的 URL 路径。</returns>
        /// <exception cref="ArgumentException">当文件无效或格式不被允许时抛出。</exception>
        public async Task<string> UploadFileAsync(IFormFile file)
        {
            ValidateFile(file, AllowedFileExtensions);
            return await SaveFileAsync(file, "uploads/files");
        }

        /// <summary>
        /// 上传 ZIP 或 RAR 压缩包，自动解压并返回解压后目录的 URL 路径。
        /// </summary>
        /// <param name="file">待上传的 .zip 或 .rar 文件。</param>
        /// <returns>返回解压后文件夹的相对 URL 路径（如 "/uploads/unzipped/xxx/"）。</returns>
        /// <exception cref="ArgumentException">当文件非压缩包、损坏或包含危险路径时抛出。</exception>
        public async Task<string> UploadAndUnzipAsync(IFormFile file)
        {
            if (file == null || file.Length == 0)
                throw new ArgumentException("未选择压缩文件");

            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (extension != ".zip" && extension != ".rar")
                throw new ArgumentException("仅支持 .zip 和 .rar 格式");

            // 1. 获取原始文件名并分离扩展名
            var originalFileName = Path.GetFileName(file.FileName);

            // 2. 清洗文件名主体（不含扩展名）
            var fileNameWithoutExt = Path.GetFileNameWithoutExtension(originalFileName);
            var safeNamePart = SanitizeFileName(fileNameWithoutExt);

            // 3. 生成新文件名：GUID + 安全文件名 + 原始扩展名
            var newFileName = $"{Guid.NewGuid()}-{safeNamePart}{extension}";

            // 1. 保存临时压缩包
            var tempArchiveName = newFileName;
            var tempArchivePath = Path.Combine(_webHostEnvironment.WebRootPath, "uploads/temp_archives", tempArchiveName);
            EnsureDirectoryExists(Path.GetDirectoryName(tempArchivePath)!);
            await SaveToFileSystemAsync(file, tempArchivePath);

            // 2. 准备解压目录
            var folderName = Path.GetFileNameWithoutExtension(tempArchiveName);
            var unzipDirRelative = Path.Combine("uploads/unzipped", folderName);
            var unzipDirAbsolute = Path.Combine(_webHostEnvironment.WebRootPath, unzipDirRelative);
            Directory.CreateDirectory(unzipDirAbsolute);

            var normalizedTargetDir = NormalizeDirectoryPath(unzipDirAbsolute);
            bool success = false;

            try
            {
                using var archive = ArchiveFactory.OpenArchive(tempArchivePath);
                foreach (var entry in archive.Entries)
                {
                    if (entry.IsDirectory) continue;
                    if (string.IsNullOrEmpty(entry.Key))
                        throw new InvalidOperationException("压缩包内包含无效的空文件名");

                    // 安全检查：防止路径遍历
                    if (Path.IsPathRooted(entry.Key))
                        throw new SecurityException($"检测到绝对路径攻击: {entry.Key}");

                    var extractPath = Path.Combine(unzipDirAbsolute, entry.Key);
                    var normalizedExtractPath = Path.GetFullPath(extractPath);

                    if (!normalizedExtractPath.StartsWith(normalizedTargetDir, StringComparison.OrdinalIgnoreCase))
                        throw new SecurityException($"检测到路径遍历攻击: {entry.Key}");

                    EnsureDirectoryExists(Path.GetDirectoryName(normalizedExtractPath)!);
                    using var entryStream = entry.OpenEntryStream();
                    using var fs = new FileStream(normalizedExtractPath, FileMode.Create);
                    await entryStream.CopyToAsync(fs);
                }
                success = true;
                return ToUrlPath(unzipDirRelative);
            }
            finally
            {
                // 清理临时压缩包
                CleanupFile(tempArchivePath);
                // 若失败，回滚整个解压目录
                if (!success && Directory.Exists(unzipDirAbsolute))
                {
                    try { Directory.Delete(unzipDirAbsolute, true); }
                    catch { /* ignore */ }
                }
            }
        }

        #region 私有辅助方法

        /// <summary>
        /// 清洗文件名：移除或替换所有不安全字符，确保文件名在文件系统和 URL 中都安全。
        /// 允许：字母、数字、中文、下划线、连字符；其余替换为连字符。
        /// 合并多个连字符，并去除首尾连字符。
        /// </summary>
        private static string SanitizeFileName(string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName))
                return "unnamed";

            // 移除路径遍历尝试（虽然 Path.GetFileName 应已处理，但双重保险）
            fileName = fileName.Replace("..", "").Replace("/", "").Replace("\\", "");

            // 只保留安全字符：\w 包含 [a-zA-Z0-9_]，加上中文 \u4e00-\u9fff，允许连字符 -
            // 注意：点（.）也会被替换，以避免文件名中出现如 "a.b.c" 的形式，防止潜在解析歧义或显示问题。
            var safe = Regex.Replace(fileName, @"[^\w\u4e00-\u9fff\-]", "-");

            // 合并多个连续的连字符为一个
            safe = Regex.Replace(safe, @"-+", "-");

            // 去除开头和结尾的连字符
            safe = safe.Trim('-');

            // 如果结果为空（比如原文件名全是特殊符号），返回默认名
            return string.IsNullOrEmpty(safe) ? "file" : safe;
        }

        /// <summary>
        /// 验证文件是否有效且扩展名在允许列表中。
        /// </summary>
        private void ValidateFile(IFormFile file, string[] allowedExtensions)
        {
            if (file == null || file.Length == 0)
                throw new ArgumentException("文件为空");

            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (string.IsNullOrEmpty(ext) || !allowedExtensions.Contains(ext))
                throw new ArgumentException("不支持的文件格式");
        }

        /// <summary>
        /// 将文件保存到指定相对目录，并返回 Web 可访问的 URL 路径。
        /// </summary>
        private async Task<string> SaveFileAsync(IFormFile file, string relativeDir)
        {
            // 1. 获取原始文件名并分离扩展名
            var originalFileName = Path.GetFileName(file.FileName);
            var extension = Path.GetExtension(originalFileName)?.ToLowerInvariant() ?? "";

            // 2. 清洗文件名主体（不含扩展名）
            var fileNameWithoutExt = Path.GetFileNameWithoutExtension(originalFileName);
            var safeNamePart = SanitizeFileName(fileNameWithoutExt);

            // 3. 生成新文件名：GUID + 安全文件名 + 原始扩展名
            var newFileName = $"{Guid.NewGuid()}-{safeNamePart}{extension}";

            // 构建相对路径和绝对路径
            var relativePath = Path.Combine(relativeDir, newFileName);
            var absolutePath = Path.Combine(_webHostEnvironment.WebRootPath, relativePath);

            // 安全地创建目录
            EnsureDirectoryExists(Path.GetDirectoryName(absolutePath)!);

            // 保存文件
            await SaveToFileSystemAsync(file, absolutePath);

            // 构建返回 URL
            return ToUrlPath(relativePath);
        }

        /// <summary>
        /// 将文件流写入磁盘。
        /// </summary>
        private async Task SaveToFileSystemAsync(IFormFile file, string filePath)
        {
            try
            {
                await using var stream = new FileStream(filePath, FileMode.Create);
                await file.CopyToAsync(stream);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "文件保存失败: {FilePath}", filePath);
                throw new InvalidOperationException($"文件保存失败: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// 确保目录存在，若不存在则创建。
        /// </summary>
        private void EnsureDirectoryExists(string directoryPath)
        {
            if (!string.IsNullOrEmpty(directoryPath))
                Directory.CreateDirectory(directoryPath);
        }

        /// <summary>
        /// 规范化目录路径，确保以目录分隔符结尾。
        /// </summary>
        private string NormalizeDirectoryPath(string dirPath)
        {
            var normalized = Path.GetFullPath(dirPath);
            if (!normalized.EndsWith(Path.DirectorySeparatorChar.ToString()))
                normalized += Path.DirectorySeparatorChar;
            return normalized;
        }

        /// <summary>
        /// 将本地相对路径转换为 Web URL 路径（使用正斜杠）。
        /// </summary>
        private string ToUrlPath(string relativePath)
        {
            return "/" + relativePath.Replace("\\", "/").Replace("//", "/");
        }

        /// <summary>
        /// 安静地删除文件（忽略异常）。
        /// </summary>
        private void CleanupFile(string filePath)
        {
            try
            {
                if (File.Exists(filePath))
                    File.Delete(filePath);
            }
            catch
            {
                // 可记录警告日志
            }
        }

        #endregion
    }
}