namespace Bedrock.Configuration
{
    /// <summary>
    /// 数据库配置类，用于存储数据库连接字符串
    /// </summary>
    public class DatabaseConfig
    {
        /// <summary>
        /// 默认数据库连接字符串
        /// </summary>
        public string DefaultConnection { get; set; } = string.Empty;

        /// <summary>
        /// 默认的数据库类型，例如 "MySql" 或 "SqlServer"
        /// </summary>
        public string DefaultDbType { get; set; } = string.Empty;
    }
}