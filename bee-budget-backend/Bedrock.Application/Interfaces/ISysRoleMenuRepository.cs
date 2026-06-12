using Bedrock.Core.Entities;

namespace Bedrock.Application.Interfaces
{
    /// <summary>
    /// 角色菜单关联仓储接口，定义了对角色菜单关联的基本数据操作。
    /// </summary>
    public interface ISysRoleMenuRepository
    {
        /// <summary>
        /// 创建角色菜单关联。
        /// </summary>
        /// <param name="entity">要创建的角色菜单关联实体对象。</param>
        /// <returns>返回受影响的记录数（通常为 1，若记录不存在则为 0）。</returns>
        Task<int> CreateAsync(SysRoleMenu entity);

        /// <summary>
        /// 批量创建角色菜单关联。
        /// </summary>
        /// <param name="entities">要创建的角色菜单关联实体对象集合。</param>
        /// <returns>返回成功插入的记录数量；若集合为空，则返回 0。</returns>
        Task<int> CreateBatchAsync(IEnumerable<SysRoleMenu> entities);

        /// <summary>
        /// 删除角色菜单关联。
        /// </summary>
        /// <param name="entity">要删除的角色菜单关联实体对象。</param>
        /// <returns>返回成功标记为删除的记录数量。</returns>
        Task<int> DeleteAsync(SysRoleMenu entity);

        /// <summary>
        /// 根据角色ID删除所有关联的菜单。
        /// </summary>
        /// <param name="roleId">角色ID。</param>
        /// <returns>返回成功标记为删除的记录数量。</returns>
        Task<int> DeleteByRoleIdAsync(long roleId);

        /// <summary>
        /// 根据菜单ID删除所有关联的角色。
        /// </summary>
        /// <param name="menuId">菜单ID。</param>
        /// <returns>返回成功标记为删除的记录数量。</returns>
        Task<int> DeleteByMenuIdAsync(long menuId);

        /// <summary>
        /// 根据角色ID获取所有关联的菜单ID。
        /// </summary>
        /// <param name="roleId">角色ID。</param>
        /// <returns>返回与角色关联的菜单ID集合。</returns>
        Task<List<long>> GetMenuIdsByRoleIdAsync(long roleId);

        /// <summary>
        /// 根据菜单ID获取所有关联的角色ID。
        /// </summary>
        /// <param name="menuId">菜单ID。</param>
        /// <returns>返回与菜单关联的角色ID集合。</returns>
        Task<List<long>> GetRoleIdsByMenuIdAsync(long menuId);

        /// <summary>
        /// 检查角色和菜单是否已关联。
        /// </summary>
        /// <param name="roleId">角色ID。</param>
        /// <param name="menuId">菜单ID。</param>
        /// <returns>如果已关联返回true，否则返回false。</returns>
        Task<bool> ExistsAsync(long roleId, long menuId);

        /// <summary>
        /// 获取所有角色菜单关联。
        /// </summary>
        /// <returns>返回所有角色菜单关联的集合。</returns>
        Task<List<SysRoleMenu>> GetAllAsync();

        /// <summary>
        /// 根据角色 ID 列表，批量软删除菜单记录，并记录操作人。
        /// </summary>
        /// <param name="roleIds">要删除的角色 ID 列表。</param>
        /// <returns>返回成功标记为删除的记录数量；若角色 ID 列表为空，则返回 0。</returns>      
        Task<int> DeleteByRoleIdsAsync(IEnumerable<long> roldIds);

    }
} 