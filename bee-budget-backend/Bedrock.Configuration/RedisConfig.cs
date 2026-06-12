namespace Bedrock.Configuration
{
    /// <summary>
    /// Redis 配置类，用于存储 Redis 的连接和配置信息
    /// </summary>
    public class RedisConfig
    {
        /// <summary>
        /// Redis 的连接字符串或配置选项，例如 "localhost:6379,abortConnect=false"
        /// </summary>
        public string Configuration { get; set; } = "localhost:6379";

        /// <summary>
        /// 可选：Redis 实例的名称，用于区分不同的 Redis 实例
        /// </summary>
        public string InstanceName { get; set; } = "defaultInstance";

        /// <summary>
        /// 默认的数据库索引，默认为 0
        /// </summary>
        public int DefaultDatabase { get; set; } = 0;
    }
}