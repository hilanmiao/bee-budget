using Bedrock.Application.Interfaces;
using Bedrock.Core.Entities;
using SqlSugar;

namespace Bedrock.Infrastructure.Repositories
{
    /// <summary>
    /// 系统字典项仓储实现。
    /// <para>
    /// 支持软删除（通过 <c>DeletedAt</c> 字段标记），所有查询默认过滤已删除记录。
    /// 业务规则：<c>Label</c> 与 <c>Value</c> 均保证全局唯一。
    /// </para>
    /// </summary>
    public class SysDictItemRepository : ISysDictItemRepository
    {
        private readonly ISqlSugarClient _db;

        /// <summary>
        /// 构造函数注入 SqlSugar 数据库上下文。
        /// </summary>
        /// <param name="db">数据库客户端实例。</param>
        public SysDictItemRepository(ISqlSugarClient db)
        {
            _db = db;
        }

        /// <summary>
        /// 创建一条新的字典项记录。
        /// </summary>
        /// <param name="entity">待创建的实体。主键由数据库生成。</param>
        /// <returns>返回插入后生成的主键 ID。</returns>
        public async Task<long> CreateAsync(SysDictItem entity)
        {
            var id = await _db.Insertable(entity).ExecuteReturnBigIdentityAsync();
            return id;
        }

        /// <summary>
        /// 批量创建字典项记录。
        /// </summary>
        /// <param name="entities">待创建的实体集合。</param>
        /// <returns>返回成功插入的记录数量。</returns>
        public async Task<int> CreateBatchAsync(IEnumerable<SysDictItem> entities)
        {
            var entityArray = entities as SysDictItem[] ?? entities.ToArray();
            if (!entityArray.Any())
                return 0;

            var rowsAffected = await _db.Insertable(entityArray).ExecuteCommandAsync();
            return rowsAffected;
        }

        /// <summary>
        /// 更新一条字典项记录。
        /// </summary>
        /// <param name="entity">包含更新数据的实体，必须包含有效 <c>Id</c>。</param>
        /// <returns>返回受影响的记录数（通常为 1，若记录不存在则为 0）。</returns>
        public async Task<int> UpdateAsync(SysDictItem entity)
        {
            var rowsAffected = await _db.Updateable(entity).ExecuteCommandAsync();
            return rowsAffected;
        }

        /// <summary>
        /// 批量更新字典项记录。
        /// </summary>
        /// <param name="entities">待更新的实体集合，每个实体必须包含有效 <c>Id</c>。</param>
        /// <returns>返回所有更新操作受影响的总记录数。</returns>
        public async Task<int> UpdateBatchAsync(IEnumerable<SysDictItem> entities)
        {
            var entityArray = entities as SysDictItem[] ?? entities.ToArray();
            if (!entityArray.Any())
                return 0;

            var rowsAffected = await _db.Updateable(entityArray).ExecuteCommandAsync();
            return rowsAffected;
        }

        /// <summary>
        /// 删除指定的字典项记录（软删除）。
        /// </summary>
        /// <param name="entity">要删除的实体，通常用于获取 <c>Id</c> 和审计信息。</param>
        /// <returns>返回受影响的记录数（1 或 0）。</returns>
        public async Task<int> DeleteAsync(SysDictItem entity)
        {
            var rowsAffected = await _db.Updateable(entity).ExecuteCommandAsync();
            return rowsAffected;
        }

        /// <summary>
        /// 批量删除字典项记录（软删除），并记录操作人。
        /// </summary>
        /// <param name="ids">要删除的记录 ID 列表。</param>
        /// <param name="operatorId">执行删除操作的用户 ID。</param>
        /// <returns>返回成功标记为删除的记录数量。</returns>
        public async Task<int> DeleteBatchAsync(IEnumerable<long> ids, long operatorId)
        {
            var idList = ids as long[] ?? ids.ToArray();
            if (!idList.Any())
                return 0;

            var rowsAffected = await _db.Updateable<SysDictItem>()
                .SetColumns(e => new SysDictItem
                {
                    DeletedAt = DateTime.UtcNow,
                    DeletedById = operatorId
                })
                .Where(e => idList.Contains(e.Id))
                .ExecuteCommandAsync();

            return rowsAffected;
        }

