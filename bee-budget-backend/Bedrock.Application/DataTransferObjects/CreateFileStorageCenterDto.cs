namespace Bedrock.Application.DataTransferObjects
{
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