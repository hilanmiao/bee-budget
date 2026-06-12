using Bedrock.Application.DataTransferObjects;
using Bedrock.Application.Interfaces;
using Bedrock.Core.Entities;
using Microsoft.Extensions.Logging;
using SqlSugar;
using System.Data;

namespace Bedrock.Application.Services
{
    /// <summary>
    /// App版本服务实现。
    /// <para>
    /// 支持软删除（通过 DeletedAt 字段标记），所有查询默认过滤已删除记录。
    /// 业务规则：AppId 保证全局唯一。
    /// </para>
    /// </summary>
    public class AppVersionService : IAppVersionService
    {
        private readonly ILogger<AppVersionService> _logger;
        private readonly ISqlSugarClient _db;
        private readonly IAppVersionRepository _appVersionRepository;

        /// <summary>
        /// 初始化 <see cref="AppVersionService"/> 类的新实例。
        /// </summary>
        /// <param name="db">SqlSugar 客户端，用于事务控制。</param>
        /// <param name="appVersionRepository">App版本仓储。</param>
        public AppVersionService(
            ILogger<AppVersionService> logger,
            ISqlSugarClient db,
            IAppVersionRepository appVersionRepository
            )
        {
            _logger = logger;
            _db = db;
            _appVersionRepository = appVersionRepository;
        }

        /// <summary>
        /// 创建新的App版本。
        /// </summary>
        /// <param name="createDto">创建数据传输对象，包含 AppId 等字段。</param>
        /// <param name="operatorId">操作人App版本 ID，用于审计。</param>
        /// <returns>返回新创建记录的主键 ID。</returns>
        /// <exception cref="ArgumentException">当 AppId 和 VersionCode 已存在时抛出。</exception>
        /// <exception cref="ArgumentNullException">当 createDto 为 null 时抛出。</exception>
        public async Task<long> CreateAsync(CreateAppVersionDto createDto, long operatorId)
        {
            if (createDto == null)
                throw new ArgumentNullException(nameof(createDto));

            if (string.IsNullOrWhiteSpace(createDto.VersionName))
                throw new ArgumentException("App版本名称不能为空。", nameof(createDto.VersionName));

            var trimmedAppId = createDto.AppId.Trim();

            // 检查版本号
            var maxVersionEntity = await _appVersionRepository.GetMaxVersionAsync(trimmedAppId);
            if (maxVersionEntity != null && maxVersionEntity.VersionCode >= createDto.VersionCode)
            {
                throw new InvalidOperationException("版本号不能小于等于已存在的版本号");
            }

            var entity = new AppVersion
            {
                AppId = trimmedAppId,
                Title = createDto.Title,
                Contents = createDto.Contents,
                Platform = createDto.Platform,
                VersionName = createDto.VersionName,
                VersionCode = createDto.VersionCode,
                Url = createDto.Url,
                IsStablePublish = createDto.IsStablePublish,
                IsSilently = createDto.IsSilently,
                IsMandatory = createDto.IsMandatory,
                CreatedById = operatorId,
                CreatedAt = DateTime.UtcNow
            };

            var newId = await _appVersionRepository.CreateAsync(entity);

            _logger.LogInformation("用户 {UserId} 创建App版本 {AppVersionName} (ID: {AppVersionId}) 成功。", operatorId, createDto.VersionName, newId);

            return newId;
        }

        /// <summary>
        /// 批量创建App版本。
        /// </summary>
        /// <param name="createDtos">创建 DTO 集合。</param>
        /// <param name="operatorId">操作人用户 ID。</param>
        /// <returns>返回成功插入的记录数量；若集合为空则返回 0。</returns>
        /// <exception cref="ArgumentException">当存在重复实际值或标签时抛出。</exception>
        public async Task<int> CreateBatchAsync(IEnumerable<CreateAppVersionDto> createDtos, long operatorId)
        {
            if (createDtos == null || !createDtos.Any())
                return 0;

            var dtoList = createDtos.ToList();

            foreach (var dto in dtoList)
            {
                if (string.IsNullOrWhiteSpace(dto.VersionName))
                    throw new ArgumentException("App版名称不能为空。");
            }

            var versionNames = dtoList.Select(d => d.VersionName.Trim()).Distinct().ToList();

            var existingEntities = await _appVersionRepository.GetAllAsync(dtoList.First().AppId);
            var existingVersionNames = existingEntities.Select(e => e.VersionName).ToHashSet();

            if (versionNames.Any(t => existingVersionNames.Contains(t)))
                throw new ArgumentException("部分App版本名称已存在。");

            var entities = dtoList.Select(dto => new AppVersion
            {
                AppId = dto.AppId,
                Title = dto.Title,
                Contents = dto.Contents,
                Platform = dto.Platform,
                VersionName = dto.VersionName,
                VersionCode = dto.VersionCode,
                Url = dto.Url,
                IsStablePublish = dto.IsStablePublish,
                IsSilently = dto.IsSilently,
                IsMandatory = dto.IsMandatory,
                CreatedById = operatorId,
                CreatedAt = DateTime.UtcNow,
                UpdatedById = operatorId,
                UpdatedAt = DateTime.UtcNow
            }).ToList();

            var insertedCount = await _appVersionRepository.CreateBatchAsync(entities);

            _logger.LogInformation("用户 {UserId} 批量创建App版本成功，创建数量 {InsertedCount}。", operatorId, insertedCount);

            return insertedCount;
        }

