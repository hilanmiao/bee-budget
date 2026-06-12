namespace Bedrock.Application.DataTransferObjects
{
    /// <summary>
    /// 验证验证码时使用的数据传输对象。
    /// </summary>
    public class ValidateCaptchaDto
    {
        /// <summary>
        /// 验证码的唯一标识符（由生成接口返回）。
        /// </summary>
        public string CaptchaId { get; set; } = string.Empty;

        /// <summary>
        /// 验证码文本。
        /// </summary>
        public string CaptchaContent { get; set; } = string.Empty;
    }
}

