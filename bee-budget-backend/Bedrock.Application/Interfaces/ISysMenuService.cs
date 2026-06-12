using Bedrock.Application.DataTransferObjects;
using Bedrock.Application.ValueObjects;

namespace Bedrock.Application.Interfaces
{
    /// <summary>
    /// 系统菜单应用服务接口。
    /// <para>
    /// 支持软删除（通过 DeletedAt 字段标记），所有查询默认过滤已删除记录。
    /// 业务规则：Name 保证全局唯一。
    /// </para>
    /// </summary>
    public interface ISysMenuService
    {
        /// <summary>
        /// 创建新的菜单。
        /// </summary>
        /// <param name="createDto">创建数据传输对象，包含 Name 等字段。</param>
        /// <param name="operatorId">操作人用户 ID，用于审计。</param>
        /// <returns>返回新创建记录的主键 ID。</returns>
        /// <exception cref="ArgumentException">当 Name 已存在时抛出。</exception>
        /// <exception cref="ArgumentNullException">当 createDto 为 null 时抛出。</exception>
        Task<long> CreateAsync(CreateSysMenuDto createDto, long operatorId);

        /// <summary>
        /// 更新指定的菜单。
        /// </summary>
        /// <param name="updateDto">更新数据传输对象，必须包含有效 ID。</param>
        /// <param name="operatorId">操作人用户 ID。</param>
        /// <returns>返回被更新的菜单 ID。</returns>
        /// <exception cref="ArgumentException">当记录不存在、已删除或更新后违反唯一性约束时抛出。</exception>
        /// <exception cref="ArgumentNullException">当 updateDto 为 null 时抛出。</exception>
        /// <exception cref="InvalidOperationException">当数据库更新未影响任何行时抛出（并发冲突）。</exception>
        Task<long> UpdateAsync(UpdateSysMenuDto updateDto, long operatorId);

        /// <summary>
        /// 软删除指定的菜单。
        /// </summary>
        /// <param name="id">要删除的菜单 ID。</param>
        /// <param name="operatorId">操作人用户 ID。</param>
        /// <returns>返回被删除的菜单 ID。</returns>
        /// <exception cref="ArgumentException">当记录不存在或已被删除时抛出。</exception>
        Task<long> DeleteAsync(long id, long operatorId);

        /// <summary>
        /// 根据主键获取单条未删除的菜单详情。
        /// </summary>
        /// <param name="id">菜单唯一标识。</param>
        /// <returns>返回匹配的 DTO；若未找到或已删除，则返回 null。</returns>
        Task<SysMenuDto?> GetAsync(long id);

        /// <summary>
        /// 查询未删除的菜单列表，支持按名称模糊搜索和状态精确筛选。
        /// </summary>
        /// <param name="name">菜单名称，用于模糊搜索（可选）。</param>
        /// <param name="status">菜单状态，用于精确匹配（可选）。</param>
        /// <returns>返回匹配条件的 DTO 列表（可能为空）。</returns>
        Task<List<SysMenuDto>> GetAllAsync(string? name = null, string? status = null);

        /// <summary>
        /// 根据菜单名称获取唯一未删除的菜单详情。
        /// </summary>
        /// <param name="name">菜单的显示名称（如“用户管理”），用于精确匹配。</param>
        /// <returns>返回匹配的 DTO；若未找到或已删除，则返回 null。</returns>
        Task<SysMenuDto?> GetByNameAsync(string name);

        /// <summary>
        /// 查询未删除的菜单列表，支持按用户 ID 精确筛选。
        /// </summary>
        /// <param name="userId">用户 ID ，用于精确匹配（可选）。</param>
        /// <returns>返回匹配条件的 DTO 列表（可能为空）。</returns>
        Task<List<SysMenuDto>> GetAllByUserIdAsync(long userId);

        /// <summary>
        /// 查询未删除的菜单列表，支持按角色 ID 精确筛选。
        /// </summary>
        /// <param name="userId">角色 ID ，用于精确匹配（可选）。</param>
        /// <returns>返回匹配条件的 DTO 列表（可能为空）。</returns>
        Task<List<SysMenuDto>> GetAllByRoleIdAsync(long roleId);

        /// <summary>
        /// 构建前端需要的路由数据
        /// </summary>
        /// <param name="menus"></param>
        /// <returns></returns>
        List<RouterVO> BuildRouterData(List<SysMenuDto> menus);

        /// <summary>
        /// 构建前端需要的树形选择控件数据
        /// </summary>
        /// <param name="menus">菜单列表</param>
        /// <returns></returns>
        List<TreeSelectDto> BuildTreeSelectData(List<SysMenuDto> menus);

        /// <summary>
        /// 根据用户 ID 获取路由数据。
        /// </summary>
        /// <param name="userId">用户 ID</param>
        /// <returns></returns>
        Task<List<RouterVO>> GetRouterDataByUserId(long userId);

        /// <summary>
        /// 根据角色 ID 获取树形选择控件数据。
        /// </summary>
        /// <param name="roleId">角色 ID。</param>
        /// <returns></returns>
        Task<RoleMenuTreeSelectDto> GetTreeSelectDataByRoleId(long roleId);

        /// <summary>
        /// 根据用户 ID 获取树形选择控件数据。
        /// </summary>
        /// <param name="userId">用户 ID。</param>
        /// <returns></returns>
        Task<List<TreeSelectDto>> GetTreeSelectDataByUserId(long userId);
    }
}