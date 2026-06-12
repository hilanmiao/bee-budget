using System.Collections.Generic;

namespace Bedrock.Application.DataTransferObjects
{
    /// <summary>
    /// 创建系统用户DTO
    /// </summary>
    public class CreateSysUserDto
    {
        public string UserName { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
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