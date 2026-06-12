using Bedrock.Application.Interfaces;
using Bedrock.Core.Entities;
using SqlSugar;

namespace Bedrock.Infrastructure.Repositories
{
    /// <summary>
    /// 系统角色仓储实现。
    /// <para>
    /// 支持软删除（通过 DeletedAt 字段标记），所有查询默认过滤已删除记录。
    /// 业务规则：Key 与 Name 均保证全局唯一。
    /// </para>
    /// </summary>
    public class SysRoleRepository : ISysRoleRepository
    {
        private readonly ISqlSugarClient _db;

        /// <summary>
        /// 构造函数注入 SqlSugar 数据库上下文。
        /// </summary>
        /// <param name="db">数据库客户端实例。</param>
        public SysRoleRepository(ISqlSugarClient db)
        {
            _db = db;
        }

        /// <summary>
        /// 创建一条新的角色记录。
        /// </summary>
        /// <param name="entity">待创建的实体。主键由数据库生成。</param>
        /// <returns>返回插入后生成的主键 ID。</returns>
        public async Task<long> CreateAsync(SysRole entity)
        {
            var id = await _db.Insertable(entity).ExecuteReturnBigIdentityAsync();
            return id;
        }

        /// <summary>
        /// 批量创建角色记录。
        /// </summary>
        /// <param name="entities">待创建的实体集合。</param>
        /// <returns>返回成功插入的记录数量；若集合为空，则返回 0。</returns>
        public async Task<int> CreateBatchAsync(IEnumerable<SysRole> entities)
        {
            var entityArray = entities as SysRole[] ?? entities.ToArray();
            if (!entityArray.Any())
                return 0;

            var rowsAffected = await _db.Insertable(entityArray).ExecuteCommandAsync();
            return rowsAffected;
        }

        /// <summary>
        /// 更新一条角色记录。
        /// </summary>
        /// <param name="entity">包含更新数据的实体，必须包含有效 Id。</param>
        /// <returns>返回受影响的记录数（通常为 1，若记录不存在则为 0）。</returns>
        public async Task<int> UpdateAsync(SysRole entity)
        {
            var rowsAffected = await _db.Updateable(entity).ExecuteCommandAsync();
            return rowsAffected;
        }

        /// <summary>
        /// 批量更新角色记录。
        /// </summary>
        /// <param name="entities">待更新的实体集合，每个实体必须包含有效 Id。</param>
        /// <returns>返回所有更新操作受影响的总记录数；若集合为空，则返回 0。</returns>
        public async Task<int> UpdateBatchAsync(IEnumerable<SysRole> entities)
        {
            var entityArray = entities as SysRole[] ?? entities.ToArray();
            if (!entityArray.Any())
                return 0;

            var rowsAffected = await _db.Updateable(entityArray).ExecuteCommandAsync();
            return rowsAffected;
        }

        /// <summary>
        /// 软删除指定的角色记录（标记 DeletedAt 和 DeletedById）。
        /// </summary>
        /// <param name="entity">要删除的实体，用于获取 Id 和审计信息。</param>
        /// <returns>返回受影响的记录数（通常为 1，若记录不存在则为 0）。</returns>
        public async Task<int> DeleteAsync(SysRole entity)
        {
            var rowsAffected = await _db.Updateable(entity).ExecuteCommandAsync();
            return rowsAffected;
        }

        /// <summary>
        /// 批量软删除角色记录，并记录操作人。
        /// </summary>
        /// <param name="ids">要删除的记录 ID 列表。</param>
        /// <param name="operatorId">执行删除操作的用户 ID，用于审计。</param>
        /// <returns>返回成功标记为删除的记录数量；若 ID 列表为空，则返回 0。</returns>
        public async Task<int> DeleteBatchAsync(IEnumerable<long> ids, long operatorId)
        {
            var idList = ids as long[] ?? ids.ToArray();
            if (!idList.Any())
                return 0;

            var rowsAffected = await _db.Updateable<SysRole>()
                .SetColumns(e => new SysRole
                {
                    DeletedAt = DateTime.UtcNow,
                    DeletedById = operatorId
                })
                .Where(e => idList.Contains(e.Id))
                .ExecuteCommandAsync();

            return rowsAffected;
        }

        /// <summary>
        /// 根据主键获取单条未删除的角色记录。
        /// </summary>
        /// <param name="id">记录的唯一标识符。</param>
        /// <returns>匹配的实体；若未找到或已删除，则返回 null。</returns>
        public async Task<SysRole> GetAsync(long id)
        {
            var entity = await _db.Queryable<SysRole>()
                .Where(e => e.Id == id && e.DeletedAt == null)
                .FirstAsync();

            return entity;
        }

