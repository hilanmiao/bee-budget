using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bedrock.Application.DataTransferObjects
{
    /// <summary>
    /// 返回指定日期范围内收支汇总信息时使用的数据传输对象。
    /// </summary>
    public class DateRangeTotalAmountDto
    {
        /// <summary>
        /// 收入总额。
        /// </summary>
        public decimal? IncomeTotal { get; set; }
        /// <summary>
        /// 支出总额。
        /// </summary>
        public decimal? ExpenseTotal { get; set; }
    }
}
