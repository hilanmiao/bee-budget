using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bedrock.Application.DataTransferObjects
{
    public class MonthlyTotalAmountDto
    {
        public int Month { get; set; } // 月份
        public decimal TotalAmount { get; set; } // 当月交易总额
    }
}
