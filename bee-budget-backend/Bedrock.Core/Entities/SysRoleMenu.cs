namespace Bedrock.Core.Entities
{
    /// <summary>
    /// 系统角色菜单关联实体类，用于表示角色与菜单的关联关系。
    /// </summary>
    public class SysRoleMenu
    {
        /// <summary>
        /// 角色ID（主键）。
        /// </summary>
        [SqlSugar.SugarColumn(IsPrimaryKey = true)]
        public long RoleId { get; set; }

        /// <summary>
        /// 菜单ID（主键）。
        /// </summary>
        [SqlSugar.SugarColumn(IsPrimaryKey = true)]
        public long MenuId { get; set; }
    }
}