        /// <summary>
        /// 更新指定的App版本。
        /// </summary>
        /// <param name="updateDto">更新数据传输对象，必须包含有效 ID。</param>
        /// <param name="operatorId">操作人App版本 ID。</param>
        /// <returns>返回受影响的记录数（通常为 1，若记录不存在则为 0）。</returns>
        /// <exception cref="ArgumentException">当记录不存在、已删除或更新后违反唯一性约束时抛出。</exception>
        /// <exception cref="ArgumentNullException">当 updateDto 为 null 时抛出。</exception>
        /// <exception cref="InvalidOperationException">当数据库更新未影响任何行时抛出（并发冲突）。</exception>
        public async Task<long> UpdateAsync(UpdateAppVersionDto updateDto, long operatorId)
        {
            if (updateDto == null)
                throw new ArgumentNullException(nameof(updateDto));
            if (updateDto.Id <= 0)
                throw new ArgumentException("无效的App版本 ID。", nameof(updateDto.Id));

            var entity = await _appVersionRepository.GetAsync(updateDto.Id);
            if (entity == null)
                throw new ArgumentException("指定的App版本不存在或已被删除。", nameof(updateDto.Id));

            // 上线后不能再修改
            if (entity.IsStablePublish)
                throw new ArgumentException("当前App版本已上线不允许修改。", nameof(updateDto.Id));

            entity.Title = updateDto.Title;
            entity.Contents = updateDto.Contents;
            entity.Platform = updateDto.Platform;
            entity.VersionName = updateDto.VersionName;
            entity.VersionCode = updateDto.VersionCode;
            entity.Url = updateDto.Url;
            entity.IsStablePublish = updateDto.IsStablePublish;
            entity.IsSilently = updateDto.IsSilently;
            entity.IsMandatory = updateDto.IsMandatory;
            entity.UpdatedById = operatorId;
            entity.UpdatedAt = DateTime.UtcNow;

            var rowsAffected = await _appVersionRepository.UpdateAsync(entity);
            if (rowsAffected == 0)
            {
                _logger.LogWarning("用户 {UserId} 更新App版本 {AppVersionName} (ID: {AppVersionId}) 失败，未影响任何数据库记录。", operatorId, entity.VersionName, entity.Id);
                throw new InvalidOperationException($"用户 {operatorId} 更新App版本 {entity.VersionName} (ID: {entity.Id}) 失败，未影响任何数据库记录。");
            }

            _logger.LogInformation("用户 {UserId} 更新App版本 {AppVersionName} (ID: {AppVersionId}) 成功。", operatorId, entity.VersionName, entity.Id);

            return entity.Id;
        }

