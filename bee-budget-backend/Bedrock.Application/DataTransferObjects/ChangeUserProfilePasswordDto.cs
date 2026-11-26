using System.ComponentModel.DataAnnotations;

namespace Bedrock.Application.DataTransferObjects
{
    /// <summary>
    /// 修改用户密码
    /// </summary>
    public class ChangeUserProfilePasswordDto
    {
        /// <summary>
        /// 原密码。
        /// </summary>
        public string OldPassword { get; set; } = string.Empty;
        /// <summary>
        /// 新密码。
        /// </summary>
        public string NewPassword { get; set; } = string.Empty;
    }
}