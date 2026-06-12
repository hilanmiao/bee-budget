namespace Bedrock.Core.Entities
{
    /// <summary>
    /// 系统角色菜单关联实体类，用于表示用户与角色的关联关系。
    /// </summary>
    public class SysUserRole
    {
        /// <summary>
        /// 用户ID（主键）。
        /// </summary>
        [SqlSugar.SugarColumn(IsPrimaryKey = true)]
        public long UserId { get; set; }

        /// <summary>
        /// 角色ID（主键）。
        /// </summary>
        [SqlSugar.SugarColumn(IsPrimaryKey = true)]
        public long RoleId { get; set; }
    }
}