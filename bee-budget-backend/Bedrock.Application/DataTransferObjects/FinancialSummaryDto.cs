using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bedrock.Application.DataTransferObjects
{
    public class FinancialSummaryDto
    {
        public decimal? CurrentYearIncomeTotal { get; set; }
        public decimal? CurrentYearExpenseTotal { get; set; }
        public decimal? LastYearIncomeTotal { get; set; }
        public decimal? LastYearExpenseTotal { get; set; }

        public decimal? CurrentMonthIncomeTotal { get; set; }
        public decimal? CurrentMonthExpenseTotal { get; set; }
        public decimal? LastMonthIncomeTotal { get; set; }
        public decimal? LastMonthExpenseTotal { get; set; }

        public decimal? CurrentWeekIncomeTotal { get; set; }
        public decimal? CurrentWeekExpenseTotal { get; set; }
        public decimal? LastWeekIncomeTotal { get; set; }
        public decimal? LastWeekExpenseTotal { get; set; }

        public decimal? CurrentDayIncomeTotal { get; set; }
        public decimal? CurrentDayExpenseTotal { get; set; }
        public decimal? YesterdayIncomeTotal { get; set; }
        public decimal? YesterdayExpenseTotal { get; set; }
    }
}
