namespace Bedrock.Application.DataTransferObjects
{
    /// <summary>
    /// 用户查询摘要（用于高效 JOIN 查询）
    /// </summary>
    public class SysUserSummary
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
        /// 关联的角色摘要（查询专用）
        /// </summary>
        public IEnumerable<RoleSummary> Roles { get; set; } = Enumerable.Empty<RoleSummary>();
    }

    /// <summary>
    /// 角色摘要（用于查询投影的 DTO，支持 LEFT JOIN 空值）
    /// </summary>
    public class RoleSummary
    {
        // 所有通过 LEFT JOIN（或 RIGHT JOIN）引入的字段，都可能为 null，在 DTO 或投影中应声明为可空类型
        /// <summary>
        /// 角色 ID；LEFT JOIN 未匹配到角色时为 null。
        /// </summary>
        public long? Id { get; set; }
        /// <summary>
        /// 角色名称；LEFT JOIN 未匹配到角色时为 null。
        /// </summary>
        public string? Name { get; set; } = string.Empty;
        /// <summary>
        /// 角色标识（权限关键字）；LEFT JOIN 未匹配到角色时为 null。
        /// </summary>
        public string? Key { get; set; } = string.Empty;
    }
}