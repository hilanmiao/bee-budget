namespace Bedrock.Application.DataTransferObjects
{
    /// <summary>
    /// 返回交易分类统计信息时使用的数据传输对象。
    /// </summary>
    public class TransactionCategoryStatDto
    {
        /// <summary>
        /// 交易分类Id
        /// </summary>
        public long TransactionCategoryId { get; set; }

        /// <summary>
        /// 交易分类名称
        /// </summary>
        public string TransactionCategoryName { get; set; } = string.Empty;

        /// <summary>
        /// 交易分类图标
        /// </summary>
        public string? TransactionCategoryIcon { get; set; }

        /// <summary>
        /// 交易金额总和
        /// </summary>
        public decimal TransactionTotalAmount { get; set; }

        /// <summary>
        /// 交易次数
        /// </summary>
        public decimal TransactionCount { get; set; }
    }
}
