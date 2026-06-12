namespace Bedrock.Application.DataTransferObjects
{
    /// <summary>
    /// 创建交易信息时使用的数据传输对象。
    /// </summary>
    public class CreateTransactionDto
    {
        /// <summary>
        /// 关联账本的ID。
        /// </summary>
        public long LedgerId { get; set; }

        /// <summary>
        /// 关联交易分类的ID。
        /// </summary>
        public long TransactionCategoryId { get; set; }

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

    }
}