using Bedrock.Application.DataTransferObjects;

namespace Bedrock.Application.Interfaces
{
    /// <summary>
    /// 系统角色应用服务接口。
    /// <para>
    /// 支持软删除（通过 DeletedAt 字段标记），所有查询默认过滤已删除记录。
    /// 业务规则：Key 与 Name 均保证全局唯一。
    /// </para>
    /// </summary>
    public interface ISysRoleService
    {
        /// <summary>
        /// 创建新的角色。
        /// </summary>
        /// <param name="createDto">创建数据传输对象，包含 Key、Name 等字段。</param>
        /// <param name="operatorId">操作人用户 ID，用于审计。</param>
        /// <returns>返回新创建记录的主键 ID。</returns>
        /// <exception cref="ArgumentException">当 Key 或 Name 已存在时抛出。</exception>
        /// <exception cref="ArgumentNullException">当 createDto 为 null 时抛出。</exception>
        Task<long> CreateAsync(CreateSysRoleDto createDto, long operatorId);

        /// <summary>
        /// 更新指定的角色。
        /// </summary>
        /// <param name="updateDto">更新数据传输对象，必须包含有效 ID。</param>
        /// <param name="operatorId">操作人用户 ID。</param>
        /// <returns>返回被更新的角色 ID。</returns>
        /// <exception cref="ArgumentException">当记录不存在、已删除或更新后违反唯一性约束时抛出。</exception>
        /// <exception cref="ArgumentNullException">当 updateDto 为 null 时抛出。</exception>
        /// <exception cref="InvalidOperationException">当数据库更新未影响任何行时抛出（并发冲突）。</exception>
        Task<long> UpdateAsync(UpdateSysRoleDto updateDto, long operatorId);

        /// <summary>
        /// 软删除指定的角色。
        /// </summary>
        /// <param name="id">要删除的角色 ID。</param>
        /// <param name="operatorId">操作人用户 ID。</param>
        /// <returns>返回被删除的角色 ID。</returns>
        /// <exception cref="ArgumentException">当记录不存在或已被删除时抛出。</exception>
        Task<long> DeleteAsync(long id, long operatorId);

        /// <summary>
        /// 批量软删除角色。
        /// </summary>
        /// <param name="ids">要删除的角色 ID 列表。</param>
        /// <param name="operatorId">操作人用户 ID。</param>
        /// <returns>返回成功标记为删除的记录数量；若 ID 列表为空则返回 0。</returns>
        /// <exception cref="ArgumentException">当部分 ID 不存在或已被删除时可能抛出。</exception>
        Task<int> DeleteBatchAsync(IEnumerable<long> ids, long operatorId);

        /// <summary>
        /// 根据主键获取单条未删除的角色详情。
        /// </summary>
        /// <param name="id">角色唯一标识。</param>
        /// <returns>返回匹配的 DTO；若未找到或已删除，则返回 null。</returns>
        Task<SysRoleDto?> GetAsync(long id);

        /// <summary>
        /// 查询未删除的角色列表，支持按名称模糊搜索和状态精确筛选。
        /// </summary>
        /// <param name="name">角色名称，用于模糊搜索（可选）。</param>
        /// <param name="status">角色状态，用于精确匹配（可选）。</param>
        /// <returns>返回匹配条件的 DTO 列表（可能为空）。</returns>
        Task<List<SysRoleDto>> GetAllAsync(string? name = null, string? status = null);

        /// <summary>
        /// 分页查询未删除的角色列表，支持名称、权限字符模糊搜索和状态精确筛选。
        /// </summary>
        /// <param name="pageNumber">页码，从 1 开始。</param>
        /// <param name="pageSize">每页数量。</param>
        /// <param name="name">角色名称，用于模糊搜索（可选）。</param>
        /// <param name="key">角色权限字符，用于模糊搜索（可选）。</param>
        /// <param name="status">角色状态，用于精确匹配（可选）。</param>
        /// <param name="orderByField">排序字段（可选）。</param>
        /// <param name="orderByType">排序方式（可选）。</param>
        /// <returns>返回分页结果对象，包含数据和分页元信息。</returns>
        Task<PaginationResult<SysRoleDto>> GetPagedAsync(
            int pageNumber,
            int pageSize,
            string? name = null,
            string? key = null,
            string? status = null,
            string? orderByField = null,
            string? orderByType = null);

        /// <summary>
        /// 根据角色权限字符获取唯一未删除的角色详情。
        /// </summary>
        /// <param name="key">角色的唯一权限字符（如 admin），用于精确匹配。</param>
        /// <returns>返回匹配的 DTO；若未找到或已删除，则返回 null。</returns>
        Task<SysRoleDto?> GetByKeyAsync(string key);

        /// <summary>
        /// 根据角色名称获取唯一未删除的角色详情。
        /// </summary>
        /// <param name="name">角色的显示名称（如“管理员”），用于精确匹配。</param>
        /// <returns>返回匹配的 DTO；若未找到或已删除，则返回 null。</returns>
        Task<SysRoleDto?> GetByNameAsync(string name);

        /// <summary>
        /// 更新角色的状态。
        /// </summary>
        /// <param name="id">要更新的角色 ID。</param>
        /// <param name="status">状态</param>
        /// <param name="operatorId">操作人用户 ID。</param>
        /// <returns>返回被更新的记录 ID。</returns>
        Task<long> ChangeStatusAsync(long id, string status, long operatorId);
    }
}