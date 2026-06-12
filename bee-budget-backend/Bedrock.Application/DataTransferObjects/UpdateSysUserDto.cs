using System.Collections.Generic;

namespace Bedrock.Application.DataTransferObjects
{
    /// <summary>
    /// 更新系统用户DTO
    /// </summary>
    public class UpdateSysUserDto
    {
        public long Id { get; set; }
        public string? UserName { get; set; }
        public string? Password { get; set; }
        public string? NickName { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Email { get; set; }
        public string? Sex { get; set; }
        public string? Avatar { get; set; }
        public string? Status { get; set; }
        public string? Remark { get; set; }
        /// <summary>
        /// 角色ID集合
        /// </summary>
        public List<long> RoleIds { get; set; } = new();
    }
} 