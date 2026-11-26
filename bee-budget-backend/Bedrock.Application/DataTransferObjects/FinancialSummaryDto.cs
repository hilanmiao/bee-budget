using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bedrock.Application.DataTransferObjects
{
    /// <summary>
    /// 返回多周期财务汇总信息时使用的数据传输对象。
    /// </summary>
    public class FinancialSummaryDto
    {
        /// <summary>
        /// 本年收入总额。
        /// </summary>
        public decimal? CurrentYearIncomeTotal { get; set; }
        /// <summary>
        /// 本年支出总额。
        /// </summary>
        public decimal? CurrentYearExpenseTotal { get; set; }
        /// <summary>
        /// 上年收入总额。
        /// </summary>
        public decimal? LastYearIncomeTotal { get; set; }
        /// <summary>
        /// 上年支出总额。
        /// </summary>
        public decimal? LastYearExpenseTotal { get; set; }

        /// <summary>
        /// 本月收入总额。
        /// </summary>
        public decimal? CurrentMonthIncomeTotal { get; set; }
        /// <summary>
        /// 本月支出总额。
        /// </summary>
        public decimal? CurrentMonthExpenseTotal { get; set; }
        /// <summary>
        /// 上月收入总额。
        /// </summary>
        public decimal? LastMonthIncomeTotal { get; set; }
        /// <summary>
        /// 上月支出总额。
        /// </summary>
        public decimal? LastMonthExpenseTotal { get; set; }

        /// <summary>
        /// 本周收入总额。
        /// </summary>
        public decimal? CurrentWeekIncomeTotal { get; set; }
        /// <summary>
        /// 本周支出总额。
        /// </summary>
        public decimal? CurrentWeekExpenseTotal { get; set; }
        /// <summary>
        /// 上周收入总额。
        /// </summary>
        public decimal? LastWeekIncomeTotal { get; set; }
        /// <summary>
        /// 上周支出总额。
        /// </summary>
        public decimal? LastWeekExpenseTotal { get; set; }

        /// <summary>
        /// 本日收入总额。
        /// </summary>
        public decimal? CurrentDayIncomeTotal { get; set; }
        /// <summary>
        /// 本日支出总额。
        /// </summary>
        public decimal? CurrentDayExpenseTotal { get; set; }
        /// <summary>
        /// 昨日收入总额。
        /// </summary>
        public decimal? YesterdayIncomeTotal { get; set; }
        /// <summary>
        /// 昨日支出总额。
        /// </summary>
        public decimal? YesterdayExpenseTotal { get; set; }
    }
}
