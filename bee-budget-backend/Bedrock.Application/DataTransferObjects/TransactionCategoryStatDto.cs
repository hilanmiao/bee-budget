namespace Bedrock.Application.DataTransferObjects
{
    /// <summary>
    /// 返回交易收支总额统计信息时使用的数据传输对象。
    /// </summary>
    public class TransactionTotalStatDto
    {
        /// <summary>
        /// 总收入金额
        /// </summary>
        public decimal TotalIncome { get; set; }

        /// <summary>
        /// 总支出金额
        /// </summary>
        public decimal TotalExpense { get; set; }
    }
}
