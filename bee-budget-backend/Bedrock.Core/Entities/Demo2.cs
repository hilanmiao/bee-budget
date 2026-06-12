namespace Bedrock.Core.Entities
{
    /// <summary>
    /// 样例2实体类，用于表示样例2的基本信息。
    /// 索引：IX_Demo2_Demo1Id_DeletedAt 
    /// </summary>
    public class Demo2
    {
        /// <summary>
        /// 唯一标识符（主键）。
        /// </summary>
        [SqlSugar.SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
        public long Id { get; set; }

        /// <summary>
        /// 关联Demo1的Id。
        /// </summary>
        public long Demo1Id { get; set; }

        /// <summary>
        /// 名称。
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// 别名。
        /// </summary>
        public string? AliasName { get; set; }

        /// <summary>
        /// 编码。
        /// </summary>
        public int? Code { get; set; }

        /// <summary>
        /// 是否可见。true 表示可见，false 表示隐藏。
        /// </summary>
        public bool IsVisible { get; set; }

        /// <summary>
        /// 显示顺序。
        /// </summary>
        public int? Sort { get; set; }

        /// <summary>
        /// 状态。0 表示正常, 1 表示停用。
        /// </summary>
        public string? Status { get; set; }

        /// <summary>
        /// 备注。
        /// </summary>
        public string? Remark { get; set; }

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