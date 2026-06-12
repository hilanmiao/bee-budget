using Bedrock.Application.Interfaces;
using Bedrock.Core.Entities;
using SqlSugar;

namespace Bedrock.Infrastructure.Repositories
{
    /// <summary>
    /// 系统菜单仓储实现。
    /// <para>
    /// 支持软删除（通过 DeletedAt 字段标记），所有查询默认过滤已删除记录。
    /// 业务规则：Name 保证全局唯一。
    /// </para>
    /// </summary>
    public class SysMenuRepository : ISysMenuRepository
    {
        private readonly ISqlSugarClient _db;

        /// <summary>
        /// 构造函数注入 SqlSugar 数据库上下文。
        /// </summary>
        /// <param name="db">数据库客户端实例。</param>
        public SysMenuRepository(ISqlSugarClient db)
        {
            _db = db;
        }

        /// <summary>
        /// 创建一条新的菜单记录。
        /// </summary>
        /// <param name="entity">待创建的实体。主键由数据库生成。</param>
        /// <returns>返回插入后生成的主键 ID。</returns>
        public async Task<long> CreateAsync(SysMenu entity)
        {
            var id = await _db.Insertable(entity).ExecuteReturnBigIdentityAsync();
            return id;
        }

        /// <summary>
        /// 批量创建菜单记录。
        /// </summary>
        /// <param name="entities">待创建的实体集合。</param>
        /// <returns>返回成功插入的记录数量；若集合为空，则返回 0。</returns>
        public async Task<int> CreateBatchAsync(IEnumerable<SysMenu> entities)
        {
            var entityArray = entities as SysMenu[] ?? entities.ToArray();
            if (!entityArray.Any())
                return 0;

            var rowsAffected = await _db.Insertable(entityArray).ExecuteCommandAsync();
            return rowsAffected;
        }

        /// <summary>
        /// 更新一条菜单记录。
        /// </summary>
        /// <param name="entity">包含更新数据的实体，必须包含有效 Id。</param>
        /// <returns>返回受影响的记录数（通常为 1，若记录不存在则为 0）。</returns>
        public async Task<int> UpdateAsync(SysMenu entity)
        {
            var rowsAffected = await _db.Updateable(entity).ExecuteCommandAsync();
            return rowsAffected;
        }

        /// <summary>
        /// 批量更新菜单记录。
        /// </summary>
        /// <param name="entities">待更新的实体集合，每个实体必须包含有效 Id。</param>
        /// <returns>返回所有更新操作受影响的总记录数；若集合为空，则返回 0。</returns>
        public async Task<int> UpdateBatchAsync(IEnumerable<SysMenu> entities)
        {
            var entityArray = entities as SysMenu[] ?? entities.ToArray();
            if (!entityArray.Any())
                return 0;

            var rowsAffected = await _db.Updateable(entityArray).ExecuteCommandAsync();
            return rowsAffected;
        }

        /// <summary>
        /// 软删除指定的菜单记录（标记 DeletedAt 和 DeletedById）。
        /// </summary>
        /// <param name="entity">要删除的实体，用于获取 Id 和审计信息。</param>
        /// <returns>返回受影响的记录数（通常为 1，若记录不存在则为 0）。</returns>
        public async Task<int> DeleteAsync(SysMenu entity)
        {
            var rowsAffected = await _db.Updateable(entity).ExecuteCommandAsync();
            return rowsAffected;
        }

        /// <summary>
        /// 批量软删除菜单记录，并记录操作人。
        /// </summary>
        /// <param name="ids">要删除的记录 ID 列表。</param>
        /// <param name="operatorId">执行删除操作的用户 ID，用于审计。</param>
        /// <returns>返回成功标记为删除的记录数量；若 ID 列表为空，则返回 0。</returns>
        public async Task<int> DeleteBatchAsync(IEnumerable<long> ids, long operatorId)
        {
            var idList = ids as long[] ?? ids.ToArray();
            if (!idList.Any())
                return 0;

            var rowsAffected = await _db.Updateable<SysMenu>()
                .SetColumns(e => new SysMenu
                {
                    DeletedAt = DateTime.UtcNow,
                    DeletedById = operatorId
                })
                .Where(e => idList.Contains(e.Id))
                .ExecuteCommandAsync();

            return rowsAffected;
        }

        /// <summary>
        /// 根据主键获取单条未删除的菜单记录。
        /// </summary>
        /// <param name="id">记录的唯一标识符。</param>
        /// <returns>匹配的实体；若未找到或已删除，则返回 null。</returns>
        public async Task<SysMenu> GetAsync(long id)
        {
            var entity = await _db.Queryable<SysMenu>()
                .Where(e => e.Id == id && e.DeletedAt == null)
                .FirstAsync();

            return entity;
        }

