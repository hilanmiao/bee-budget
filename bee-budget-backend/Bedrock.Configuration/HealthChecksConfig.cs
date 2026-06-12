namespace Bedrock.Configuration
{
    /// <summary>
    /// 健康检查配置类，用于存储健康检查相关的数据库连接和 API URL
    /// </summary>
    public class HealthChecksConfig
    {
        /// <summary>
        /// 数据库连接字符串
        /// </summary>
        public string DatabaseConnection { get; set; } = string.Empty;

        /// <summary>
        /// 健康检查 API 的 URL
        /// </summary>
        public string HealthApiUrl { get; set; } = string.Empty;
    }
}