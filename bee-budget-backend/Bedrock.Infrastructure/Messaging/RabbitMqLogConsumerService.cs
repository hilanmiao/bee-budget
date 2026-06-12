//using Bedrock.Infrastructure.Logging;
//using log4net;
//using Microsoft.Extensions.Hosting;
//using Microsoft.Extensions.Options;
//using RabbitMQ.Client.Events;
//using RabbitMQ.Client;
//using SqlSugar;
//using System.Text;
//using System.Text.Json;
//using System.Threading.Tasks;
//using System.Collections.Generic;
//using Bedrock.Configuration;

namespace Bedrock.Infrastructure.Messaging
{
    //public class RabbitMqLogConsumerService : BackgroundService, IAsyncDisposable
    //{
    //    private readonly IConnection _connection; // RabbitMQ连接对象
    //    private readonly IModel _channel;         // RabbitMQ通道对象
    //    private readonly ISqlSugarClient _db;      // 数据库客户端
    //    private readonly RabbitMQConfig _rabbitMQConfig; // RabbitMQ配置
    //    private static readonly ILog log = LogManager.GetLogger(typeof(RabbitMqLogConsumerService)); // 日志记录器
    //    private readonly Dictionary<string, ILogHandler> _logHandlers; // 日志处理器映射表

    //    // 构造函数注入RabbitMQ配置、数据库客户端和日志处理器
    //    public RabbitMqLogConsumerService(
    //        IOptions<RabbitMQConfig> rabbitMQConfig,
    //        ISqlSugarClient db,
    //        IEnumerable<ILogHandler> logHandlers)
    //    {
    //        _db = db;
    //        _rabbitMQConfig = rabbitMQConfig.Value;
    //        var factory = new ConnectionFactory()
    //        {
    //            HostName = _rabbitMQConfig.HostName,
    //            UserName = _rabbitMQConfig.UserName,
    //            Password = _rabbitMQConfig.Password,
    //            Port = _rabbitMQConfig.Port,
    //            VirtualHost = _rabbitMQConfig.VirtualHost
    //        };

    //        // 创建连接并初始化通道
    //        _connection = factory.CreateConnection();
    //        _channel = _connection.CreateModel();

    //        // 声明一个持久化的队列（durable: true），确保重启后队列不会丢失
    //        _channel.QueueDeclare(queue: "log_queue", durable: true, exclusive: false, autoDelete: false, arguments: null);
    //        log.Info("RabbitMqLogConsumerService has been initialized and is listening to 'log_queue'.");

    //        // 注册日志处理器
    //        _logHandlers = new Dictionary<string, ILogHandler>();
    //        foreach (var handler in logHandlers)
    //        {
    //            var handlerType = handler.GetType().Name;
    //            _logHandlers[handlerType] = handler;
    //        }
    //    }

    //    // 执行后台服务任务
    //    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    //    {
    //        log.Info("Starting to consume messages from 'log_queue'.");
    //        var consumer = new EventingBasicConsumer(_channel);
    //        consumer.Received += async (model, ea) =>
    //        {
    //            log.Info("Message received.");
    //            var body = ea.Body.ToArray();
    //            var message = Encoding.UTF8.GetString(body);
    //            log.Info($"Received a new log message: {message}");

    //            try
    //            {
    //                await HandleLogMessage(message);
    //            }
    //            catch (Exception ex)
    //            {
    //                log.Error("Failed to handle log message.", ex);
    //            }
    //            finally
    //            {
    //                // 发送确认，告知RabbitMQ该消息已被成功处理，可以从队列中移除
    //                _channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
    //            }
    //        };

    //        // 开始消费消息，autoAck 设置为 false，确保只有在消息被确认后才从队列中移除
    //        _channel.BasicConsume(queue: "log_queue", autoAck: false, consumer: consumer);

    //        log.Info("Started consuming messages from 'log_queue'.");
    //        return Task.CompletedTask;
    //    }

    //    // 处理接收到的日志消息
    //    private async Task HandleLogMessage(string logMessage)
    //    {
    //        var wrapper = JsonSerializer.Deserialize<LogMessageWrapper<object>>(logMessage);
    //        if (wrapper == null || !_logHandlers.ContainsKey(wrapper.LogType))
    //        {
    //            log.Warn("No matching log type found for the received message.");
    //            return;
    //        }

    //        int maxRetries = 3;
    //        for (int i = 0; i < maxRetries; i++)
    //        {
    //            try
    //            {
    //                await ((ILogHandler)_logHandlers[wrapper.LogType]).HandleAsync(logMessage);
    //                break; // 成功处理后退出循环
    //            }
    //            catch (Exception ex)
    //            {
    //                if (i == maxRetries - 1) // 达到最大重试次数
    //                {
    //                    log.Error("Failed to handle log message after maximum retries.", ex);
    //                    throw;
    //                }
    //                else
    //                {
    //                    log.Warn($"Error occurred while handling log message. Retrying... ({i + 1}/{maxRetries})");
    //                    await Task.Delay(1000 * i); // 指数退避策略等待时间
    //                }
    //            }
    //        }
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
