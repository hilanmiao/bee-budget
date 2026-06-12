namespace Bedrock.Core.Entities
{
    /// <summary>
    /// App设备实体类，用于表示安装App的设备的基本信息。
    /// </summary>
    public class AppDevice
    {
        /// <summary>
        /// 唯一标识符（主键）。
        /// </summary>
        [SqlSugar.SugarColumn(IsPrimaryKey = true, IsIdentity = false)]
        public Guid Id { get; set; }

        /// <summary>
        /// 应用的唯一标识符，通常用于区分不同的App。
        /// </summary>
        public string AppId { get; set; } = string.Empty;

        /// <summary>
        /// 设备的唯一标识符，通常用于区分不同的设备。
        /// </summary>
        public string DeviceId { get; set; } = string.Empty;

        /// <summary>
        /// 设备名称或型号信息。
        /// </summary>
        public string Device { get; set; } = string.Empty;

        /// <summary>
        /// 操作系统信息，例如 "Android" 或 "iOS"。
        /// </summary>
        public string Os { get; set; } = string.Empty;

        /// <summary>
        /// ORM（对象关系映射）相关信息（如果有）。
        /// </summary>
        public string Orm { get; set; } = string.Empty;

        /// <summary>
        /// 主机信息，可能包含设备的主机名或其他相关信息。
        /// </summary>
        public string Host { get; set; } = string.Empty;

        /// <summary>
        /// 设备的唯一标识符（可能是另一种格式的唯一标识）。
        /// </summary>
        public string Uni { get; set; } = string.Empty;

        /// <summary>
        /// 应用的相关信息（可能是应用版本或其他描述信息）。
        /// </summary>
        public string App { get; set; } = string.Empty;

        /// <summary>
        /// 其他附加信息，可能包含设备的额外描述或配置。
        /// </summary>
        public string Others { get; set; } = string.Empty;

        /// <summary>
        /// 创建时间（UTC）。
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// 创建者ID，指向创建该记录的用户。
        /// </summary>
        public Guid CreatedById { get; set; }

        /// <summary>
        /// 更新时间（UTC）。
        /// </summary>
        public DateTime? UpdatedAt { get; set; }

        /// <summary>
        /// 更新者ID，指向最后一次更新该记录的用户。
        /// </summary>
        public Guid? UpdatedById { get; set; }

        /// <summary>
        /// 删除时间（UTC）。
        /// </summary>
        public DateTime? DeletedAt { get; set; }

        /// <summary>
        /// 删除者ID，指向删除该记录的用户。
        /// </summary>
        public Guid? DeletedById { get; set; }
    }
}