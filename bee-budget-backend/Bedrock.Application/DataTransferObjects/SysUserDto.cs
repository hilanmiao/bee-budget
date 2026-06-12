namespace Bedrock.Application.DataTransferObjects
{
    /// <summary>
    /// 系统用户详情DTO
    /// </summary>
    public class SysUserDto
    {
        public long Id { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string? NickName { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Email { get; set; }
        public string? Sex { get; set; }
        public string? Avatar { get; set; }
        public string? Status { get; set; }
        public string? Remark { get; set; }
        public DateTime? CreatedAt { get; set; }
        /// <summary>
        /// 角色集合
        /// </summary>
        public IEnumerable<SysRoleMiniDto>? Roles { get; set; }
    }
}