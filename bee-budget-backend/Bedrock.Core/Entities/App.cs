namespace Bedrock.Core.Entities
{
    /// <summary>
    /// App应用实体类，用于表示App应用的基本信息。
    /// </summary>
    public class App
    {
        /// <summary>
        /// 唯一标识符（主键）。
        /// </summary>
        [SqlSugar.SugarColumn(IsPrimaryKey = true, IsIdentity = false)]
        public long Id { get; set; }

        /// <summary>
        /// 应用的唯一标识符，通常用于区分不同的App。
        /// </summary>
        public string AppId { get; set; } = string.Empty;

        /// <summary>
        /// 应用的名称。
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// 应用的描述信息。
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// 应用图标的URL地址。
        /// </summary>
        public string? Icon { get; set; }

        /// <summary>
        /// 应用截图的URL地址。
        /// </summary>
        public string? Screenshot { get; set; }

        /// <summary>
        /// 应用对应的H5页面的URL地址。
        /// </summary>
        public string? H5Url { get; set; }

        /// <summary>
        /// 标识该应用是否启用。true 表示启用，false 表示禁用。
        /// </summary>
        public bool IsEnabled { get; set; }

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