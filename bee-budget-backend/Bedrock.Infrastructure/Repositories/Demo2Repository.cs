using Bedrock.Application.DataTransferObjects;
using Bedrock.Application.Interfaces;
using Bedrock.Core.Entities;
using SqlSugar;

namespace Bedrock.Infrastructure.Repositories
{
    /// <summary>
    /// 样例2仓储实现。
    /// <para>
    /// 支持软删除（通过 DeletedAt 字段标记），所有查询默认过滤已删除记录。
    /// 业务规则：Demo1Id 和 Code 保证全局唯一。
    /// </para>
    /// </summary>
    public class Demo2Repository : IDemo2Repository
    {
        private readonly ISqlSugarClient _db;

        /// <summary>
        /// 构造函数注入 SqlSugar 数据库上下文。
        /// </summary>
        /// <param name="db">数据库客户端实例。</param>
        public Demo2Repository(ISqlSugarClient db)
        {
            _db = db;
        }

        /// <summary>
        /// 创建一条新的样例2记录。
        /// </summary>
        /// <param name="entity">待创建的实体。主键由数据库生成。</param>
        /// <returns>返回插入后生成的主键 ID。</returns>
        public async Task<long> CreateAsync(Demo2 entity)
        {
            var id = await _db.Insertable(entity).ExecuteReturnBigIdentityAsync();
            return id;
        }

        /// <summary>
        /// 批量创建样例2记录。
        /// </summary>
        /// <param name="entities">待创建的实体集合。</param>
        /// <returns>返回成功插入的记录数量。</returns>
        public async Task<int> CreateBatchAsync(IEnumerable<Demo2> entities)
        {
            var entityArray = entities as Demo2[] ?? entities.ToArray();
            if (!entityArray.Any())
                return 0;

            var rowsAffected = await _db.Insertable(entityArray).ExecuteCommandAsync();
            return rowsAffected;
        }

        /// <summary>
        /// 更新一条样例2记录。
        /// </summary>
        /// <param name="entity">包含更新数据的实体，必须包含有效 Id。</param>
        /// <returns>返回受影响的记录数（通常为 1，若记录不存在则为 0）。</returns>
        public async Task<int> UpdateAsync(Demo2 entity)
        {
            var rowsAffected = await _db.Updateable(entity).ExecuteCommandAsync();
            return rowsAffected;
        }

        /// <summary>
        /// 批量更新样例2记录。
        /// </summary>
        /// <param name="entities">待更新的实体集合，每个实体必须包含有效 <c>Id</c>。</param>
        /// <returns>返回所有更新操作受影响的总记录数。</returns>
        public async Task<int> UpdateBatchAsync(IEnumerable<Demo2> entities)
        {
            var entityArray = entities as Demo2[] ?? entities.ToArray();
            if (!entityArray.Any())
                return 0;

            var rowsAffected = await _db.Updateable(entityArray).ExecuteCommandAsync();
            return rowsAffected;
        }

        /// <summary>
        /// 软删除指定的样例2记录（标记 DeletedAt 和 DeletedById）。
        /// </summary>
        /// <param name="entity">要删除的实体，用于获取 Id 和审计信息。</param>
        /// <returns>返回受影响的记录数（1 或 0）。</returns>
        public async Task<int> DeleteAsync(Demo2 entity)
        {
            var rowsAffected = await _db.Updateable(entity).ExecuteCommandAsync();
            return rowsAffected;
        }

        /// <summary>
        /// 批量软删除样例2记录，并记录操作人。
        /// </summary>
        /// <param name="ids">要删除的记录 ID 列表。</param>
        /// <param name="operatorId">执行删除操作的操作人 ID，用于审计。</param>
        /// <returns>返回成功标记为删除的记录数量；若 ID 列表为空，则返回 0。</returns>
        public async Task<int> DeleteBatchAsync(IEnumerable<long> ids, long operatorId)
        {
            var idList = ids as long[] ?? ids.ToArray();
            if (!idList.Any())
                return 0;

            var rowsAffected = await _db.Updateable<Demo2>()
                .SetColumns(it => new Demo2
                {
                    DeletedAt = DateTime.UtcNow,
                    DeletedById = operatorId
                })
                .Where(it => idList.Contains(it.Id))
                .ExecuteCommandAsync();

            return rowsAffected;
        }