        /// <summary>
        /// 根据 ID 列表批量获取未删除的角色实体。
        /// </summary>
        /// <param name="ids">角色 ID 列表。</param>
        /// <returns>返回匹配的实体列表（可能为空）。</returns>
        public async Task<List<SysRole>> GetByIdsAsync(IEnumerable<long> ids)
        {
            if (ids == null)
                return new List<SysRole>();

            var idList = ids as long[] ?? ids.ToArray();
            if (!idList.Any())
                return new List<SysRole>();

            return await _db.Queryable<SysRole>()
                .Where(x => idList.Contains(x.Id))
                .Where(x => x.DeletedAt == null)
                .ToListAsync();
        }

        /// <summary>
        /// 查询未删除的角色记录，支持按名称模糊搜索和状态精确筛选。
        /// </summary>
        /// <param name="name">角色名称，用于模糊搜索（可选）。不传则忽略该条件。</param>
        /// <param name="status">角色状态，用于精确匹配（可选）。不传则忽略该条件。</param>
        /// <returns>匹配条件的未删除记录集合（可能为空）。</returns>
        public async Task<List<SysRole>> GetAllAsync(string? name = null, string? status = null)
        {
            var query = _db.Queryable<SysRole>().Where(e => e.DeletedAt == null);

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
        /// 分页查询未删除的角色记录，支持名称、类型模糊搜索和状态精确筛选。
        /// </summary>
        /// <param name="pageNumber">页码，从 1 开始。</param>
        /// <param name="pageSize">每页记录数。</param>
        /// <param name="name">角色名称，用于模糊搜索（可选）。</param>
        /// <param name="key">角色权限字符，用于模糊搜索（可选）。</param>
        /// <param name="status">角色状态，用于精确匹配（可选）。</param>
        /// <param name="orderByField">排序字段（可选）。</param>
        /// <param name="orderByType">排序方式（可选）。</param>
        /// <returns>
        /// 元组包含分页结果：
        /// - Data：当前页的数据列表（可能为空）。
        /// - TotalCount：满足查询条件的总记录数。
        /// </returns>
        public async Task<(List<SysRole> Data, int TotalCount)> GetPagedAsync(
            int pageNumber,
            int pageSize,
            string? name = null,
            string? key = null,
            string? status = null,
            string? orderByField = null,
            string? orderByType = null)
        {
            // 排序字段映射：前端字段名 → 数据库字段名
            orderByField = (orderByField ?? "createdAt") switch
            {
                "id" => "id",
                "userName" => "user_name",
                "sort" => "sort",
                "createdAt" => "created_at",
                _ => "created_at"
            };
            orderByType = orderByType ?? "DESC";

            var query = _db.Queryable<SysRole>().Where(e => e.DeletedAt == null);

            if (!string.IsNullOrWhiteSpace(name))
            {
                query = query.Where(e => e.Name.Contains(name));
            }

            if (!string.IsNullOrWhiteSpace(key))
            {
                query = query.Where(e => e.Key.Contains(key));
            }

            if (!string.IsNullOrWhiteSpace(status))
            {
                query = query.Where(e => e.Status == status);
            }

            var totalCount = await query.CountAsync();
            var pagedData = await query
                .OrderBy($"{orderByField} {orderByType}")
                .ToPageListAsync(pageNumber, pageSize);

            return (pagedData, totalCount);
        }

        /// <summary>
        /// 根据角色权限字符获取唯一未删除的记录。
        /// </summary>
        /// <param name="key">角色的唯一权限字符（如 admin），用于精确匹配。</param>
        /// <returns>匹配的实体；若未找到或已删除，则返回 null。由于 Key 唯一，最多返回一条。</returns>
        public async Task<SysRole?> GetByKeyAsync(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
                return null;

            var entity = await _db.Queryable<SysRole>()
                .Where(e => e.Key == key && e.DeletedAt == null)
                .FirstAsync();

            return entity;
        }

        /// <summary>
        /// 根据角色名称获取唯一未删除的记录。
        /// </summary>
        /// <param name="name">角色的显示名称（如“管理员”），用于精确匹配。</param>
        /// <returns>匹配的实体；若未找到或已删除，则返回 null。由于 Name 唯一，最多返回一条。</returns>
        public async Task<SysRole?> GetByNameAsync(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return null;

            var entity = await _db.Queryable<SysRole>()
                .Where(e => e.Name == name && e.DeletedAt == null)
                .FirstAsync();

            return entity;
        }

        /// <summary>
        /// 根据用户 ID 查询未删除的角色记录。
        /// </summary>
        /// <param name="userId">用户 ID。</param>
        /// <returns>匹配条件的未删除记录集合（可能为空）。</returns>
        public async Task<IEnumerable<SysRole>> GetAllByUserIdAsync(long userId)
        {
            var query = _db.Queryable<SysRole, SysUserRole>((r, ur) => new object[]
             {
                JoinType.Left, r.Id == ur.RoleId,
             })
             .Where((r, ur) => r.DeletedAt == null);

            query = query.Where((r, ur) => ur.UserId == userId);

            var entities = await query
                .OrderBy((r, ur) => r.Sort, OrderByType.Asc)
                .ToListAsync();

            return entities;
        }

    }
}