namespace Bedrock.Application.DataTransferObjects
{
    /// <summary>
    /// 系统用户详情DTO
    /// </summary>
    public class SysUserDto
    {
        /// <summary>
        /// 唯一标识符（主键）。
        /// </summary>
        public long Id { get; set; }
        /// <summary>
        /// 用户名。
        /// </summary>
        public string UserName { get; set; } = string.Empty;
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
        /// <summary>
        /// 状态。
        /// </summary>
        public string? Status { get; set; }
        /// <summary>
        /// 备注。
        /// </summary>
        public string? Remark { get; set; }
        /// <summary>
        /// 创建时间（UTC）。
        /// </summary>
        public DateTime? CreatedAt { get; set; }
        /// <summary>
        /// 角色集合
        /// </summary>
        public IEnumerable<SysRoleMiniDto>? Roles { get; set; }
    }
}