namespace Bedrock.Application.DataTransferObjects
{
    public class AppVersionDto
    {
        /// <summary>
        /// 唯一标识符（主键）。
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// 应用的唯一标识符，通常用于区分不同的App。
        /// </summary>
        public string AppId { get; set; } = string.Empty;

        /// <summary>
        /// 版本标题，通常用于描述该版本的主要内容或更新点。
        /// </summary>
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// 版本更新的内容说明，通常是详细的更新日志。
        /// </summary>
        public string Contents { get; set; } = string.Empty;

        /// <summary>
        /// 平台信息，例如 "Android" 或 "iOS"，用于区分版本所属的操作系统。
        /// </summary>
        public string Platform { get; set; } = string.Empty;

        /// <summary>
        /// 版本名称，例如 "1.0.0"，用于人类可读的版本号。
        /// </summary>
        public string VersionName { get; set; } = string.Empty;

        /// <summary>
        /// 版本代码，通常是一个整数，用于标识版本的内部编号。
        /// </summary>
        public int VersionCode { get; set; }

        /// <summary>
        /// 版本下载地址的URL。
        /// </summary>
        public string Url { get; set; } = string.Empty;

        /// <summary>
        /// 标识该版本是否已稳定发布。true 表示已稳定发布，false 表示未稳定发布。
        /// </summary>
        public bool IsStablePublish { get; set; }

        /// <summary>
        /// 标识该版本是否为静默更新。true 表示静默更新，false 表示非静默更新。
        /// </summary>
        public bool IsSilently { get; set; }

        /// <summary>
        /// 标识该版本是否为强制更新。true 表示强制更新，false 表示非强制更新。
        /// </summary>
        public bool IsMandatory { get; set; }

        /// <summary>
        /// 创建时间（UTC）。
        /// </summary>
        public DateTime? CreatedAt { get; set; }

        /// <summary>
        /// 创建者ID，指向创建该记录的用户。
        /// </summary>
        public long? CreatedById { get; set; }

        /// <summary>
        /// 更新时间（UTC）。
        /// </summary>
        public DateTime? UpdatedAt { get; set; }

        /// <summary>
        /// 更新者ID，指向最后一次更新该记录的用户。
        /// </summary>
        public long? UpdatedById { get; set; }

        /// <summary>
        /// 删除时间（UTC）。
        /// </summary>
        public DateTime? DeletedAt { get; set; }

        /// <summary>
        /// 删除者ID，指向删除该记录的用户。
        /// </summary>
        public long? DeletedById { get; set; }
    }

}