using Bedrock.Application.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bedrock.Application.Services
{
    public class TestTaskService: ITestTaskService
    {
        private readonly ILogger<TestTaskService> _logger;

        public TestTaskService(ILogger<TestTaskService> logger)
        {
            _logger = logger;
        }

        public async Task SayHello(string name)
        {
            // 模拟耗时30秒
            await Task.Delay(1000 * 30);
            _logger.LogInformation("Hello, {Name}! This is a Hangfire job.", name);
            Console.WriteLine($"Hello, {name}!");
        }

        public void DoCriticalWork()
        {
            _logger.LogWarning("Doing critical background work...");
            // 模拟工作
            Thread.Sleep(2000);
        }
    }
}
