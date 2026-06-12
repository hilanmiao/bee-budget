namespace Bedrock.Application.DataTransferObjects
{
    public class SysMenuDto
    {
        /// <summary>
        /// 唯一标识符（主键）。
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// 菜单名称。
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// 父级菜单ID，用于构建菜单树结构。
        /// </summary>
        public long ParentId { get; set; }

        /// <summary>
        /// 路由名称。
        /// </summary>
        public string? RouteName { get; set; }

        /// <summary>
        /// 路由地址，用于前端路由匹配。
        /// </summary>
        public string? Path { get; set; }

        /// <summary>
        /// 组件路径，指向前端页面组件的位置。
        /// </summary>
        public string? Component { get; set; }

        /// <summary>
        /// 路由参数，用于附加查询字符串等信息。
        /// </summary>
        public string? Query { get; set; }

        /// <summary>
        /// 是否为外链（1: 是, 0: 否）。
        /// </summary>
        public string? IsFrame { get; set; }

        /// <summary>
        /// 是否缓存（1: 缓存, 0: 不缓存）。
        /// </summary>
        public string? IsCache { get; set; }

        /// <summary>
        /// 菜单类型（M: 目录, C: 菜单, F: 按钮）。
        /// </summary>
        public string? MenuType { get; set; }

        /// <summary>
        /// 菜单是否可见（0: 可见, 1: 隐藏）。
        /// </summary>
        public string? Visible { get; set; }

        /// <summary>
        /// 权限标识，用于权限控制。
        /// </summary>
        public string? Perms { get; set; }

        /// <summary>
        /// 菜单图标。
        /// </summary>
        public string? Icon { get; set; }

        /// <summary>
        /// 菜单状态（0: 正常, 1: 停用）。
        /// </summary>
        public string? Status { get; set; }

        /// <summary>
        /// 排序。
        /// </summary>
        public int? Sort { get; set; }

        /// <summary>
        /// 备注。
        /// </summary>
        public string? Remark { get; set; }

        /// <summary>
        /// 创建时间。
        /// </summary>
        public DateTime? CreatedAt { get; set; }

        /// <summary>
        /// 创建者ID，指向创建该记录的用户。
        /// </summary>
        public Guid? CreatedById { get; set; }

        /// <summary>
        /// 更新时间。
        /// </summary>
        public DateTime? UpdatedAt { get; set; }

        /// <summary>
        /// 更新者ID，指向最后一次更新该记录的用户。
        /// </summary>
        public Guid? UpdatedById { get; set; }

        /// <summary>
        /// 删除时间。
        /// </summary>
        public DateTime? DeletedAt { get; set; }

        /// <summary>
        /// 删除者ID，指向删除该记录的用户。
        /// </summary>
        public Guid? DeletedById { get; set; }

        /// <summary>
        /// 子菜单列表。
        /// </summary>
        public List<SysMenuDto>? Children { get; set; } = new();
    }

}