        /// <summary>
        /// 根据主键获取单条未删除的样例2记录。
        /// </summary>
        /// <param name="id">记录的唯一标识符。</param>
        /// <returns>匹配的实体；若未找到或已删除，则返回 null。</returns>
        public async Task<Demo2?> GetAsync(long id)
        {
            var entity = await _db.Queryable<Demo2>()
                .Where(it => it.DeletedAt == null && it.Id == id)
                .FirstAsync();

            return entity;
        }

        /// <summary>
        /// 查询未删除的样例2记录，支持按名称模糊搜索、状态和Demo1Id精确搜索。
        /// </summary>
        /// <param name="name">样例2名称，用于模糊搜索（可选）。</param>
        /// <param name="status">样例2状态，用于模糊搜索（可选）。</param>
        /// <param name="demo1Id">样例2Demo1Id，用于精确搜索（可选）。</param>
        /// <param name="startDate">开始日期，用于日期范围搜索（可选）。</param>
        /// <param name="endDate">结束日期，用于日期范围搜索（可选）。</param>
        /// <returns>匹配条件的未删除记录集合（可能为空）。</returns>
        public async Task<List<Demo2>> GetAllAsync(
            string? name = null,
            string? status = null,
            long? demo1Id = null,
            DateTime? startDate = null,
            DateTime? endDate = null)
        {
            var query = _db.Queryable<Demo2>().Where(it => it.DeletedAt == null);

            if (!string.IsNullOrWhiteSpace(name))
            {
                query = query.Where(it => it.Name.Contains(name));
            }

            if (!string.IsNullOrWhiteSpace(status))
            {
                query = query.Where(it => it.Status == status);
            }

            if (demo1Id != null)
            {
                query = query.Where(it => it.Demo1Id == demo1Id);
            }

            if (startDate != null)
            {
                query = query.Where(it => it.CreatedAt >= startDate.Value);
            }

            if (endDate != null)
            {
                query = query.Where(it => it.CreatedAt < endDate.Value);
            }

            var entities = await query
                //.OrderBy(it => it.CreatedAt, OrderByType.Desc)
                .OrderBy(it => it.Sort, OrderByType.Asc)
                .ToListAsync();

            return entities;
        }

        /// <summary>
        /// 分页查询未删除的样例2记录，支持名称模糊搜索、状态和Demo1Id精确搜索。
        /// </summary>
        /// <param name="pageNumber">页码，从 1 开始。</param>
        /// <param name="pageSize">每页记录数。</param>
        /// <param name="name">样例2名称，用于模糊搜索（可选）。</param>
        /// <param name="status">样例2状态，用于模糊搜索（可选）。</param>
        /// <param name="demo1Id">样例2Demo1Id，用于精确搜索（可选）。</param>
        /// <param name="startDate">开始日期，用于日期范围搜索（可选）。</param>
        /// <param name="endDate">结束日期，用于日期范围搜索（可选）。</param>
        /// <param name="orderByField">排序字段（可选）。</param>
        /// <param name="orderByType">排序方式（可选）。</param>
        /// <returns>
        /// 元组包含分页结果：
        /// - Data：当前页的数据列表（可能为空）。
        /// - TotalCount：满足查询条件的总记录数。
        /// </returns>
        public async Task<(List<Demo2> Data, int TotalCount)> GetPagedAsync(
            int pageNumber,
            int pageSize,
            string? name = null,
            string? status = null,
            long? demo1Id = null,
            DateTime? startDate = null,
            DateTime? endDate = null,
            string? orderByField = null,
            string? orderByType = null)
        {
            // 排序字段映射：前端字段名 → 数据库字段名
            orderByField = (orderByField ?? "createdAt") switch
            {
                "id" => "id",
                "createdAt" => "created_at",
                "updatedAt" => "updated_at",
                _ => "created_at"
            };
            orderByType = orderByType ?? "DESC";

            var query = _db.Queryable<Demo2>().Where(it => it.DeletedAt == null);

            if (!string.IsNullOrWhiteSpace(name))
            {
                query = query.Where(it => it.Name.Contains(name));
            }

            if (!string.IsNullOrWhiteSpace(status))
            {
                query = query.Where(it => it.Status == status);
            }

            if (demo1Id != null)
            {
                query = query.Where(it => it.Demo1Id == demo1Id);
            }

            if (startDate != null)
            {
                query = query.Where(it => it.CreatedAt >= startDate.Value);
            }

            if (endDate != null)
            {
                query = query.Where(it => it.CreatedAt < endDate.Value);
            }

            var totalCount = await query.CountAsync();
            var pagedData = await query
                .OrderBy($"{orderByField} {orderByType}")
                .ToPageListAsync(pageNumber, pageSize);

            return (pagedData, totalCount);
        }

