namespace Bedrock.Application.DataTransferObjects
{
    /// <summary>
    /// 返回交易信息时使用的数据传输对象。
    /// </summary>
    public class TransactionSummary
    {
        /// <summary>
        /// 唯一标识符（主键）。
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// 关联账本的ID。
        /// </summary>
        public long LedgerId { get; set; }

        /// <summary>
        /// 关联账本的名称。
        /// </summary>
        public string LedgerName { get; set; } = string.Empty;

        /// <summary>
        /// 关联交易分类的ID。
        /// </summary>
        public long TransactionCategoryId { get; set; }

        /// <summary>
        /// 关联分类的名称。
        /// </summary>
        public string TransactionCategoryName { get; set; } = string.Empty;

        /// <summary>
        /// 关联分类的图标（可选）。
        /// </summary>
        public string? TransactionCategoryIcon { get; set; }

        /// <summary>
        /// 交易类型（收入、支出、不计入收支）。
        /// </summary>
        public string Type { get; set; } = string.Empty;

        /// <summary>
        /// 交易日期。
        /// </summary>
        public DateTime Date { get; set; }

        /// <summary>
        /// 交易描述。
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// 交易金额。
        /// </summary>
        public decimal Amount { get; set; }

        /// <summary>
        /// 状态。0 表示已完成, 1 表示已作废。
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