        /// <summary>
        /// 根据 ID 列表批量获取未删除的菜单实体。
        /// </summary>
        /// <param name="ids">菜单 ID 列表。</param>
        /// <returns>返回匹配的实体列表（可能为空）。</returns>
        public async Task<List<SysMenu>> GetByIdsAsync(IEnumerable<long> ids)
        {
            if (ids == null)
                return new List<SysMenu>();

            var idList = ids as long[] ?? ids.ToArray();
            if (!idList.Any())
                return new List<SysMenu>();

            return await _db.Queryable<SysMenu>()
                .Where(x => idList.Contains(x.Id))
                .Where(x => x.DeletedAt == null)
                .ToListAsync();
        }

        /// <summary>
        /// 查询未删除的菜单记录，支持按名称模糊搜索和状态精确筛选。
        /// </summary>
        /// <param name="name">菜单名称，用于模糊搜索（可选）。不传则忽略该条件。</param>
        /// <param name="status">菜单状态，用于精确匹配（可选）。不传则忽略该条件。</param>
        /// <returns>匹配条件的未删除记录集合（可能为空）。</returns>
        public async Task<List<SysMenu>> GetAllAsync(string? name = null, string? status = null)
        {
            var query = _db.Queryable<SysMenu>().Where(e => e.DeletedAt == null);

            if (!string.IsNullOrWhiteSpace(name))
            {
                query = query.Where(e => e.Name.Contains(name));
            }

            if (!string.IsNullOrWhiteSpace(status))
            {
                query = query.Where(e => e.Status == status);
            }

            var entities = await query
                .OrderBy(e => e.Sort, OrderByType.Asc)
                .ToListAsync();

            return entities;
        }

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
        public async Task<(List<SysMenu> Data, int TotalCount)> GetPagedAsync(
            int pageNumber,
            int pageSize,
            string? name = null,
            string? status = null)
        {
            var query = _db.Queryable<SysMenu>().Where(e => e.DeletedAt == null);

            if (!string.IsNullOrWhiteSpace(name))
            {
                query = query.Where(e => e.Name.Contains(name));
            }

            if (!string.IsNullOrWhiteSpace(status))
            {
                query = query.Where(e => e.Status == status);
            }

            var totalCount = await query.CountAsync();
            var pagedData = await query
                .OrderBy(e => e.Sort, OrderByType.Asc)
                .ToPageListAsync(pageNumber, pageSize);

            return (pagedData, totalCount);
        }

        /// <summary>
        /// 根据菜单名称获取唯一未删除的记录。
        /// </summary>
        /// <param name="name">菜单的显示名称（如“用户管理”），用于精确匹配。</param>
        /// <returns>匹配的实体；若未找到或已删除，则返回 null。由于 Name 唯一，最多返回一条。</returns>
        public async Task<SysMenu?> GetByNameAsync(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return null;

            var entity = await _db.Queryable<SysMenu>()
                .Where(e => e.Name == name && e.DeletedAt == null)
                .FirstAsync();

            return entity;
        }

