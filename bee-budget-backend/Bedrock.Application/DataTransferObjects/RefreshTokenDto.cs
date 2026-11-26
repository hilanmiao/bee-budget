namespace Bedrock.Application.DataTransferObjects
{
    /// <summary>
    /// 刷新访问令牌时使用的数据传输对象。
    /// </summary>
    public class RefreshTokenDto
    {
        /// <summary>
        /// 用户 ID。
        /// </summary>
        public long UserId { get; set; }
        /// <summary>
        /// 刷新令牌 ID。
        /// </summary>
        public string RefreshTokenId { get; set; } = string.Empty;
    }
}