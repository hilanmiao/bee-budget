namespace Bedrock.Configuration
{
    /// <summary>
    /// RabbitMQ 配置类，用于存储 RabbitMQ 的连接和配置信息
    /// </summary>
    public class RabbitMQConfig
    {
        /// <summary>
        /// RabbitMQ 服务器的主机名或 IP 地址
        /// </summary>
        public string? HostName { get; set; }

        /// <summary>
        /// RabbitMQ 的用户名，用于身份验证
        /// </summary>
        public string? UserName { get; set; }

        /// <summary>
        /// RabbitMQ 的密码，用于身份验证
        /// </summary>
        public string? Password { get; set; }

        /// <summary>
        /// RabbitMQ 的端口号，默认为 5672（AMQP 协议）
        /// </summary>
        public int Port { get; set; }

        /// <summary>
        /// RabbitMQ 的虚拟主机，用于隔离不同的消息队列环境
        /// </summary>
        public string? VirtualHost { get; set; }
    }
}