using Bedrock.Core.Entities;

namespace Bedrock.Application.Interfaces
{
    /// <summary>
    /// 系统菜单仓储接口。
    /// <para>
    /// 支持软删除（通过 DeletedAt 字段标记），所有查询默认过滤已删除记录。
    /// 业务规则：Name 保证全局唯一。
    /// </para>
    /// </summary>
    public interface ISysMenuRepository
    {
        /// <summary>
        /// 创建一条新的菜单记录。
        /// </summary>
        /// <param name="entity">待创建的实体。主键由数据库生成。</param>
        /// <returns>返回插入后生成的主键 ID。</returns>
        Task<long> CreateAsync(SysMenu entity);

        /// <summary>
        /// 批量创建菜单记录。
        /// </summary>
        /// <param name="entities">待创建的实体集合。</param>
        /// <returns>返回成功插入的记录数量；若集合为空，则返回 0。</returns>
        Task<int> CreateBatchAsync(IEnumerable<SysMenu> entities);

        /// <summary>
        /// 更新一条菜单记录。
        /// </summary>
        /// <param name="entity">包含更新数据的实体，必须包含有效 Id。</param>
        /// <returns>返回受影响的记录数（通常为 1，若记录不存在则为 0）。</returns>
        Task<int> UpdateAsync(SysMenu entity);

        /// <summary>
        /// 批量更新菜单记录。
        /// </summary>
        /// <param name="entities">待更新的实体集合，每个实体必须包含有效 Id。</param>
        /// <returns>返回所有更新操作受影响的总记录数；若集合为空，则返回 0。</returns>
        Task<int> UpdateBatchAsync(IEnumerable<SysMenu> entities);

        /// <summary>
        /// 软删除指定的菜单记录（标记 DeletedAt 和 DeletedById）。
        /// </summary>
        /// <param name="entity">要删除的实体，用于获取 Id 和审计信息。</param>
        /// <returns>返回受影响的记录数（通常为 1，若记录不存在则为 0）。</returns>
        Task<int> DeleteAsync(SysMenu entity);

        /// <summary>
        /// 批量软删除菜单记录，并记录操作人。
        /// </summary>
        /// <param name="ids">要删除的记录 ID 列表。</param>
        /// <param name="operatorId">执行删除操作的用户 ID，用于审计。</param>
        /// <returns>返回成功标记为删除的记录数量；若 ID 列表为空，则返回 0。</returns>
        Task<int> DeleteBatchAsync(IEnumerable<long> ids, long operatorId);

        /// <summary>
        /// 根据主键获取单条未删除的菜单记录。
        /// </summary>
        /// <param name="id">记录的唯一标识符。</param>
        /// <returns>匹配的实体；若未找到或已删除，则返回 null。</returns>
        Task<SysMenu> GetAsync(long id);

        /// <summary>
        /// 根据 ID 列表批量获取未删除的菜单记录。
        /// </summary>
        /// <param name="ids">菜单 ID 列表。</param>
        /// <returns>返回匹配的实体列表（可能为空）。</returns>
        Task<List<SysMenu>> GetByIdsAsync(IEnumerable<long> ids);

        /// <summary>
        /// 查询未删除的菜单记录，支持按名称模糊搜索和状态精确筛选。
        /// </summary>
        /// <param name="name">菜单名称，用于模糊搜索（可选）。</param>
        /// <param name="status">菜单状态，用于精确匹配（可选）。</param>
        /// <returns>匹配条件的未删除记录集合（可能为空）。</returns>
        Task<List<SysMenu>> GetAllAsync(string? name = null, string? status = null);

        /// <summary>
        /// 分页查询未删除的菜单记录，支持名称和状态精确筛选。
        /// </summary>
        /// <param name="pageNumber">页码，从 1 开始。</param>
        /// <param name="pageSize">每页记录数。</param>
        /// <param name="name">菜单名称，用于模糊搜索（可选）。</param>
        /// <param name="status">菜单状态，用于精确匹配（可选）。</param>
        /// <returns>
        /// 元组包含分页结果：
        /// - Data：当前页的数据列表（可能为空）。
        /// - TotalCount：满足查询条件的总记录数。
        /// </returns>
        Task<(List<SysMenu> Data, int TotalCount)> GetPagedAsync(
            int pageNumber,
            int pageSize,
            string? name = null,
            string? status = null);

        /// <summary>
        /// 根据菜单名称获取唯一未删除的记录。
        /// </summary>
        /// <param name="name">菜单的显示名称（如“用户管理”），用于精确匹配。</param>
        /// <returns>匹配的实体；若未找到或已删除，则返回 null。由于 Name 唯一，最多返回一条。</returns>
        Task<SysMenu?> GetByNameAsync(string name);

        /// <summary>
        /// 查询未删除的菜单记录，支持按用户 ID 精确筛选。
        /// </summary>
        /// <param name="userId">用户 ID ，用于精确匹配。</param>
        /// <returns>匹配条件的未删除记录集合（可能为空）。</returns>
        Task<List<SysMenu>> GetAllByUserIdAsync(long userId);

        /// <summary>
        /// 查询未删除的菜单记录，支持按角色 ID 精确筛选。
        /// </summary>
        /// <param name="userId">角色 ID ，用于精确匹配。</param>
        /// <returns>匹配条件的未删除记录集合（可能为空）。</returns>
        Task<List<SysMenu>> GetAllByRoleIdAsync(long roleId);

        /// <summary>
        /// 根据父级菜单 ID ，批量软删除菜单记录，并记录操作人。
        /// </summary>
        /// <param name="parentId">要删除的记录 父级菜单 ID 。</param>
        /// <param name="operatorId">执行删除操作的用户 ID，用于审计。</param>
        /// <returns>返回成功标记为删除的记录数量；若 菜单 列表为空，则返回 0。</returns>    
        Task<int> DeleteByParentIdAsync(long parentId, long operatorId);
    }
}