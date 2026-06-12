using Bedrock.Application.DataTransferObjects;
using Bedrock.Application.Interfaces;
using Bedrock.Core.Entities;
using SqlSugar;

namespace Bedrock.Infrastructure.Repositories
{
    /// <summary>
    /// App版本仓储实现。
    /// <para>
    /// 支持软删除（通过 DeletedAt 字段标记），所有查询默认过滤已删除记录。
    /// 业务规则：AppId 和 VersionCode 保证全局唯一。
    /// </para>
    /// </summary>
    public class AppVersionRepository : IAppVersionRepository
    {
        private readonly ISqlSugarClient _db;

        /// <summary>
        /// 构造函数注入 SqlSugar 数据库上下文。
        /// </summary>
        /// <param name="db">数据库客户端实例。</param>
        public AppVersionRepository(ISqlSugarClient db)
        {
            _db = db;
        }

        /// <summary>
        /// 创建一条新的App版本记录。
        /// </summary>
        /// <param name="entity">待创建的实体。主键由数据库生成。</param>
        /// <returns>返回插入后生成的主键 ID。</returns>
        public async Task<long> CreateAsync(AppVersion entity)
        {
            var id = await _db.Insertable(entity).ExecuteReturnBigIdentityAsync();
            return id;
        }

        /// <summary>
        /// 批量创建App版本记录。
        /// </summary>
        /// <param name="entities">待创建的实体集合。</param>
        /// <returns>返回成功插入的记录数量。</returns>
        public async Task<int> CreateBatchAsync(IEnumerable<AppVersion> entities)
        {
            var entityArray = entities as AppVersion[] ?? entities.ToArray();
            if (!entityArray.Any())
                return 0;

            var rowsAffected = await _db.Insertable(entityArray).ExecuteCommandAsync();
            return rowsAffected;
        }

        /// <summary>
        /// 更新一条App版本记录。
        /// </summary>
        /// <param name="entity">包含更新数据的实体，必须包含有效 Id。</param>
        /// <returns>返回受影响的记录数（通常为 1，若记录不存在则为 0）。</returns>
        public async Task<int> UpdateAsync(AppVersion entity)
        {
            var rowsAffected = await _db.Updateable(entity).ExecuteCommandAsync();
            return rowsAffected;
        }

        /// <summary>
        /// 批量更新App版本记录。
        /// </summary>
        /// <param name="entities">待更新的实体集合，每个实体必须包含有效 <c>Id</c>。</param>
        /// <returns>返回所有更新操作受影响的总记录数。</returns>
        public async Task<int> UpdateBatchAsync(IEnumerable<AppVersion> entities)
        {
            var entityArray = entities as AppVersion[] ?? entities.ToArray();
            if (!entityArray.Any())
                return 0;

            var rowsAffected = await _db.Updateable(entityArray).ExecuteCommandAsync();
            return rowsAffected;
        }

        /// <summary>
        /// 软删除指定的App版本记录（标记 DeletedAt 和 DeletedById）。
        /// </summary>
        /// <param name="entity">要删除的实体，用于获取 Id 和审计信息。</param>
        /// <returns>返回受影响的记录数（1 或 0）。</returns>
        public async Task<int> DeleteAsync(AppVersion entity)
        {
            var rowsAffected = await _db.Updateable(entity).ExecuteCommandAsync();
            return rowsAffected;
        }

        /// <summary>
        /// 批量软删除App版本记录，并记录操作人。
        /// </summary>
        /// <param name="ids">要删除的记录 ID 列表。</param>
        /// <param name="operatorId">执行删除操作的App版本 ID，用于审计。</param>
        /// <returns>返回成功标记为删除的记录数量；若 ID 列表为空，则返回 0。</returns>
        public async Task<int> DeleteBatchAsync(IEnumerable<long> ids, long operatorId)
        {
            var idList = ids as long[] ?? ids.ToArray();
            if (!idList.Any())
                return 0;

            var rowsAffected = await _db.Updateable<AppVersion>()
                .SetColumns(e => new AppVersion
                {
                    DeletedAt = DateTime.UtcNow,
                    DeletedById = operatorId
                })
                .Where(e => idList.Contains(e.Id))
                .ExecuteCommandAsync();

            return rowsAffected;
        }

