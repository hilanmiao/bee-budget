namespace Bedrock.Configuration
{
    /// <summary>
    /// 用户Token清理配置类
    /// </summary>
    public class TokenCleanupConfig
    {
        /// <summary>
        /// 清理间隔，默认30分
        /// </summary>
        public TimeSpan CleanupInterval { get; set; } = TimeSpan.FromMinutes(30);
    }
}