        /// <summary>
        /// 批量更新App版本。
        /// </summary>
        /// <param name="updateDtos">更新 DTO 集合，每个必须包含有效 ID。</param>
        /// <param name="operatorId">操作人用户 ID。</param>
        /// <returns>返回成功更新的记录数量；若集合为空则返回 0。</returns>
        /// <exception cref="ArgumentException">当任一记录不存在或违反业务规则时抛出。</exception>
        public async Task<int> UpdateBatchAsync(IEnumerable<UpdateAppVersionDto> updateDtos, long operatorId)
        {
            if (updateDtos == null || !updateDtos.Any())
                return 0;

            var dtoList = updateDtos.ToList();
            var entities = new List<AppVersion>();

            // 1. 验证并准备实体（这部分在事务外，因为主要是查询和业务逻辑校验）
            foreach (var dto in dtoList)
            {
                if (dto.Id <= 0)
                    throw new ArgumentException($"无效的 ID: {dto.Id}");

                var entity = await _appVersionRepository.GetAsync(dto.Id);
                if (entity == null)
                    throw new ArgumentException($"ID 为 {dto.Id} 的App版本不存在或已被删除。");

                var trimmedVersionName = dto.VersionName.Trim();

                //// 检查 Value 是否被占用（排除自身）
                //if (entity.Value != trimmedValue)
                //{
                //    var exists = await _AppVersionRepository.GetByValueAsync(trimmedValue);
                //    if (exists != null && exists.Id != dto.Id)
                //        throw new ArgumentException($"App版本实际值 '{trimmedValue}' 已被占用。");
                //}

                // 准备待更新的实体
                entity.Title = dto.Title;
                entity.Contents = dto.Contents;
                entity.Platform = dto.Platform;
                entity.VersionName = dto.VersionName;
                entity.VersionCode = dto.VersionCode;
                entity.Url = dto.Url;
                entity.IsStablePublish = dto.IsStablePublish;
                entity.IsSilently = dto.IsSilently;
                entity.IsMandatory = dto.IsMandatory;
                entity.UpdatedById = operatorId;
                entity.UpdatedAt = DateTime.UtcNow;

                entities.Add(entity);
            }

            // 2. 使用 UseTranAsync 执行数据库更新操作（核心事务部分）
            DbResult<int> result = await _db.Ado.UseTranAsync<int>(
                async () =>
                {
                    // 执行批量更新
                    var updatedCount = await _appVersionRepository.UpdateBatchAsync(entities);

                    // 校验更新数量
                    if (updatedCount != entities.Count)
                    {
                        _logger.LogWarning("用户 {UserId} 批量更新App版本失败，期望更新 {Count} 条，但实际仅成功更新 {UpdatedCount} 条。", operatorId, entities.Count, updatedCount);
                        throw new ArgumentException($"用户 {operatorId} 批量更新App版本失败，期望更新 {entities.Count} 条，但实际仅成功更新 {updatedCount} 条。");
                    }

                    // 事务成功，返回更新数量
                    return updatedCount;
                },
                // 可选：错误回调，记录日志
                ex =>
                {
                    _logger.LogWarning("用户 {UserId} 批量更新App版本失败，{ErrorMessage}。", operatorId, ex.Message);
                }
            );

            // 3. 检查事务结果
            if (!result.IsSuccess)
            {
                // 直接抛出原始异常，保留完整的 StackTrace
                throw result.ErrorException;
            }

            _logger.LogInformation("用户 {UserId} 批量更新App版本成功，更新数量 {UpdatedCount}。", operatorId, entities.Count);

            // 返回成功结果
            return result.Data;
        }

        /// <summary>
        /// 软删除指定的App版本。
        /// </summary>
        /// <param name="id">要删除的App版本 ID。</param>
        /// <param name="operatorId">操作人App版本 ID。</param>
        /// <returns>返回被删除的记录 ID。</returns>
        /// <exception cref="ArgumentException">当记录不存在或已被删除时抛出。</exception>
        public async Task<long> DeleteAsync(long id, long operatorId)
        {
            if (id <= 0)
                throw new ArgumentException("无效的App版本 ID。", nameof(id));

            var entity = await _appVersionRepository.GetAsync(id);
            if (entity == null)
                throw new ArgumentException("App版本不存在或已被删除。");

            // 使用 UseTranAsync<long> 显式指定返回类型
            DbResult<long> result = await _db.Ado.UseTranAsync<long>(
                async () =>
                {
                    var rowsAffected = 0;

                    // 执行软删除
                    entity.DeletedAt = DateTime.UtcNow;
                    entity.DeletedById = operatorId;
                    rowsAffected = await _appVersionRepository.DeleteAsync(entity);

                    if (rowsAffected == 0)
                    {
                        _logger.LogWarning("用户 {UserId} 删除App版本 {AppVersionName} (ID: {AppVersionId}) 失败，未影响任何数据库记录。", operatorId, entity.VersionName, entity.Id);
                        throw new InvalidOperationException($"用户 {operatorId} 删除App版本 {entity.VersionName} (ID: {entity.Id}) 失败，未影响任何数据库记录。");
                    }

                    return id; // 事务成功，返回 ID
                },
                // 可选：使用 errorCallBack 记录日志
                ex =>
                {
                    _logger.LogWarning("用户 {UserId} 删除App版本 {AppVersionName} (ID: {AppVersionId}) 失败，{ErrorMessage}。", operatorId, entity.VersionName, entity.Id, ex.Message);
                }
            );

            // 检查结果并处理错误
            if (!result.IsSuccess)
            {
                // 关键：直接抛出原始异常以保留完整的 StackTrace
                throw result.ErrorException;
            }

            _logger.LogInformation("用户 {UserId} 删除App版本 {AppVersionName} (ID: {AppVersionId}) 成功。", operatorId, entity.VersionName, entity.Id);

            // 返回成功结果
            return result.Data;
        }

