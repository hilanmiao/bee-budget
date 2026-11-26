namespace Bedrock.Application.DataTransferObjects
{
    /// <summary>
    /// 返回登录认证结果时使用的数据传输对象。
    /// </summary>
    public class LoginResponseDto
    {
        /// <summary>
        /// 访问令牌。
        /// </summary>
        public string? AccessToken { get; set; }
        /// <summary>
        /// 访问令牌过期时间。
        /// </summary>
        public DateTime ExpiresAt { get; set; }
        /// <summary>
        /// 刷新令牌 ID。
        /// </summary>
        public string? RefreshTokenId { get; set; }
    }
}