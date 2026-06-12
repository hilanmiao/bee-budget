namespace Bedrock.Core.Entities
{
    /// <summary>
    /// 交易分类实体类，用于表示交易的分类信息。
    /// </summary>
    public class TransactionCategory
    {
        /// <summary>
        /// 唯一标识符（主键）。
        /// </summary>
        [SqlSugar.SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
        public long Id { get; set; }

        /// <summary>
        /// 名称。
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// 是否为公共分类。
        /// </summary>
        public bool IsPublic { get; set; }

        /// <summary>
        /// 图标（可选）。
        /// </summary>
        public string? Icon { get; set; }

        /// <summary>
        /// 用户ID（可选）。
        /// </summary>
        public long? UserId { get; set; }

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