        /// <summary>
        /// 根据 ID 列表批量获取未删除的样例2实体。
        /// </summary>
        /// <param name="ids">样例2 ID 列表（可为 List、Array 等）。</param>
        /// <returns>返回匹配的实体列表（已执行查询，非延迟）。</returns>
        public async Task<List<Demo2>> GetByIdsAsync(IEnumerable<long> ids)
        {
            if (ids == null)
                return new();

            var idList = ids as long[] ?? ids.ToArray();
            if (!idList.Any())
                return new();

            return await _db.Queryable<Demo2>()
                .Where(it => it.DeletedAt == null)
                .Where(it => idList.Contains(it.Id))
                .ToListAsync();
        }

        /// <summary>
        /// 根据样例2名称获取唯一未删除的记录。
        /// </summary>
        /// <param name="name">样例2的名称（如“测试”），用于精确匹配。</param>
        /// <returns>匹配的实体；若未找到或已删除，则返回 null。由于 Name 唯一，最多返回一条。</returns>
        public async Task<Demo2?> GetByNameAsync(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return null;

            var entity = await _db.Queryable<Demo2>()
                .Where(it => it.DeletedAt == null && it.Name == name)
                .FirstAsync();

            return entity;
        }

        /// <summary>
        /// 根据样例2编码获取一条记录。
        /// </summary>
        /// <param name="code">样例2编码（如 0），精确匹配。</param>
        /// <returns>匹配的实体；若未找到或已删除，则返回 <c>null</c>。</returns>
        public async Task<Demo2?> GetByCodeAsync(int code)
        {
            if (code <= 0)
                return null;

            var entity = await _db.Queryable<Demo2>()
                .Where(it => it.DeletedAt == null && it.Code == code)
                .FirstAsync();

            return entity;
        }

        /// <summary>
        /// 根据样例2Demo1Id获取一条记录。
        /// </summary>
        /// <param name="demo1Id">样例2Demo1Id（如 0），精确匹配。</param>
        /// <returns>匹配的实体；若未找到或已删除，则返回 <c>null</c>。</returns>
        public async Task<Demo2?> GetByDemo1IdAsync(long demo1Id)
        {
            if (demo1Id <= 0)
                return null;

            var entity = await _db.Queryable<Demo2>()
                .Where(it => it.DeletedAt == null && it.Demo1Id == demo1Id)
                .FirstAsync();

            return entity;
        }

        /// <summary>
        /// 根据样例2Demo1Id查询未删除的样例2记录。
        /// </summary>
        /// <param name="demo1Id">样例2Demo1Id。</param>
        /// <returns>匹配条件的未删除记录集合（可能为空）。</returns>
        public async Task<List<Demo2>> GetAllByDemo1IdAsync(long demo1Id)
        {
            if (demo1Id <= 0)
                return new();

            var query = _db.Queryable<Demo2>()
                .Where(it => it.DeletedAt == null && it.Demo1Id == demo1Id);

            var entities = await query
                //.OrderBy(it => it.CreatedAt, OrderByType.Desc)
                .OrderBy(it => it.Sort, OrderByType.Asc)
                .ToListAsync();

            return entities;
        }

        /// <summary>
        /// 根据Demo1Id，批量软删除样例2记录，并记录操作人。
        /// </summary>
        /// <param name="demo1Id">要删除的记录 Demo1Id。</param>
        /// <param name="operatorId">执行删除操作的操作人 ID，用于审计。</param>
        /// <returns>返回成功标记为删除的记录数量；</returns>    
        public async Task<int> DeleteByDemo1IdAsync(long demo1Id, long operatorId)
        {
            var rowsAffected = await _db.Updateable<Demo2>()
                .SetColumns(it => new Demo2
                {
                    DeletedAt = DateTime.UtcNow,
                    DeletedById = operatorId
                })
                .Where(it => it.Demo1Id == demo1Id)
                .ExecuteCommandAsync();

            return rowsAffected;
        }

