namespace Bedrock.Application.DataTransferObjects
{
    /// <summary>
    /// 创建文件存储中心信息时使用的数据传输对象。
    /// </summary>
    public class CreateFileStorageCenterDto
    {
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