using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bedrock.Application.DataTransferObjects
{
    /// <summary>
    /// 返回月度交易总额信息时使用的数据传输对象。
    /// </summary>
    public class MonthlyTotalAmountDto
    {
        /// <summary>
        /// 月份（1-12）。
        /// </summary>
        public int Month { get; set; }
        /// <summary>
        /// 当月交易总额。
        /// </summary>
        public decimal TotalAmount { get; set; }
    }
}
