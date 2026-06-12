namespace Bedrock.Application.DataTransferObjects
{
    public class RefreshTokenDto
    {
        public long UserId { get; set; }
        public string RefreshTokenId { get; set; } = string.Empty;
    }
}