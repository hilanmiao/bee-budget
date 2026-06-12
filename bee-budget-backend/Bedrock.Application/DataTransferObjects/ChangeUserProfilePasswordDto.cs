using System.ComponentModel.DataAnnotations;

namespace Bedrock.Application.DataTransferObjects
{
    /// <summary>
    /// 修改用户密码
    /// </summary>
    public class ChangeUserProfilePasswordDto
    {
        public string OldPassword { get; set; } = string.Empty;
        public string NewPassword { get; set; } = string.Empty;
    }
}