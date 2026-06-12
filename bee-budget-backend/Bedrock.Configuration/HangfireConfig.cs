namespace Bedrock.Configuration
{
    /// <summary>
    /// Hangfire 配置类，用于存储 Hangfire 的数据库连接和配置信息
    /// </summary>
    public class HangfireConfig
    {
        /// <summary>
        /// 数据库连接字符串
        /// </summary>
        public string DatabaseConnection { get; set; } = string.Empty;
    }
}