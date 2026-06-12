namespace Bedrock.Application.ValueObjects
{
    /// <summary>
    /// 用户登录会话信息值对象（VO）
    /// 描述一个活跃的登录会话，包含 AccessToken、生命周期、设备/IP 等信息
    /// </summary>
    public class SessionInfoVO
    {
        /// <summary>
        /// 用户唯一标识
        /// </summary>
        public long UserId { get; set; }

        /// <summary>
        /// 当前会话的 AccessToken（用于 API 认证）
        /// </summary>
        public string AccessToken { get; set; } = string.Empty;

        /// <summary>
        /// 会话创建时间（登录时间，UTC）
        /// </summary>
        public DateTime LoginTime { get; set; }

        /// <summary>
        /// AccessToken 的过期时间（UTC）
        /// 通常为登录时间 + 2小时（可根据业务调整）
        /// </summary>
        public DateTime ExpiresAt { get; set; }

        /// <summary>
        /// 会话唯一标识，用于管理、撤销特定会话
        /// </summary>
        public string SessionId { get; set; } = string.Empty;

        /// <summary>
        /// 最后一次使用该会话访问 API 的时间（UTC）
        /// 用于“活跃会话检测”或“自动踢出长时间未活动用户”
        /// </summary>
        /// 太麻烦，暂时不需要
        //public DateTime? LastAccessedAt { get; set; }

        /// <summary>
        /// 登录设备标识（如 "Mac Safari", "Android App v2.1"）
        /// 用于“我的登录设备”管理页面
        /// </summary>
        public string? Device { get; set; }

        /// <summary>
        /// 登录/访问时的客户端 IP 地址
        /// 用于安全审计、异地登录告警
        /// </summary>
        public string? IpAddress { get; set; }

        /// <summary>
        /// 客户端 User-Agent 字符串
        /// 用于识别浏览器/操作系统/客户端版本
        /// </summary>
        public string? UserAgent { get; set; }

        /// <summary>
        /// （可选）关联的 RefreshTokenId，用于联动刷新或登出
        /// </summary>
        public string? RefreshTokenId { get; set; }
    }
}