        /// <summary>
        /// 批量软删除App版本。
        /// </summary>
        /// <param name="ids">要删除的App版本 ID 列表。</param>
        /// <param name="operatorId">操作人用户 ID。</param>
        /// <returns>返回成功标记为删除的记录数量；若 ID 列表为空则返回 0。</returns>
        /// <exception cref="ArgumentException">当部分 ID 不存在或已被删除时可能抛出。</exception>
        public async Task<int> DeleteBatchAsync(IEnumerable<long> ids, long operatorId)
        {
            if (ids == null || !ids.Any())
                return 0;

            var idList = ids.Distinct().Where(id => id > 0).ToList();
            if (!idList.Any())
                return 0;

            // 查询待删除的App版本实体
            var entities = await _appVersionRepository.GetByIdsAsync(idList);
            if (entities.Count != idList.Count)
                throw new ArgumentException("部分App版本不存在或已被删除。");

            // 使用 UseTranAsync 替代 BeginTranAsync
            DbResult<int> result = await _db.Ado.UseTranAsync<int>(
                async () =>
                {
                    // 批量软删除App版本
                    var deletedCount = await _appVersionRepository.DeleteBatchAsync(idList, operatorId);

                    // 校验删除数量
                    if (deletedCount != idList.Count)
                    {
                        _logger.LogWarning("用户 {UserId} 批量删除App版本失败，期望删除 {Count} 条，但实际仅成功删除 {DeletedCount} 条。", operatorId, idList.Count, deletedCount);
                        throw new ArgumentException($"用户 {operatorId} 批量删除App版本失败，期望删除 {idList.Count} 条，但实际仅成功删除 {deletedCount} 条。");
                    }

                    // 事务成功，返回删除数量
                    return deletedCount;
                },
                // 可选：错误回调，用于记录日志
                ex =>
                {
                    _logger.LogWarning("用户 {UserId} 批量删除App版本失败，{ErrorMessage}。", operatorId, ex.Message);
                }
            );

            // 检查事务结果
            if (!result.IsSuccess)
            {
                // 直接抛出原始异常，保留完整的 StackTrace
                throw result.ErrorException;
            }

            _logger.LogInformation("用户 {UserId} 批量删除App版本成功，删除数量 {DeletedCount}。", operatorId, entities.Count);

            // 返回成功结果
            return result.Data;
        }

        /// <summary>
        /// 根据主键获取单条未删除的App版本详情。
        /// </summary>
        /// <param name="id">App版本唯一标识。</param>
        /// <returns>返回匹配的 DTO；若未找到或已删除，则返回 null。</returns>
        public async Task<AppVersionDto?> GetAsync(long id)
        {
            var entity = await _appVersionRepository.GetAsync(id);
            if (entity == null)
                return null;
            return MapToDto(entity);
        }

        /// <summary>
        /// 查询未删除的App版本列表，支持按App版本名称模糊搜索。
        /// </summary>
        /// <param name="appId">AppId，用于精确搜索（可选）。</param>
        /// <returns>返回匹配条件的 DTO 列表（可能为空）。</returns>
        public async Task<List<AppVersionDto>> GetAllAsync(string? appId = null)
        {
            var entities = await _appVersionRepository.GetAllAsync(appId);
            return entities.Select(MapToDto).ToList();
        }

