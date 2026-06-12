using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bedrock.Core.Constants
{
    /// <summary>
    /// 业务错误码（全局唯一，便于日志追踪和前端提示）
    /// 格式：{模块}_{编号}，例如 USER_001
    /// </summary>
    public static class ErrorCodes
    {
        // 用户模块
        public const string USER_NOT_FOUND = "USER_001";
        public const string USER_EMAIL_ALREADY_EXISTS = "USER_002";
        public const string USER_INVALID_PASSWORD = "USER_003";

        // 订单模块
        public const string ORDER_NOT_FOUND = "ORDER_001";
        public const string ORDER_CANNOT_CANCEL = "ORDER_002";
        public const string ORDER_INSUFFICIENT_BALANCE = "ORDER_003";

        // 支付模块
        public const string PAYMENT_FAILED = "PAY_001";
        public const string PAYMENT_INVALID_METHOD = "PAY_002";
    }
}
