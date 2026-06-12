namespace Bedrock.Application.DataTransferObjects
{
    public class TransactionMonthlyStatDto
    {
        public int MonthIndex { get; set; } // 1-12，用于排序和显示"X月"
        public string MonthLabel { get; set; } = string.Empty; // "1月", "2月"...

        // 数据库原始查询结果 (可能为 null 如果该月无数据)
        public decimal? RawIncomeAmount { get; set; }
        public decimal? RawExpenseAmount { get; set; }

        // 最终展示给前端的值 (经过逻辑处理：过去月份补0，未来月份保持null)
        public decimal? IncomeAmount { get; set; }
        public decimal? ExpenseAmount { get; set; }
    }
}
