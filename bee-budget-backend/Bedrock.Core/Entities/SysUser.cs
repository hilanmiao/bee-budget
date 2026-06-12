namespace Bedrock.Core.Entities
{
    /// <summary>
    /// 系统用户实体类，用于表示系统用户的基本信息。
    /// 索引：IX_SysUser_UserName、IX_SysUser_PhoneNumber
    /// </summary>
    public class SysUser
    {
        /// <summary>
        /// 唯一标识符（主键）。
        /// </summary>
        [SqlSugar.SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
        public long Id { get; set; }

        /// <summary>
        /// 用户名。
        /// </summary>
        public string UserName { get; set; } = string.Empty;

        /// <summary>
        /// 用户密码（加密后的值）。
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
        /// 用户邮箱。
        /// </summary>
        public string? Email { get; set; }

        /// <summary>
        /// 性别（0: 男, 1: 女）。
        /// </summary>
        public string? Sex { get; set; }

        /// <summary>
        /// 用户头像路径或URL。
        /// </summary>
        public string? Avatar { get; set; }

        /// <summary>
        /// 用户状态（0: 正常, 1: 停用）。
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
        /// 创建者ID，指向创建该记录的用户。
        /// </summary>
        public long? CreatedById { get; set; }

        /// <summary>
        /// 更新时间（UTC）。
        /// </summary>
        public DateTime? UpdatedAt { get; set; }

        /// <summary>
        /// 更新者ID，指向最后一次更新该记录的用户。
        /// </summary>
        public long? UpdatedById { get; set; }

        /// <summary>
        /// 删除时间（UTC）。
        /// </summary>
        public DateTime? DeletedAt { get; set; }

        /// <summary>
        /// 删除者ID，指向删除该记录的用户。
        /// </summary>
        public long? DeletedById { get; set; }
    }
}