        /// <summary>
        /// 根据Demo1Id列表，批量软删除样例2记录，并记录操作人。
        /// </summary>
        /// <param name="demo1Ids">要删除的记录 Demo1Id 列表。</param>
        /// <param name="operatorId">执行删除操作的操作人 ID，用于审计。</param>
        /// <returns>返回成功标记为删除的记录数量；若 Demo1Id 列表为空，则返回 0。</returns>        
        public async Task<int> DeleteByDemo1IdsAsync(IEnumerable<long> demo1Ids, long operatorId)
        {
            var demo1IdList = demo1Ids as long[] ?? demo1Ids.ToArray();
            if (!demo1IdList.Any())
                return 0;

            var rowsAffected = await _db.Updateable<Demo2>()
                .SetColumns(it => new Demo2
                {
                    DeletedAt = DateTime.UtcNow,
                    DeletedById = operatorId
                })
                .Where(it => demo1IdList.Contains(it.Demo1Id) && it.DeletedAt == null)
                .ExecuteCommandAsync();

            return rowsAffected;
        }

        #region 关联表操作

        /// <summary>
        /// 根据主键获取单条未删除的样例2记录。
        /// </summary>
        /// <param name="id">记录的唯一标识符。</param>
        /// <returns>匹配的 Dto；若未找到或已删除，则返回 null。</returns>
        public async Task<Demo2Summary?> GetSummaryAsync(long id)
        {
            var entity = await _db.Queryable<Demo2, Demo1>((it, a) => new object[]
               {
                    JoinType.Left, it.Demo1Id == a.Id, // 连接其他表
               })
               .Where(it => it.DeletedAt == null) // 查询未删除的记录
               .Where(it => it.Id == id)
               .Select((it, a) => new Demo2Summary
               {
                   Id = it.Id,
                   Demo1Id = it.Demo1Id,
                   Demo1Name = a.Name,
                   Name = it.Name,
                   AliasName = it.AliasName,
                   Code = it.Code,
                   IsVisible = it.IsVisible,
                   Sort = it.Sort,
                   Status = it.Status,
                   Remark = it.Remark,
                   CreatedById = it.CreatedById,
                   CreatedAt = it.CreatedAt,
                   UpdatedById = it.UpdatedById,
                   UpdatedAt = it.UpdatedAt,
                   DeletedById = it.DeletedById,
                   DeletedAt = it.DeletedAt
               }) // 映射到摘要
               .FirstAsync();

            return entity; // 返回查询结果，可能为 null
        }

        /// <summary>
        /// 查询未删除的样例2记录，支持按Demo1Id精确搜索、状态和Demo1Id精确搜索。
        /// </summary>
        /// <param name="name">样例2名称，用于模糊搜索（可选）。</param>
        /// <param name="status">样例2状态，用于模糊搜索（可选）。</param>
        /// <param name="demo1Id">样例2Demo1Id，用于精确搜索（可选）。</param>
        /// <param name="startDate">开始日期，用于日期范围搜索（可选）。</param>
        /// <param name="endDate">结束日期，用于日期范围搜索（可选）。</param>
        /// <returns>匹配条件的未删除 Dto 集合（可能为空）。</returns>
        public async Task<List<Demo2Summary>> GetAllSummaryAsync(
            string? name = null,
            string? status = null,
            long? demo1Id = null,
            DateTime? startDate = null,
            DateTime? endDate = null)
        {
            var query = _db.Queryable<Demo2, Demo1>((it, a) => new object[]
                {
                    JoinType.Left, it.Demo1Id == a.Id, // 连接其他表
                })
                .Where(it => it.DeletedAt == null); // 查询未删除的记录

            if (!string.IsNullOrWhiteSpace(name))
            {
                query = query.Where(it => it.Name.Contains(name));
            }

            if (!string.IsNullOrWhiteSpace(status))
            {
                query = query.Where(it => it.Status == status);
            }

            if (demo1Id != null)
            {
                query = query.Where(it => it.Demo1Id == demo1Id);
            }

            if (startDate != null)
            {
                query = query.Where(it => it.CreatedAt >= startDate.Value);
            }

            if (endDate != null)
            {
                query = query.Where(it => it.CreatedAt < endDate.Value);
            }

            var entities = await query
                //.OrderBy(it => it.CreatedAt, OrderByType.Desc) // 按创建时间倒序排序
                .OrderBy(it => it.Sort, OrderByType.Asc) // 按显示顺寻正序排序
                .Select((it, a) => new Demo2Summary
                {
                    Id = it.Id,
                    Demo1Id = it.Demo1Id,
                    Demo1Name = a.Name,
                    Name = it.Name,
                    AliasName = it.AliasName,
                    Code = it.Code,
                    IsVisible = it.IsVisible,
                    Sort = it.Sort,
                    Status = it.Status,
                    Remark = it.Remark,
                    CreatedById = it.CreatedById,
                    CreatedAt = it.CreatedAt,
                    UpdatedById = it.UpdatedById,
                    UpdatedAt = it.UpdatedAt,
                    DeletedById = it.DeletedById,
                    DeletedAt = it.DeletedAt
                }) // 映射到摘要
                .ToListAsync();

            return entities; // 返回查询结果，可能为 null
        }

