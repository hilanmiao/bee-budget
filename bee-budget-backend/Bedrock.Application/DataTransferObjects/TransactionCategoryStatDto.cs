namespace Bedrock.Application.DataTransferObjects
{
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
