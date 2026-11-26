using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bedrock.Application.DataTransferObjects
{
    /// <summary>
    /// 返回交易分类金额汇总信息时使用的数据传输对象。
    /// </summary>
    public class CategoryTotalAmountDto
    {
        /// <summary>
        /// 交易分类 ID。
        /// </summary>
        public Guid CategoryId { get; set; }
        /// <summary>
        /// 交易分类名称。
        /// </summary>
        public string CategoryName { get; set; } = string.Empty;
        /// <summary>
        /// 交易分类类型（支出或收入）。
        /// </summary>
        public string CategoryType { get; set; } = string.Empty;
        /// <summary>
        /// 该交易分类下的金额合计。
        /// </summary>
        public decimal TotalAmount { get; set; }
    }
}
