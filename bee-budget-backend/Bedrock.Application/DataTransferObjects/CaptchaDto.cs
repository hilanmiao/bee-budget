namespace Bedrock.Application.DataTransferObjects
{
    /// <summary>
    /// 图形验证码数据传输对象。
    /// </summary>
    public class CaptchaDto
    {
        /// <summary>
        /// 验证码的唯一标识符（如 GUID）。
        /// </summary>
        public string CaptchaId { get; set; } = string.Empty;

        /// <summary>
        /// 验证码图像的 Base64 编码字符串，格式为 data URL（data:image/png;base64,...）。
        /// </summary>
        public string ImageBase64 { get; set; } = string.Empty;

        /// <summary>
        /// 验证码生成的时间（UTC）。
        /// </summary>
        public DateTime CreatedAt { get; set; }
    }
}