        /// <summary>
        /// 根据主键获取单条未删除的App版本记录。
        /// </summary>
        /// <param name="id">记录的唯一标识符。</param>
        /// <returns>匹配的实体；若未找到或已删除，则返回 null。</returns>
        public async Task<AppVersion> GetAsync(long id)
        {
            var entity = await _db.Queryable<AppVersion>()
                .Where(e => e.Id == id && e.DeletedAt == null)
                .FirstAsync();

            return entity;
        }

        /// <summary>
        /// 查询未删除的App版本记录，支持按AppId精确搜索。
        /// </summary>
        /// <param name="appId">AppId，用于精确搜索（可选）。</param>
        /// <returns>匹配条件的未删除记录集合（可能为空）。</returns>
        public async Task<List<AppVersion>> GetAllAsync(string? appId = null)
        {
            var query = _db.Queryable<AppVersion>().Where(e => e.DeletedAt == null);

            if (!string.IsNullOrWhiteSpace(appId))
            {
                query = query.Where(e => e.AppId == appId);
            }

            var entities = await query
                .OrderBy(e => e.CreatedAt, OrderByType.Desc)
                .ToListAsync();

            return entities;
        }

        /// <summary>
        /// 分页查询未删除的App版本记录，支持名称模糊搜索。
        /// </summary>
        /// <param name="pageNumber">页码，从 1 开始。</param>
        /// <param name="pageSize">每页记录数。</param>
        /// <param name="appId">AppId，用于精确搜索（可选）。</param>
        /// <param name="orderByField">排序字段（可选）。</param>
        /// <param name="orderByType">排序方式（可选）。</param>
        /// <returns>
        /// 元组包含分页结果：
        /// - Data：当前页的数据列表（可能为空）。
        /// - TotalCount：满足查询条件的总记录数。
        /// </returns>
        public async Task<(List<AppVersion> Data, int TotalCount)> GetPagedAsync(
            int pageNumber,
            int pageSize,
            string? appId = null,
            string? orderByField = null,
            string? orderByType = null)
        {
            // 排序字段映射：前端字段名 → 数据库字段名
            orderByField = (orderByField ?? "createdAt") switch
            {
                "id" => "id",
                "createdAt" => "created_at",
                _ => "created_at"
            };
            orderByType = orderByType ?? "DESC";

            var query = _db.Queryable<AppVersion>().Where(e => e.DeletedAt == null);

            if (!string.IsNullOrWhiteSpace(appId))
            {
                query = query.Where(e => e.AppId == appId);
            }
            var totalCount = await query.CountAsync();
            var pagedData = await query
                .OrderBy($"{orderByField} {orderByType}")
                .ToPageListAsync(pageNumber, pageSize);

            return (pagedData, totalCount);
        }

        /// <summary>
        /// 根据 ID 列表批量获取未删除的App版本实体。
        /// </summary>
        /// <param name="ids">App版本 ID 列表（可为 List、Array 等）。</param>
        /// <returns>返回匹配的实体列表（已执行查询，非延迟）。</returns>
        public async Task<List<AppVersion>> GetByIdsAsync(IEnumerable<long> ids)
        {
            if (ids == null)
                return new List<AppVersion>();

            var idList = ids as long[] ?? ids.ToArray();
            if (!idList.Any())
                return new List<AppVersion>();

            return await _db.Queryable<AppVersion>()
                .Where(x => idList.Contains(x.Id))
                .Where(x => x.DeletedAt == null)
                .ToListAsync();
        }

        /// <summary>
        /// 根据App版本实际值获取唯一记录。
        /// </summary>
        /// <param name="appId">App版本实际值（如 0），精确匹配。</param>
        /// <returns>匹配的实体；若未找到或已删除，则返回 <c>null</c>。由于 <c>AppId</c> 唯一，最多返回一条。</returns>
        public async Task<AppVersion?> GetByAppIdAsync(string appId)
        {
            if (string.IsNullOrWhiteSpace(appId))
                return null;

            var entity = await _db.Queryable<AppVersion>()
                .Where(e => e.AppId == appId && e.DeletedAt == null)
                .FirstAsync();

            return entity;
        }

        /// <summary>
        /// 根据 AppId 获取已上线发行的最大版本信息。
        /// </summary>
        /// <param name="appId">记录的 AppId。</param>
        /// <returns>返回与 ID 匹配的实体对象；如果记录不存在，则返回 null。</returns>
        public async Task<AppVersion> GetMaxVersionAsync(string appId)
        {
            // 查询未删除的记录
            var entity = await _db.Queryable<AppVersion>()
               .Where(e => e.DeletedAt == null) // 查询未删除的记录
                                                //.Where(e => e.AppId == appId && e.IsStablePublish == true)
               .Where(e => e.AppId == appId)
               .OrderBy(e => e.VersionCode, OrderByType.Desc)
               .FirstAsync();

            return entity; // 返回查询结果，可能为 null
        }