        /// <summary>
        /// 分页查询未删除的样例2记录，支持名称模糊搜索、状态和Demo1Id精确搜索。
        /// </summary>
        /// <param name="pageNumber">页码，从 1 开始。</param>
        /// <param name="pageSize">每页记录数。</param>
        /// <param name="name">样例2名称，用于模糊搜索（可选）。</param>
        /// <param name="status">样例2状态，用于模糊搜索（可选）。</param>
        /// <param name="demo1Id">样例2Demo1Id，用于精确搜索（可选）。</param>
        /// <param name="startDate">开始日期，用于日期范围搜索（可选）。</param>
        /// <param name="endDate">结束日期，用于日期范围搜索（可选）。</param>
        /// <param name="orderByField">排序字段（可选）。</param>
        /// <param name="orderByType">排序方式（可选）。</param>
        /// <returns>
        /// 元组包含分页结果：
        /// - Data：当前页的数据列表（可能为空）。
        /// - TotalCount：满足查询条件的总记录数。
        /// </returns>
        public async Task<(List<Demo2Summary> Data, int TotalCount)> GetPagedSummaryAsync(
            int pageNumber,
            int pageSize,
            string? name = null,
            string? status = null,
            long? demo1Id = null,
            DateTime? startDate = null,
            DateTime? endDate = null,
            string? orderByField = null,
            string? orderByType = null)
        {
            // 排序字段映射：前端字段名 → 数据库字段名
            //orderByField = (orderByField ?? "createdAt") switch
            //{
            //    "id" => "id",
            //    "createdAt" => "created_at",
            //    "updatedAt" => "updated_at",
            //    "code" => "code",
            //    _ => "created_at"
            //};
            //orderByType = orderByType ?? "DESC";
            orderByField = (orderByField ?? "sort") switch
            {
                "id" => "id",
                "sort" => "sort",
                "createdAt" => "created_at",
                "updatedAt" => "updated_at",
                "code" => "code",
                _ => "created_at"
            };
            orderByType = orderByType ?? "ASC";

            var query = _db.Queryable<Demo2, Demo1>((it, a) => new object[]
            {
                JoinType.Left, it.Demo1Id == a.Id, // 连接其他表
            })
            .Where(it => it.DeletedAt == null); // 查询未删除的记录

            if (!string.IsNullOrWhiteSpace(name))
            {
                query = query.Where(it => it.Name.Contains(name));
            }

            if (!string.IsNullOrWhiteSpace(status))
            {
                query = query.Where(it => it.Status == status);
            }

            if (demo1Id != null)
            {
                query = query.Where(it => it.Demo1Id == demo1Id);
            }

            if (startDate != null)
            {
                query = query.Where(it => it.CreatedAt >= startDate.Value);
            }

            if (endDate != null)
            {
                query = query.Where(it => it.CreatedAt < endDate.Value);
            }

            // 获取总记录数
            var totalCount = await query.CountAsync();

            // 分页查询，并按创建时间倒序排序
            var pagedData = await query
                .OrderBy($"it.{orderByField} {orderByType}")
                .Select((it, a) => new Demo2Summary
                {
                    Id = it.Id,
                    Demo1Id = it.Demo1Id,
                    Demo1Name = a.Name,
                    Name = it.Name,
                    AliasName = it.AliasName,
                    Code = it.Code,
                    IsVisible = it.IsVisible,
                    Sort = it.Sort,
                    Status = it.Status,
                    Remark = it.Remark,
                    CreatedById = it.CreatedById,
                    CreatedAt = it.CreatedAt,
                    UpdatedById = it.UpdatedById,
                    UpdatedAt = it.UpdatedAt,
                    DeletedById = it.DeletedById,
                    DeletedAt = it.DeletedAt
                }) // 映射到摘要
                .ToPageListAsync(pageNumber, pageSize);

            return (pagedData, totalCount); // 返回分页结果
        }

        #endregion
    }
}