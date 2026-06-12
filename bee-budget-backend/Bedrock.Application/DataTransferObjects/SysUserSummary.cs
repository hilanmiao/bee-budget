namespace Bedrock.Application.DataTransferObjects
{
    /// <summary>
    /// 用户查询摘要（用于高效 JOIN 查询）
    /// </summary>
    public class SysUserSummary
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
        public long? Id { get; set; }
        public string? Name { get; set; } = string.Empty;
        public string? Key { get; set; } = string.Empty;
    }
}