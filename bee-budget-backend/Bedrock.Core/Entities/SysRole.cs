namespace Bedrock.Core.Entities
{
    /// <summary>
    /// 系统角色实体类，用于表示系统角色的基本信息。
    /// </summary>
    public class SysRole
    {
        /// <summary>
        /// 唯一标识符（主键）。
        /// </summary>
        [SqlSugar.SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
        public long Id { get; set; }

        /// <summary>
        /// 角色名称。
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// 角色权限字符。
        /// </summary>
        public string Key { get; set; } = string.Empty;

        /// <summary>
        /// 数据范围（1：所有数据权限；2：自定义数据权限；3：本部门数据权限；4：本部门及以下数据权限；5：仅本人数据权限）。
        /// </summary>
        public string? DataScope { get; set; }

        /// <summary>
        /// 菜单树选择项是否关联显示（ 0：父子不互相关联显示 1：父子互相关联显示）。
        /// </summary>
        public bool? MenuCheckStrictly { get; set; }

        /// <summary>
        /// 部门树选择项是否关联显示（0：父子不互相关联显示 1：父子互相关联显示）。
        /// </summary>
        public bool? DeptCheckStrictly { get; set; }

        /// <summary>
        /// 角色状态（0正常 1停用）。
        /// </summary>
        public string? Status { get; set; }

        /// <summary>
        /// 显示顺序，用于控制角色的排序。
        /// </summary>
        public int? Sort { get; set; }

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