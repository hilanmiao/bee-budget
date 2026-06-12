namespace Bedrock.Application.DataTransferObjects
{
    /// <summary>
    /// 用户状态变更数据传输对象
    /// </summary>
    public class ResetUserPasswordDto
    {
        public string Password { get; set; } = string.Empty;
    }
} 