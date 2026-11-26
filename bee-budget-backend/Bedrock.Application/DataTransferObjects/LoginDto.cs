namespace Bedrock.Application.DataTransferObjects
{
    /// <summary>
    /// 用户登录时使用的数据传输对象。
    /// </summary>
    public class LoginDto
    {
        /// <summary>
        /// 用户名。
        /// </summary>
        public string Username { get; set; } = string.Empty;
        /// <summary>
        /// 密码。
        /// </summary>
        public string Password { get; set; } = string.Empty;
        /// <summary>
        /// 验证码 ID。
        /// </summary>
        public string? CaptchaId { get; set; }
        /// <summary>
        /// 用户输入的验证码内容。
        /// </summary>
        public string? CaptchaContent { get; set; }
    }
}