        /// <summary>
        /// 查询未删除的菜单记录，支持按用户 ID 精确筛选。
        /// </summary>
        /// <param name="userId">用户 ID ，用于精确匹配。</param>
        /// <returns>匹配条件的未删除记录集合（可能为空）。</returns>
        public async Task<List<SysMenu>> GetAllByUserIdAsync(long userId)
        {
            // 有错！一个用户可能有多个角色，会导致出现重复的菜单
            //var query = _db.Queryable<SysMenu, SysRoleMenu, SysUserRole, SysRole>((m, rm, ur, r) => new object[]
            //{
            //    JoinType.Left, m.Id == rm.MenuId, // 左连接 SysMenu 和 SysRoleMenu
            //    JoinType.Left, rm.RoleId == ur.RoleId, // 左连接 SysRoleMenu 和 SysUserRole
            //    JoinType.Left, ur.RoleId == r.Id // 左连接 SysUserRole 和 SysRole
            //})
            //.Where((m, rm, ur, r) => m.DeletedAt == null);

            //if (userId != null)
            //{
            //    query = query.Where((m, rm, ur, r) => ur.UserId == userId);
            //}

            //var entities = await query
            //    .OrderBy((m, rm, ur, r) => m.Sort, OrderByType.Asc)
            //    .Select((m, rm, ur, r) => new SysMenu
            //    {
            //        Id = m.Id,
            //        Name = m.Name,
            //        ParentId = m.ParentId,
            //        RouteName = m.RouteName,
            //        Sort = m.Sort,
            //        Path = m.Path,
            //        Component = m.Component,
            //        Query = m.Query,
            //        IsFrame = m.IsFrame,
            //        IsCache = m.IsCache,
            //        MenuType = m.MenuType,
            //        Visible = m.Visible,
            //        Status = m.Status,
            //        Perms = m.Perms,
            //        Icon = m.Icon,
            //    }
            //    )
            //    .ToListAsync();

            // 1.去掉无用的第4张表 SysRole
            // 2.Left Join 会让数据库先生成大量 null 记录，然后再被 where 过滤掉 → 虽然最终效果一样，但性能更差，语义更混乱。

            // 优化
            // Step 1: 查出用户有权访问的菜单ID（去重）
            var menuIds = await _db.Queryable<SysMenu, SysRoleMenu, SysUserRole>((m, rm, ur) => new object[]
                {
                    JoinType.Inner, m.Id == rm.MenuId,     // 菜单必须被某个角色授权
                    JoinType.Inner, rm.RoleId == ur.RoleId // 该角色必须分配给了当前用户
                })
                .Where((m, rm, ur) => m.DeletedAt == null && ur.UserId == userId)
                .Select((m, rm, ur) => m.Id)
                .Distinct() // ✅ 按主键去重，绝对安全
                .ToListAsync();

            if (!menuIds.Any()) return new List<SysMenu>();

            // Step 2: 用去重后的ID查完整菜单
            var entities = await _db.Queryable<SysMenu>()
                .Where(m => menuIds.Contains(m.Id))
                .OrderBy(m => m.Sort, OrderByType.Asc)
                .ToListAsync();

            return entities;
        }

        /// <summary>
        /// 查询未删除的菜单记录，支持按角色 ID 精确筛选。
        /// </summary>
        /// <param name="userId">角色 ID ，用于精确匹配。</param>
        /// <returns>匹配条件的未删除记录集合（可能为空）。</returns>
        public async Task<List<SysMenu>> GetAllByRoleIdAsync(long roleId)
        {
            //var query = _db.Queryable<SysMenu, SysRoleMenu>((m, rm) => new object[]
            //{
            //    JoinType.Left, m.Id == rm.MenuId, // 左连接 SysMenu 和 SysRoleMenu
            //})
            //.Where((m, rm) => m.DeletedAt == null);

            //query = query.Where((m, rm) => rm.RoleId == roleId);

            //var entities = await query
            //    .OrderBy((m, rm) => m.Sort, OrderByType.Asc)
            //    .Select((m, rm) => new SysMenu
            //    {
            //        Id = m.Id,
            //        Name = m.Name,
            //        ParentId = m.ParentId,
            //        RouteName = m.RouteName,
            //        Sort = m.Sort,
            //        Path = m.Path,
            //        Component = m.Component,
            //        Query = m.Query,
            //        IsFrame = m.IsFrame,
            //        IsCache = m.IsCache,
            //        MenuType = m.MenuType,
            //        Visible = m.Visible,
            //        Status = m.Status,
            //        Perms = m.Perms,
            //        Icon = m.Icon,
            //    }
            //    )
            //    .ToListAsync();

            // 优化
            // Step 1: 查询该角色关联的所有菜单ID
            var menuIds = await _db.Queryable<SysRoleMenu>()
                .Where(rm => rm.RoleId == roleId)
                .Select(rm => rm.MenuId)
                .ToListAsync();

            if (!menuIds.Any()) return new List<SysMenu>();

            // Step 2: 查询菜单详情（无JOIN，高效！）
            var entities = await _db.Queryable<SysMenu>()
                .Where(m => menuIds.Contains(m.Id) && m.DeletedAt == null)
                .OrderBy(m => m.Sort, OrderByType.Asc)
                .ToListAsync();

            return entities;
        }

        /// <summary>
        /// 根据父级菜单 ID ，批量软删除菜单记录，并记录操作人。
        /// </summary>
        /// <param name="parentId">要删除的记录 父级菜单 ID 。</param>
        /// <param name="operatorId">执行删除操作的用户 ID，用于审计。</param>
        /// <returns>返回成功标记为删除的记录数量；若 菜单 列表为空，则返回 0。</returns>    
        public async Task<int> DeleteByParentIdAsync(long parentId, long operatorId)
        {
            var rowsAffected = await _db.Updateable<SysMenu>()
                .SetColumns(e => new SysMenu
                {
                    DeletedAt = DateTime.UtcNow,
                    DeletedById = operatorId
                })
                .Where(e => e.ParentId == parentId)
                .ExecuteCommandAsync();

            return rowsAffected;
        }
    }
}