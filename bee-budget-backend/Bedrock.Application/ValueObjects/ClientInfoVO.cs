namespace Bedrock.Application.ValueObjects
{
    /// <summary>
    /// 客户端信息值对象（VO）
    /// 设备/IP 等信息
    /// </summary>
    public class ClientInfoVO
    {
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

    }
}
