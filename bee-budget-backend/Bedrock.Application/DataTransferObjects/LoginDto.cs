namespace Bedrock.Application.DataTransferObjects
{
    public class LoginDto
    {
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string? CaptchaId { get; set; }
        public string? CaptchaContent { get; set; }
    }
}