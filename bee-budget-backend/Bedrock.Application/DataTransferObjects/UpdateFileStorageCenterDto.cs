namespace Bedrock.Application.DataTransferObjects
{
    /// <summary>
    /// 更新文件存储中心信息时使用的数据传输对象。
    /// </summary>
    public class UpdateFileStorageCenterDto
    {
        /// <summary>
        /// 唯一标识符（主键）。
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// 文件名称。
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// 文件存储路径。
        /// </summary>
        public string? StorePath { get; set; }

        /// <summary>
        /// 备注。
        /// </summary>
        public string? Remark { get; set; }

    }
}