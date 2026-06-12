namespace Bedrock.Infrastructure.Helpers
{
    /// <summary>
    /// 控制台帮助类
    /// </summary>
    public static class ConsoleHelper
    {
        /// <summary>
        /// 使用指定颜色打印信息到控制台
        /// </summary>
        /// <param name="message">要打印的信息</param>
        /// <param name="foregroundColor">前景色（文字颜色）。</param>
        /// <param name="backgroundColor">背景色</param>
        private static void WriteLine(string message, ConsoleColor foregroundColor = ConsoleColor.Gray, ConsoleColor backgroundColor = ConsoleColor.Black)
        {
            // 默认黑底灰字
            var originalForegroundColor = ConsoleColor.Gray;
            var originalBackgroundColor = ConsoleColor.Black;

            // 恢复原始颜色设置
            Console.ForegroundColor = originalForegroundColor;
            Console.BackgroundColor = originalBackgroundColor;
            //Console.WriteLine("Begin -----------------------");

            // 设置新的颜色
            Console.ForegroundColor = foregroundColor;
            Console.BackgroundColor = backgroundColor;

            // 打印消息
            Console.WriteLine(message);

            // 恢复原始颜色设置
            Console.ForegroundColor = originalForegroundColor;
            Console.BackgroundColor = originalBackgroundColor;
            //Console.WriteLine("----------------------- End");
        }

        /// <summary>
        /// 打印错误信息到控制台（红色文本）
        /// </summary>
        /// <param name="message">要打印的错误信息</param>
        public static void Error(string message)
        {
            WriteLine($"[ERROR] {message}", ConsoleColor.Red);
        }

        /// <summary>
        /// 打印警告信息到控制台（黄色文本）
        /// </summary>
        /// <param name="message">要打印的警告信息</param>
        public static void Warning(string message)
        {
            WriteLine($"[WARNING] {message}", ConsoleColor.Yellow);
        }

        /// <summary>
        /// 打印成功信息到控制台（绿色文本）
        /// </summary>
        /// <param name="message">要打印的成功信息</param>
        public static void Success(string message)
        {
            WriteLine($"[SUCCESS] {message}", ConsoleColor.Green);
        }

        /// <summary>
        /// 打印普通信息到控制台（灰色文本）
        /// </summary>
        /// <param name="message">要打印的信息</param>
        public static void Info(string message)
        {
            WriteLine(message, ConsoleColor.Gray);
        }
    }
}