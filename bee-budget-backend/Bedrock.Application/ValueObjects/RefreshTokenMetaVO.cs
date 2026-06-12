namespace Bedrock.Application.ValueObjects
{
    /// <summary>
    /// RefreshToken 的元数据值对象（VO）
    /// 用于存储 refreshToken 的上下文信息，如设备、IP、时间等，便于审计、风控、设备管理
    /// </summary>
    public class RefreshTokenMetaVO
    {
        /// <summary>
        /// 关联的用户 ID
        /// </summary>
        public long UserId { get; set; }

        /// <summary>
        /// RefreshToken 的创建时间（UTC）
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// 最后一次使用该 refreshToken 刷新 Access Token 的时间（UTC）
        /// 用于活跃度检测或自动清理
        /// </summary>
        public DateTime? LastUsedAt { get; set; }

        /// <summary>
        /// 登录设备标识（如 "iPhone 15", "Windows Chrome"）
        /// 可用于“登录设备管理”功能
        /// </summary>
        public string? Device { get; set; }

        /// <summary>
        /// 登录时的客户端 IP 地址
        /// 用于安全审计、异地登录检测
        /// </summary>
        public string? IpAddress { get; set; }

        /// <summary>
        /// 客户端 User-Agent 字符串
        /// 用于识别浏览器/操作系统/客户端类型
        /// </summary>
        public string? UserAgent { get; set; }

        /// <summary>
        /// RefreshToken 的过期时间（UTC）
        /// 通常为创建时间 + 30天（可根据业务调整）
        /// </summary>
        public DateTime ExpiresAt { get; set; }

        /// <summary>
        /// （可选）关联的会话 ID，用于联动登出
        /// </summary>
        public string? SessionId { get; set; }
    }
}