        /// <summary>
        /// 根据App版本AppId查询未删除的App版本记录。
        /// </summary>
        /// <param name="appId">App版本AppId。</param>
        /// <returns>匹配条件的未删除记录集合（可能为空）。</returns>
        public async Task<List<AppVersion>> GetAllByAppIdAsync(string appId)
        {
            if (string.IsNullOrWhiteSpace(appId))
                return new List<AppVersion>();

            var query = _db.Queryable<AppVersion>()
                .Where(e => e.DeletedAt == null && e.AppId == appId);

            var entities = await query
                .OrderBy(e => e.CreatedAt, OrderByType.Desc)
                .ToListAsync();

            return entities;
        }

        /// <summary>
        /// 根据AppId，批量软删除App版本记录，并记录操作人。
        /// </summary>
        /// <param name="appId">要删除的记录 AppId。</param>
        /// <param name="operatorId">执行删除操作的用户 ID，用于审计。</param>
        /// <returns>返回成功标记为删除的记录数量；若 AppId 列表为空，则返回 0。</returns>    
        public async Task<int> DeleteByAppIdAsync(string appId, long operatorId)
        {
            var rowsAffected = await _db.Updateable<AppVersion>()
                .SetColumns(e => new AppVersion
                {
                    DeletedAt = DateTime.UtcNow,
                    DeletedById = operatorId
                })
                .Where(e => e.AppId == appId)
                .ExecuteCommandAsync();

            return rowsAffected;
        }

        /// <summary>
        /// 根据AppId列表，批量软删除App版本记录，并记录操作人。
        /// </summary>
        /// <param name="appIds">要删除的记录 AppId 列表。</param>
        /// <param name="operatorId">执行删除操作的用户 ID，用于审计。</param>
        /// <returns>返回成功标记为删除的记录数量；若 AppId 列表为空，则返回 0。</returns>        
        public async Task<int> DeleteByAppIdsAsync(IEnumerable<string> appIds, long operatorId)
        {
            var appIdList = appIds as string[] ?? appIds.ToArray();
            if (!appIdList.Any())
                return 0;

            var rowsAffected = await _db.Updateable<AppVersion>()
                .SetColumns(e => new AppVersion
                {
                    DeletedAt = DateTime.UtcNow,
                    DeletedById = operatorId
                })
                .Where(e => appIdList.Contains(e.AppId) && e.DeletedAt == null)
                .ExecuteCommandAsync();

            return rowsAffected;
        }

        #region 关联表操作

        /// <summary>
        /// 根据主键获取单条未删除的App版本记录。
        /// </summary>
        /// <param name="id">记录的唯一标识符。</param>
        /// <returns>匹配的实体；若未找到或已删除，则返回 null。</returns>
        public async Task<AppVersionSummary> GetSummaryAsync(long id)
        {
            var entity = await _db.Queryable<AppVersion, App>((e, a) => new object[]
               {
                    JoinType.Left, e.AppId == a.AppId, // 连接其他表
               })
               .Where(e => e.DeletedAt == null) // 查询未删除的记录
               .Where(e => e.Id == id)
               .Select((e, a) => new AppVersionSummary
               {
                   Id = e.Id,
                   AppId = e.AppId,
                   AppName = a.Name,
                   Title = e.Title,
                   Contents = e.Contents,
                   Platform = e.Platform,
                   VersionName = e.VersionName,
                   VersionCode = e.VersionCode,
                   Url = e.Url,
                   IsStablePublish = e.IsStablePublish,
                   IsSilently = e.IsSilently,
                   IsMandatory = e.IsMandatory,
                   CreatedById = e.CreatedById,
                   CreatedAt = e.CreatedAt,
                   UpdatedById = e.UpdatedById,
                   UpdatedAt = e.UpdatedAt,
                   DeletedById = e.DeletedById,
                   DeletedAt = e.DeletedAt
               }) // 映射到摘要
               .FirstAsync();

            return entity; // 返回查询结果，可能为 null
        }

