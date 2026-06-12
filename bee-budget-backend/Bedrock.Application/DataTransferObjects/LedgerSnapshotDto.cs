namespace Bedrock.Application.DataTransferObjects
{
    /// <summary>
    /// 账本实时数据快照。
    /// 包含本日、本周、本月、本年的收入与支出总额。
    /// </summary>
    public class LedgerSnapshotDto
    {
        /// <summary>本日收入</summary>
        public decimal TodayIncome { get; set; }
        /// <summary>本日支出</summary>
        public decimal TodayExpense { get; set; }

        /// <summary>本周收入</summary>
        public decimal WeekIncome { get; set; }
        /// <summary>本周支出</summary>
        public decimal WeekExpense { get; set; }

        /// <summary>本月收入</summary>
        public decimal MonthIncome { get; set; }
        /// <summary>本月支出</summary>
        public decimal MonthExpense { get; set; }

        /// <summary>本年收入</summary>
        public decimal YearIncome { get; set; }
        /// <summary>本年支出</summary>
        public decimal YearExpense { get; set; }
    }
}