        /// <summary>
        /// 分页查询未删除的App版本列表，支持名称、手机号码模糊搜索和状态精确筛选。
        /// </summary>
        /// <param name="pageNumber">页码，从 1 开始。</param>
        /// <param name="pageSize">每页数量。</param>
        /// <param name="appId">AppId，用于精确搜索（可选）。</param>
        /// <param name="orderByField">排序字段（可选）。</param>
        /// <param name="orderByType">排序方式（可选）。</param>
        /// <returns>返回分页结果对象，包含数据和分页元信息。</returns>
        public async Task<PaginationResult<AppVersionDto>> GetPagedAsync(
            int pageNumber,
            int pageSize,
            string? appId = null,
            string? orderByField = null,
            string? orderByType = null)
        {
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = 10;

            // 调用仓储层获取分页数据和总数
            var (data, totalCount) = await _appVersionRepository.GetPagedAsync(pageNumber, pageSize, appId, orderByField, orderByType);
            var pagedData = data.Select(entity => MapToDto(entity)).ToList();

            // 计算总页数
            int totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            // 封装为 PaginationResult
            return new PaginationResult<AppVersionDto>(
                items: pagedData,
                totalPages: totalPages,
                totalItems: totalCount,
                currentPage: pageNumber,
                pageSize: pageSize);
        }

        /// <summary>
        /// 根据 AppId 获取已上线发行的最大版本信息。
        /// </summary>
        /// <param name="appId">记录的 AppId。</param>
        /// <returns>返回与 AppId 匹配的数据传输对象。</returns>
        public async Task<AppVersionDto?> GetMaxVersionAsync(string appId)
        {
            var entity = await _appVersionRepository.GetMaxVersionAsync(appId);
            if (entity == null)
            {
                return null;
            }
            return MapToDto(entity);
        }

        /// <summary>
        /// 根据主键获取单条未删除的App版本详情。
        /// </summary>
        /// <param name="id">App版本唯一标识。</param>
        /// <returns>返回匹配的 DTO；若未找到或已删除，则返回 null。</returns>
        public async Task<AppVersionSummary?> GetSummaryAsync(long id)
        {
            var entity = await _appVersionRepository.GetSummaryAsync(id);
            if (entity == null)
                return null;
            return entity;
        }

        /// <summary>
        /// 查询未删除的App版本列表，支持按App版本名称模糊搜索。
        /// </summary>
        /// <param name="appId">AppId，用于精确搜索（可选）。</param>
        /// <returns>返回匹配条件的 DTO 列表（可能为空）。</returns>
        public async Task<List<AppVersionSummary>> GetAllSummaryAsync(string? appId = null)
        {
            var entities = await _appVersionRepository.GetAllSummaryAsync(appId);
            return entities;
        }

        /// <summary>
        /// 分页查询未删除的App版本列表，支持名称、手机号码模糊搜索和状态精确筛选。
        /// </summary>
        /// <param name="pageNumber">页码，从 1 开始。</param>
        /// <param name="pageSize">每页数量。</param>
        /// <param name="appId">AppId，用于精确搜索（可选）。</param>
        /// <param name="orderByField">排序字段（可选）。</param>
        /// <param name="orderByType">排序方式（可选）。</param>
        /// <returns>返回分页结果对象，包含数据和分页元信息。</returns>
        public async Task<PaginationResult<AppVersionSummary>> GetPagedSummaryAsync(
            int pageNumber,
            int pageSize,
            string? appId = null,
            string? orderByField = null,
            string? orderByType = null)
        {
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = 10;

            // 调用仓储层获取分页数据和总数
            var (data, totalCount) = await _appVersionRepository.GetPagedSummaryAsync(pageNumber, pageSize, appId, orderByField, orderByType);
            var pagedData = data.ToList();

            // 计算总页数
            int totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            // 封装为 PaginationResult
            return new PaginationResult<AppVersionSummary>(
                items: pagedData,
                totalPages: totalPages,
                totalItems: totalCount,
                currentPage: pageNumber,
                pageSize: pageSize);
        }

        /// <summary>
        /// 将领域实体映射为应用层数据传输对象。
        /// </summary>
        /// <param name="entity">源实体对象。</param>
        /// <returns>映射后的 AppVersionDto 实例。</returns>
        private static AppVersionDto MapToDto(AppVersion entity)
        {
            return new AppVersionDto
            {
                Id = entity.Id,
                AppId = entity.AppId,
                Title = entity.Title,
                Contents = entity.Contents,
                Platform = entity.Platform,
                VersionName = entity.VersionName,
                VersionCode = entity.VersionCode,
                Url = entity.Url,
                IsStablePublish = entity.IsStablePublish,
                IsSilently = entity.IsSilently,
                IsMandatory = entity.IsMandatory,
                CreatedAt = entity.CreatedAt,
                UpdatedAt = entity.UpdatedAt,
            };
        }

    }
}