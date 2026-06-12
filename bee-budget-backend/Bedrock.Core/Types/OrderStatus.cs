using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bedrock.Core.Types
{
    /// <summary>
    /// 订单状态（业务状态机）
    /// </summary>
    public enum OrderStatus
    {
        /// <summary>
        /// 已创建，待支付
        /// </summary>
        Pending = 1,

        /// <summary>
        /// 已支付，待发货
        /// </summary>
        Paid = 2,

        /// <summary>
        /// 已发货，待收货
        /// </summary>
        Shipped = 3,

        /// <summary>
        /// 用户已确认收货，订单完成
        /// </summary>
        Completed = 4,

        /// <summary>
        /// 用户取消订单（在支付前）
        /// </summary>
        Cancelled = 5,

        /// <summary>
        /// 系统自动取消（如超时未支付）
        /// </summary>
        Expired = 6,

        /// <summary>
        /// 已退款（支付后取消）
        /// </summary>
        Refunded = 7
    }

}
