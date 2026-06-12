//using Bedrock.Configuration;
//using Bedrock.Infrastructure.Logging;
//using log4net;
//using Microsoft.Extensions.Options;
//using RabbitMQ.Client;
//using RabbitMQ.Client.Events;
//using System.Text;
//using System.Text.Json;
//using System.Threading.Tasks;

namespace Bedrock.Infrastructure.Messaging
{
    //public class RabbitMqLogPublisher : IAsyncDisposable
    //{
    //    private IConnection _connection; // RabbitMQ连接对象
    //    private IModel _channel;         // RabbitMQ通道对象
    //    private static readonly ILog log = LogManager.GetLogger(typeof(RabbitMqLogPublisher)); // 日志记录器
    //    private readonly RabbitMQConfig _rabbitMQConfig; // RabbitMQ配置

    //    // 构造函数注入RabbitMQ配置
    //    public RabbitMqLogPublisher(IOptions<RabbitMQConfig> rabbitMQConfig)
    //    {
    //        _rabbitMQConfig = rabbitMQConfig.Value;
    //        var factory = new ConnectionFactory()
    //        {
    //            HostName = _rabbitMQConfig.HostName,
    //            UserName = _rabbitMQConfig.UserName,
    //            Password = _rabbitMQConfig.Password,
    //            Port = _rabbitMQConfig.Port,
    //            VirtualHost = _rabbitMQConfig.VirtualHost
    //        };

    //        Initialize(factory); // 初始化RabbitMQ连接和通道
    //    }

    //    // 初始化连接和通道，并启用发布者确认机制
    //    private void Initialize(ConnectionFactory factory)
    //    {
    //        try
    //        {
    //            _connection = factory.CreateConnection(); // 创建RabbitMQ连接
    //            _channel = _connection.CreateModel();     // 创建RabbitMQ通道

    //            // 声明一个持久化的队列（durable: true），确保重启后队列不会丢失
    //            _channel.QueueDeclare(queue: "log_queue", durable: true, exclusive: false, autoDelete: false, arguments: null);

    //            // 启用发布者确认机制，用于确认消息是否成功发送到RabbitMQ服务器
    //            _channel.ConfirmSelect();
    //            log.Info("RabbitMqLogPublisher has been initialized.");
    //        }
    //        catch (Exception ex)
    //        {
    //            log.Error("Error occurred while initializing RabbitMqLogPublisher.", ex);
    //            throw;
    //        }
    //    }

    //    // 异步发布消息，包含重试逻辑和持久化设置
    //    public async Task PublishMessageAsync<T>(T logEntry, string logTypeName, int retryCount = 3)
    //    {
    //        var wrapper = new LogMessageWrapper<T>
    //        {
    //            LogType = logTypeName,
    //            Log = logEntry
    //        };
    //        var json = JsonSerializer.Serialize(wrapper);
    //        var body = Encoding.UTF8.GetBytes(json);

    //        for (int i = 0; i <= retryCount; i++)
    //        {
    //            try
    //            {
    //                var properties = _channel.CreateBasicProperties();
    //                properties.Persistent = true; // 设置消息为持久化，确保即使服务器重启也不会丢失消息

    //                // 发布消息到指定队列
    //                _channel.BasicPublish(exchange: "", routingKey: "log_queue", basicProperties: properties, body: body);
    //                log.Info($"Published a new log message to 'log_queue': {json}");

    //                // 使用异步等待确认
    //                if (await WaitForConfirmsAsync())
    //                {
    //                    break; // 成功后退出循环
    //                }
    //            }
    //            catch (Exception ex)
    //            {
    //                if (i == retryCount) // 如果达到最大重试次数，则记录错误并抛出异常
    //                {
    //                    log.Error("Failed to publish message after maximum retries.", ex);
    //                    throw;
    //                }
    //                else
    //                {
    //                    log.Warn($"Error occurred while publishing message. Retrying... ({i + 1}/{retryCount})");
    //                    await Task.Delay(1000 * i); // 简单的指数退避策略等待时间
    //                }
    //            }
    //        }
    //    }

    //    // 异步等待确认的方法
    //    private Task<bool> WaitForConfirmsAsync()
    //    {
    //        var tcs = new TaskCompletionSource<bool>();

    //        void HandleBasicAck(object sender, BasicAckEventArgs args)
    //        {
    //            _channel.BasicAcks -= HandleBasicAck;
    //            _channel.BasicNacks -= HandleBasicNack;
    //            tcs.SetResult(true); // 当收到确认时设置任务完成
    //        }

    //        void HandleBasicNack(object sender, BasicNackEventArgs args)
    //        {
    //            _channel.BasicAcks -= HandleBasicAck;
    //            _channel.BasicNacks -= HandleBasicNack;
    //            tcs.SetResult(false); // 当收到拒绝时设置任务失败
    //        }

    //        _channel.BasicAcks += HandleBasicAck;
    //        _channel.BasicNacks += HandleBasicNack;

    //        return tcs.Task;
    //    }

    //    // 实现IAsyncDisposable接口，支持异步资源释放
    //    public async ValueTask DisposeAsync()
    //    {
    //        if (_channel != null)
    //        {
    //            // 同步关闭通道并释放资源
    //            _channel.Close(); // 关闭通道
    //            _channel.Dispose(); // 释放通道资源
    //        }
    //        if (_connection != null)
    //        {
    //            // 同步关闭连接并释放资源
    //            _connection.Close(); // 关闭连接
    //            _connection.Dispose(); // 释放连接资源
    //        }
    //        await Task.CompletedTask; // 返回已完成的任务，满足异步模式要求
    //    }
    //}
}