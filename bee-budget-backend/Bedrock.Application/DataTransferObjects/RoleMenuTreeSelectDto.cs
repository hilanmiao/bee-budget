namespace Bedrock.Application.DataTransferObjects
{
    /// <summary>
    /// 角色菜单树选择数据传输对象
    /// </summary>
    public class RoleMenuTreeSelectDto
    {
        /// <summary>
        /// 已选中的菜单ID列表
        /// </summary>
        public List<long> CheckedKeys { get; set; } = new List<long>();

        /// <summary>
        /// 菜单树列表
        /// </summary>
        public List<TreeSelectDto> Menus { get; set; } = new List<TreeSelectDto>();
    }
} 