namespace Bedrock.Application.DataTransferObjects
{
    public class LoginResponseDto
    {
        public string? AccessToken { get; set; }
        public DateTime ExpiresAt { get; set; }
        public string? RefreshTokenId { get; set; }
    }
}