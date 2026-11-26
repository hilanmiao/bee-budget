using System.Collections.Generic;

namespace Bedrock.Application.DataTransferObjects
{
    /// <summary>
    /// 创建系统用户DTO
    /// </summary>
    public class CreateSysUserDto
    {
        /// <summary>
        /// 用户名。
        /// </summary>
        public string UserName { get; set; } = string.Empty;
        /// <summary>
        /// 密码。
        /// </summary>
        public string Password { get; set; } = string.Empty;
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
        /// 角色ID集合
        /// </summary>
        public List<long> RoleIds { get; set; } = new();
    }
} 