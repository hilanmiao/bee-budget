using Bedrock.Core.Entities;

namespace Bedrock.Application.Interfaces
{
    /// <summary>
    /// 用户角色关联仓储接口，定义了对用户角色关联的基本数据操作。
    /// </summary>
    public interface ISysUserRoleRepository
    {
        /// <summary>
        /// 创建用户角色关联。
        /// </summary>
        /// <param name="entity">要创建的用户角色关联实体对象。</param>
        /// <returns>返回受影响的记录数（通常为 1，若记录不存在则为 0）。</returns>
        Task<int> CreateAsync(SysUserRole entity);

        /// <summary>
        /// 批量创建用户角色关联。
        /// </summary>
        /// <param name="entities">要创建的用户角色关联实体对象集合。</param>
        /// <returns>返回成功插入的记录数量；若集合为空，则返回 0。</returns>
        Task<int> CreateBatchAsync(IEnumerable<SysUserRole> entities);

        /// <summary>
        /// 删除用户角色关联。
        /// </summary>
        /// <param name="entity">要删除的用户角色关联实体对象。</param>
        /// <returns>返回成功标记为删除的记录数量。</returns>
        Task<int> DeleteAsync(SysUserRole entity);

        /// <summary>
        /// 根据角色ID删除所有关联的用户。
        /// </summary>
        /// <param name="roleId">角色ID。</param>
        /// <returns>返回成功标记为删除的记录数量。</returns>
        Task<int> DeleteByRoleIdAsync(long roleId);

        /// <summary>
        /// 根据用户ID删除所有关联的角色。
        /// </summary>
        /// <param name="userId">用户ID。</param>
        /// <returns>返回成功标记为删除的记录数量。</returns>
        Task<int> DeleteByUserIdAsync(long userId);

        /// <summary>
        /// 根据角色ID获取所有关联的用户ID。
        /// </summary>
        /// <param name="roleId">角色ID。</param>
        /// <returns>返回与角色关联的用户ID集合。</returns>
        Task<List<long>> GetUserIdsByRoleIdAsync(long roleId);

        /// <summary>
        /// 根据用户ID获取所有关联的角色ID。
        /// </summary>
        /// <param name="userId">用户ID。</param>
        /// <returns>返回与用户关联的角色ID集合。</returns>
        Task<List<long>> GetRoleIdsByUserIdAsync(long userId);

        /// <summary>
        /// 检查角色和用户是否已关联。
        /// </summary>
        /// <param name="roleId">角色ID。</param>
        /// <param name="userId">用户ID。</param>
        /// <returns>如果已关联返回true，否则返回false。</returns>
        Task<bool> ExistsAsync(long roleId, long userId);

        /// <summary>
        /// 获取所有用户角色关联。
        /// </summary>
        /// <returns>返回所有用户角色关联的集合。</returns>
        Task<List<SysUserRole>> GetAllAsync();

        /// <summary>
        /// 根据用户ID列表，批量软删除用户记录，并记录操作人。
        /// </summary>
        /// <param name="userIds">要删除的用户 ID 列表。</param>
        /// <returns>返回成功标记为删除的记录数量；若用户 ID 列表为空，则返回 0。</returns>        
        Task<int> DeleteByUserIdsAsync(IEnumerable<long> userIds);

    }
} 