namespace Bedrock.Application.DataTransferObjects
{
    /// <summary>
    /// 更新个人信息DTO
    /// </summary>
    public class UpdateSysUserProfileDto
    {
        /// <summary>
        /// 唯一标识符（主键）。
        /// </summary>
        public long Id { get; set; }
        /// <summary>
        /// 昵称。
        /// </summary>
        public string? NickName { get; set; }
        /// <summary>
        /// 手机号码。
        /// </summary>
        public string? PhoneNumber { get; set; }
        /// <summary>
        /// 邮箱地址。
        /// </summary>
        public string? Email { get; set; }
        /// <summary>
        /// 性别。
        /// </summary>
        public string? Sex { get; set; }
        /// <summary>
        /// 头像地址。
        /// </summary>
        public string? Avatar { get; set; }
    }
}