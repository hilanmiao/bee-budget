namespace Bedrock.Application.Interfaces
{
    /// <summary>
    /// 令牌清理服务接口，提供显式触发清理的能力。
    /// </summary>
    public interface ITokenCleanupService
    {
        /// <summary>
        /// 清理所有用户的过期 AccessToken 会话。
        /// </summary>
        Task CleanupExpiredTokensAsync();

        /// <summary>
        /// 清理所有用户的过期 RefreshToken 元数据。
        /// </summary>
        Task CleanupExpiredRefreshTokensAsync();
    }
}

