using System.Collections.Generic;

namespace Bedrock.Application.DataTransferObjects
{
    /// <summary>
    /// 更新系统用户DTO
    /// </summary>
    public class UpdateSysUserDto
    {
        /// <summary>
        /// 唯一标识符（主键）。
        /// </summary>
        public long Id { get; set; }
        /// <summary>
        /// 用户名；为 null 时不更新该字段。
        /// </summary>
        public string? UserName { get; set; }
        /// <summary>
        /// 新密码；为 null 时不更新该字段。
        /// </summary>
        public string? Password { get; set; }
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