        /// <summary>
        /// 根据主键获取单条字典项记录。
        /// </summary>
        /// <param name="id">记录的唯一标识符。</param>
        /// <returns>匹配的实体；若未找到或已删除，则返回 <c>null</c>。</returns>
        public async Task<SysDictItem> GetAsync(long id)
        {
            var entity = await _db.Queryable<SysDictItem>()
                .Where(e => e.Id == id && e.DeletedAt == null)
                .FirstAsync();

            return entity;
        }

        /// <summary>
        /// 查询字典项记录，支持按名称模糊搜索和状态筛选。
        /// </summary>
        /// <param name="label">字典项标签，用于**模糊匹配**（可选）。</param>
        /// <param name="status">字典项状态，用于**精确匹配**（可选）。</param>
        /// <returns>匹配条件的所有未删除记录集合（可能为空）。</returns>
        public async Task<List<SysDictItem>> GetAllAsync(string? label = null, string? status = null)
        {
            var query = _db.Queryable<SysDictItem>().Where(e => e.DeletedAt == null);

            if (!string.IsNullOrWhiteSpace(label))
            {
                query = query.Where(e => e.Label.Contains(label));
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
        /// 分页查询字典项记录，支持名称、类型模糊搜索、状态精确筛选、字典分类编码精确筛选。
        /// </summary>
        /// <param name="pageNumber">页码，从 1 开始。</param>
        /// <param name="pageSize">每页记录数。</param>
        /// <param name="label">字典项标签，模糊搜索（可选）。</param>
        /// <param name="value">字典项实际值，模糊搜索（可选）。</param>
        /// <param name="status">字典项状态，精确匹配（可选）。</param>
        /// <param name="categoryCode">字典分类编码，精确匹配（可选）。</param>
        /// <param name="orderByField">排序字段（可选）。</param>
        /// <param name="orderByType">排序方式（可选）。</param>
        /// <returns>
        /// 元组包含：
        /// - <c>Data</c>：当前页的数据列表；
        /// - <c>TotalCount</c>：满足条件的总记录数。
        /// </returns>
        public async Task<(List<SysDictItem> Data, int TotalCount)> GetPagedAsync(
            int pageNumber,
            int pageSize,
            string? label = null,
            string? value = null,
            string? status = null,
            string? categoryCode = null,
            string? orderByField = null,
            string? orderByType = null)
        {
            // 排序字段映射：前端字段名 → 数据库字段名
            orderByField = (orderByField ?? "sort") switch
            {
                "id" => "id",
                "sort" => "sort",
                "createdAt" => "created_at",
                _ => "created_at"
            };
            orderByType = orderByType ?? "ASC";

            var query = _db.Queryable<SysDictItem>().Where(e => e.DeletedAt == null);

            if (!string.IsNullOrWhiteSpace(label))
            {
                query = query.Where(e => e.Label.Contains(label));
            }

            if (!string.IsNullOrWhiteSpace(value))
            {
                query = query.Where(e => e.Value.Contains(value));
            }

            if (!string.IsNullOrWhiteSpace(status))
            {
                query = query.Where(e => e.Status == status);
            }

            if (!string.IsNullOrWhiteSpace(categoryCode))
            {
                query = query.Where(e => e.CategoryCode == categoryCode);
            }

            var totalCount = await query.CountAsync();
            var pagedData = await query
                .OrderBy($"{orderByField} {orderByType}")
                .ToPageListAsync(pageNumber, pageSize);

            return (pagedData, totalCount);
        }

        /// <summary>
        /// 根据 ID 列表批量获取未删除的字典项实体。
        /// </summary>
        /// <param name="ids">字典项 ID 列表（可为 List、Array 等）。</param>
        /// <returns>返回匹配的实体列表（已执行查询，非延迟）。</returns>
        public async Task<List<SysDictItem>> GetByIdsAsync(IEnumerable<long> ids)
        {
            if (ids == null)
                return new List<SysDictItem>();

            var idList = ids as long[] ?? ids.ToArray();
            if (!idList.Any())
                return new List<SysDictItem>();

            return await _db.Queryable<SysDictItem>()
                .Where(x => idList.Contains(x.Id))
                .Where(x => x.DeletedAt == null)
                .ToListAsync();
        }

        /// <summary>
        /// 根据字典项实际值获取唯一记录。
        /// </summary>
        /// <param name="value">字典项实际值（如 0），精确匹配。</param>
        /// <returns>匹配的实体；若未找到或已删除，则返回 <c>null</c>。由于 <c>Value</c> 唯一，最多返回一条。</returns>
        public async Task<SysDictItem?> GetByValueAsync(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return null;

            var entity = await _db.Queryable<SysDictItem>()
                .Where(e => e.Value == value && e.DeletedAt == null)
                .FirstAsync();

            return entity;
        }

        /// <summary>
        /// 根据字典项标签获取唯一记录。
        /// </summary>
        /// <param name="label">字典项的显示标签（如“男”），精确匹配。</param>
        /// <returns>匹配的实体；若未找到或已删除，则返回 <c>null</c>。由于 <c>Label</c> 唯一，最多返回一条。</returns>
        public async Task<SysDictItem?> GetByLabelAsync(string label)
        {
            if (string.IsNullOrWhiteSpace(label))
                return null;

            var entity = await _db.Queryable<SysDictItem>()
                .Where(e => e.Label == label && e.DeletedAt == null)
                .FirstAsync();

            return entity;
        }

        /// <summary>
        /// 根据字典分类编码查询未删除的字典项记录。
        /// </summary>
        /// <param name="categoryCode">字典分类编码。</param>
        /// <returns>匹配条件的未删除记录集合（可能为空）。</returns>
        public async Task<List<SysDictItem>> GetAllByCategoryCodeAsync(string categoryCode)
        {
            if (string.IsNullOrWhiteSpace(categoryCode))
                return new List<SysDictItem>();

            var query = _db.Queryable<SysDictItem>()
                .Where(e => e.DeletedAt == null && e.CategoryCode == categoryCode);

            var entities = await query
                .OrderBy(e => e.Sort, OrderByType.Asc)
                .ToListAsync();

            return entities;
        }

        /// <summary>
        /// 根据字典分类，批量软删除字典项记录，并记录操作人。
        /// </summary>
        /// <param name="categoryCode">要删除的记录 字典分类编码。</param>
        /// <param name="operatorId">执行删除操作的用户 ID，用于审计。</param>
        /// <returns>返回成功标记为删除的记录数量；若 字典分类编码 列表为空，则返回 0。</returns>    
        public async Task<int> DeleteByCategoryCodeAsync(string categoryCode, long operatorId)
        {
            var rowsAffected = await _db.Updateable<SysDictItem>()
                .SetColumns(e => new SysDictItem
                {
                    DeletedAt = DateTime.UtcNow,
                    DeletedById = operatorId
                })
                .Where(e => e.CategoryCode == categoryCode)
                .ExecuteCommandAsync();

            return rowsAffected;
        }

        /// <summary>
        /// 根据字典分类编码列表，批量软删除字典项记录，并记录操作人。
        /// </summary>
        /// <param name="categoryCodes">要删除的记录 字典分类编码 列表。</param>
        /// <param name="operatorId">执行删除操作的用户 ID，用于审计。</param>
        /// <returns>返回成功标记为删除的记录数量；若 字典分类编码 列表为空，则返回 0。</returns>        
        public async Task<int> DeleteByCategoryCodesAsync(IEnumerable<string> categoryCodes, long operatorId)
        {
            var categoryCodeList = categoryCodes as string[] ?? categoryCodes.ToArray();
            if (!categoryCodeList.Any())
                return 0;

            var rowsAffected = await _db.Updateable<SysDictItem>()
                .SetColumns(e => new SysDictItem
                {
                    DeletedAt = DateTime.UtcNow,
                    DeletedById = operatorId
                })
                .Where(e => categoryCodeList.Contains(e.CategoryCode) && e.DeletedAt == null)
                .ExecuteCommandAsync();

            return rowsAffected;
        }
    }
}