        /// <summary>
        /// 查询未删除的App版本记录，支持按AppId精确搜索。
        /// </summary>
        /// <param name="appId">AppId，用于精确搜索（可选）。</param>
        /// <returns>匹配条件的未删除记录集合（可能为空）。</returns>
        public async Task<List<AppVersionSummary>> GetAllSummaryAsync(string? appId = null)
        {
            var query = _db.Queryable<AppVersion, App>((e, a) => new object[]
                {
                    JoinType.Left, e.AppId == a.AppId, // 连接其他表
                })
                .Where(e => e.DeletedAt == null); // 查询未删除的记录

            if (!string.IsNullOrWhiteSpace(appId))
            {
                query = query.Where(e => e.AppId == appId);
            }

            var entities = await query
                .OrderBy(e => e.CreatedAt, OrderByType.Desc) // 按创建时间倒序排序
                .Select((e, a) => new AppVersionSummary
                {
                    Id = e.Id,
                    AppId = e.AppId,
                    AppName = a.Name,
                    Title = e.Title,
                    Contents = e.Contents,
                    Platform = e.Platform,
                    VersionName = e.VersionName,
                    VersionCode = e.VersionCode,
                    Url = e.Url,
                    IsStablePublish = e.IsStablePublish,
                    IsSilently = e.IsSilently,
                    IsMandatory = e.IsMandatory,
                    CreatedById = e.CreatedById,
                    CreatedAt = e.CreatedAt,
                    UpdatedById = e.UpdatedById,
                    UpdatedAt = e.UpdatedAt,
                    DeletedById = e.DeletedById,
                    DeletedAt = e.DeletedAt
                }) // 映射到摘要
                .ToListAsync();

            return entities; // 返回查询结果，可能为 null
        }

        /// <summary>
        /// 分页查询未删除的App版本记录，支持名称模糊搜索。
        /// </summary>
        /// <param name="pageNumber">页码，从 1 开始。</param>
        /// <param name="pageSize">每页记录数。</param>
        /// <param name="appId">AppId，用于精确搜索（可选）。</param>
        /// <param name="orderByField">排序字段（可选）。</param>
        /// <param name="orderByType">排序方式（可选）。</param>
        /// <returns>
        /// 元组包含分页结果：
        /// - Data：当前页的数据列表（可能为空）。
        /// - TotalCount：满足查询条件的总记录数。
        /// </returns>
        public async Task<(List<AppVersionSummary> Data, int TotalCount)> GetPagedSummaryAsync(
            int pageNumber,
            int pageSize,
            string? appId = null,
            string? orderByField = null,
            string? orderByType = null)
        {
            // 排序字段映射：前端字段名 → 数据库字段名
            orderByField = (orderByField ?? "createdAt") switch
            {
                "id" => "id",
                "createdAt" => "created_at",
                "versionCode" => "version_code",
                _ => "created_at"
            };
            orderByType = orderByType ?? "DESC";

            var query = _db.Queryable<AppVersion, App>((e, a) => new object[]
            {
                    JoinType.Left, e.AppId == a.AppId, // 连接其他表
            })
            .Where(e => e.DeletedAt == null); // 查询未删除的记录

            // 如果提供了 AppId，则按 AppId 进行筛选
            if (!string.IsNullOrWhiteSpace(appId))
            {
                query = query.Where(e => e.AppId == appId);
            }

            // 获取总记录数
            var totalCount = await query.CountAsync();

            // 分页查询，并按创建时间倒序排序
            var pagedData = await query
                .OrderBy($"e.{orderByField} {orderByType}")
                .Select((e, a) => new AppVersionSummary
                {
                    Id = e.Id,
                    AppId = e.AppId,
                    AppName = a.Name,
                    Title = e.Title,
                    Contents = e.Contents,
                    Platform = e.Platform,
                    VersionName = e.VersionName,
                    VersionCode = e.VersionCode,
                    Url = e.Url,
                    IsStablePublish = e.IsStablePublish,
                    IsSilently = e.IsSilently,
                    IsMandatory = e.IsMandatory,
                    CreatedById = e.CreatedById,
                    CreatedAt = e.CreatedAt,
                    UpdatedById = e.UpdatedById,
                    UpdatedAt = e.UpdatedAt,
                    DeletedById = e.DeletedById,
                    DeletedAt = e.DeletedAt
                }) // 映射到摘要
                .ToPageListAsync(pageNumber, pageSize);

            return (pagedData, totalCount); // 返回分页结果
        }

        #endregion
    }
}