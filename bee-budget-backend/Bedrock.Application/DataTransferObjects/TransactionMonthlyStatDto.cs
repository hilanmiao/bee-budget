namespace Bedrock.Application.DataTransferObjects
{
    /// <summary>
    /// 返回按月统计的交易收支信息时使用的数据传输对象。
    /// </summary>
    public class TransactionMonthlyStatDto
    {
        /// <summary>
        /// 月份序号（1-12），用于排序与显示。
        /// </summary>
        public int MonthIndex { get; set; }
        /// <summary>
        /// 月份显示文本，如「1月」「2月」。
        /// </summary>
        public string MonthLabel { get; set; } = string.Empty;

        // 数据库原始查询结果 (可能为 null 如果该月无数据)
        /// <summary>
        /// 数据库原始查询得到的收入金额；当月无数据时为 null。
        /// </summary>
        public decimal? RawIncomeAmount { get; set; }
        /// <summary>
        /// 数据库原始查询得到的支出金额；当月无数据时为 null。
        /// </summary>
        public decimal? RawExpenseAmount { get; set; }

        // 最终展示给前端的值 (经过逻辑处理：过去月份补0，未来月份保持null)
        /// <summary>
        /// 展示用的收入金额；过去月份无数据补 0，未来月份保持 null。
        /// </summary>
        public decimal? IncomeAmount { get; set; }
        /// <summary>
        /// 展示用的支出金额；过去月份无数据补 0，未来月份保持 null。
        /// </summary>
        public decimal? ExpenseAmount { get; set; }
    }
}
