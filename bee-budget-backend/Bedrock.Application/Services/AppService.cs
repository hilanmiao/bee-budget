using Bedrock.Application.DataTransferObjects;
using Bedrock.Application.Interfaces;
using Bedrock.Core.Entities;
using Microsoft.Extensions.Logging;
using SqlSugar;
using System.Data;

namespace Bedrock.Application.Services
{
    /// <summary>
    /// App应用服务实现。
    /// <para>
    /// 支持软删除（通过 DeletedAt 字段标记），所有查询默认过滤已删除记录。
    /// 业务规则：AppId 保证全局唯一。
    /// </para>
    /// </summary>
    public class AppService : IAppService
    {
        private readonly ILogger<AppService> _logger;
        private readonly ISqlSugarClient _db;
        private readonly IAppRepository _appRepository;
        private readonly IAppVersionRepository _appVersionRepository;

        /// <summary>
        /// 初始化 <see cref="AppService"/> 类的新实例。
        /// </summary>
        /// <param name="db">SqlSugar 客户端，用于事务控制。</param>
        /// <param name="appRepository">App应用仓储。</param>
        public AppService(
            ILogger<AppService> logger,
            ISqlSugarClient db,
            IAppRepository appRepository,
            IAppVersionRepository appVersionRepository
            )
        {
            _logger = logger;
            _db = db;
            _appRepository = appRepository;
            _appVersionRepository = appVersionRepository;
        }

        /// <summary>
        /// 创建新的App应用。
        /// </summary>
        /// <param name="createDto">创建数据传输对象，包含 AppId 等字段。</param>
        /// <param name="operatorId">操作人App应用 ID，用于审计。</param>
        /// <returns>返回新创建记录的主键 ID。</returns>
        /// <exception cref="ArgumentException">当 AppId 已存在时抛出。</exception>
        /// <exception cref="ArgumentNullException">当 createDto 为 null 时抛出。</exception>
        public async Task<long> CreateAsync(CreateAppDto createDto, long operatorId)
        {
            if (createDto == null)
                throw new ArgumentNullException(nameof(createDto));

            if (string.IsNullOrWhiteSpace(createDto.AppId))
                throw new ArgumentException("App应用名称不能为空。", nameof(createDto.AppId));

            var trimmedAppId = createDto.AppId.Trim();

            var existsByName = await _appRepository.GetByAppIdAsync(trimmedAppId);
            if (existsByName != null)
                throw new ArgumentException($"App应用名称 '{trimmedAppId}' 已存在。");

            var entity = new App
            {
                AppId = trimmedAppId,
                Name = createDto.Name.Trim(),
                Description = createDto.Description?.Trim(),
                Icon = createDto.Icon?.Trim(),
                Screenshot = createDto.Screenshot?.Trim(),
                H5Url = createDto.H5Url?.Trim(),
                IsEnabled = true,
                CreatedById = operatorId,
                CreatedAt = DateTime.UtcNow,
            };

            var newId = await _appRepository.CreateAsync(entity);

            _logger.LogInformation("用户 {UserId} 创建App应用 {AppName} (ID: {AppId}) 成功。", operatorId, createDto.Name, newId);

            return newId;
        }

        /// <summary>
        /// 更新指定的App应用。
        /// </summary>
        /// <param name="updateDto">更新数据传输对象，必须包含有效 ID。</param>
        /// <param name="operatorId">操作人App应用 ID。</param>
        /// <returns>返回受影响的记录数（通常为 1，若记录不存在则为 0）。</returns>
        /// <exception cref="ArgumentException">当记录不存在、已删除或更新后违反唯一性约束时抛出。</exception>
        /// <exception cref="ArgumentNullException">当 updateDto 为 null 时抛出。</exception>
        /// <exception cref="InvalidOperationException">当数据库更新未影响任何行时抛出（并发冲突）。</exception>
        public async Task<long> UpdateAsync(UpdateAppDto updateDto, long operatorId)
        {
            if (updateDto == null)
                throw new ArgumentNullException(nameof(updateDto));
            if (updateDto.Id <= 0)
                throw new ArgumentException("无效的App应用 ID。", nameof(updateDto.Id));

            var entity = await _appRepository.GetAsync(updateDto.Id);
            if (entity == null)
                throw new ArgumentException("指定的App应用不存在或已被删除。", nameof(updateDto.Id));

            entity.Description = updateDto.Description?.Trim();
            entity.Icon = updateDto.Icon?.Trim();
            entity.Screenshot = updateDto.Screenshot?.Trim();
            entity.H5Url = updateDto.H5Url?.Trim();
            entity.IsEnabled = updateDto.IsEnabled;
            entity.UpdatedById = operatorId;
            entity.UpdatedAt = DateTime.UtcNow;

            var rowsAffected = await _appRepository.UpdateAsync(entity);
            if (rowsAffected == 0)
            {
                _logger.LogWarning("用户 {UserId} 更新App应用 {AppName} (ID: {AppId}) 失败，未影响任何数据库记录。", operatorId, entity.Name, entity.Id);
                throw new InvalidOperationException($"用户 {operatorId} 更新App应用 {entity.Name} (ID: {entity.Id}) 失败，未影响任何数据库记录。");
            }

            _logger.LogInformation("用户 {UserId} 更新App应用 {AppName} (ID: {AppId}) 成功。", operatorId, entity.Name, entity.Id);

            return entity.Id;
        }

        /// <summary>
        /// 软删除指定的App应用。
        /// </summary>
        /// <param name="id">要删除的App应用 ID。</param>
        /// <param name="operatorId">操作人App应用 ID。</param>
        /// <returns>返回被删除的记录 ID。</returns>
        /// <exception cref="ArgumentException">当记录不存在或已被删除时抛出。</exception>
        public async Task<long> DeleteAsync(long id, long operatorId)
        {
            if (id <= 0)
                throw new ArgumentException("无效的App应用 ID。", nameof(id));

            var entity = await _appRepository.GetAsync(id);
            if (entity == null)
                throw new ArgumentException("App应用不存在或已被删除。");

            // 使用 UseTranAsync<long> 显式指定返回类型
            DbResult<long> result = await _db.Ado.UseTranAsync<long>(
                async () =>
                {
                    var rowsAffected = 0;

                    // 删除关联数据
                    rowsAffected = await _appVersionRepository.DeleteByAppIdAsync(entity.AppId, operatorId);
                    //if (rowsAffected == 0)
                    //    throw new InvalidOperationException("删除关联数据失败：未影响任何数据库记录。");

                    // 执行软删除
                    entity.DeletedAt = DateTime.UtcNow;
                    entity.DeletedById = operatorId;
                    rowsAffected = await _appRepository.DeleteAsync(entity);

                    if (rowsAffected == 0)
                    {
                        _logger.LogWarning("用户 {UserId} 删除App应用 {AppName} (ID: {AppId}) 失败，未影响任何数据库记录。", operatorId, entity.Name, entity.Id);
                        throw new InvalidOperationException($"用户 {operatorId} 删除App应用 {entity.Name} (ID: {entity.Id}) 失败，未影响任何数据库记录。");
                    }

                    return id; // 事务成功，返回 ID
                },
                // 可选：使用 errorCallBack 记录日志
                ex =>
                {
                    _logger.LogWarning("用户 {UserId} 删除App应用 {AppName} (ID: {AppId}) 失败，{ErrorMessage}。", operatorId, entity.Name, entity.Id, ex.Message);
                }
            );

            // 检查结果并处理错误
            if (!result.IsSuccess)
            {
                // 关键：直接抛出原始异常以保留完整的 StackTrace
                throw result.ErrorException;
            }

            _logger.LogInformation("用户 {UserId} 删除App应用 {AppName} (ID: {AppId}) 成功。", operatorId, entity.Name, entity.Id);

            // 返回成功结果
            return result.Data;
        }

        /// <summary>
        /// 批量软删除App应用。
        /// </summary>
        /// <param name="ids">要删除的App应用 ID 列表。</param>
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

            // 查询待删除的App应用实体
            var entities = await _appRepository.GetByIdsAsync(idList);
            if (entities.Count != idList.Count)
                throw new ArgumentException("部分App应用不存在或已被删除。");

            var appIds = entities.Select(e => e.AppId).ToList();

            // 使用 UseTranAsync 替代 BeginTranAsync
            DbResult<int> result = await _db.Ado.UseTranAsync<int>(
                async () =>
                {
                    // 批量删除关联的字典项
                    await _appVersionRepository.DeleteByAppIdsAsync(appIds, operatorId);

                    // 批量软删除App应用
                    var deletedCount = await _appRepository.DeleteBatchAsync(idList, operatorId);

                    // 校验删除数量
                    if (deletedCount != idList.Count)
                    {
                        _logger.LogWarning("用户 {UserId} 批量删除App应用失败，期望删除 {Count} 条，但实际仅成功删除 {DeletedCount} 条。", operatorId, idList.Count, deletedCount);
                        throw new ArgumentException($"用户 {operatorId} 批量删除App应用失败，期望删除 {idList.Count} 条，但实际仅成功删除 {deletedCount} 条。");
                    }

                    // 事务成功，返回删除数量
                    return deletedCount;
                },
                // 可选：错误回调，用于记录日志
                ex =>
                {
                    _logger.LogWarning("用户 {UserId} 批量删除App应用失败，{ErrorMessage}。", operatorId, ex.Message);
                }
            );

            // 检查事务结果
            if (!result.IsSuccess)
            {
                // 直接抛出原始异常，保留完整的 StackTrace
                throw result.ErrorException;
            }

            _logger.LogInformation("用户 {UserId} 批量删除App应用成功，删除数量 {DeletedCount}。", operatorId, entities.Count);

            // 返回成功结果
            return result.Data;
        }

        /// <summary>
        /// 根据主键获取单条未删除的App应用详情。
        /// </summary>
        /// <param name="id">App应用唯一标识。</param>
        /// <returns>返回匹配的 DTO；若未找到或已删除，则返回 null。</returns>
        public async Task<AppDto?> GetAsync(long id)
        {
            var entity = await _appRepository.GetAsync(id);
            if (entity == null)
                return null;
            return MapToDto(entity);
        }

        /// <summary>
        /// 查询未删除的App应用列表，支持按App应用名称模糊搜索。
        /// </summary>
        /// <param name="name">App应用名称，用于模糊搜索（可选）。</param>
        /// <returns>返回匹配条件的 DTO 列表（可能为空）。</returns>
        public async Task<List<AppDto>> GetAllAsync(string? name = null)
        {
            var entities = await _appRepository.GetAllAsync(name);
            return entities.Select(MapToDto).ToList();
        }

        /// <summary>
        /// 分页查询未删除的App应用列表，支持名称、手机号码模糊搜索和状态精确筛选。
        /// </summary>
        /// <param name="pageNumber">页码，从 1 开始。</param>
        /// <param name="pageSize">每页数量。</param>
        /// <param name="name">App应用名称，用于模糊搜索（可选）。</param>
        /// <param name="orderByField">排序字段（可选）。</param>
        /// <param name="orderByType">排序方式（可选）。</param>
        /// <returns>返回分页结果对象，包含数据和分页元信息。</returns>
        public async Task<PaginationResult<AppDto>> GetPagedAsync(
            int pageNumber,
            int pageSize,
            string? name = null,
            string? orderByField = null,
            string? orderByType = null)
        {
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = 10;

            // 调用仓储层获取分页数据和总数
            var (data, totalCount) = await _appRepository.GetPagedAsync(pageNumber, pageSize, name, orderByField, orderByType);
            var pagedData = data.Select(entity => MapToDto(entity)).ToList();

            // 计算总页数
            int totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            // 封装为 PaginationResult
            return new PaginationResult<AppDto>(
                items: pagedData,
                totalPages: totalPages,
                totalItems: totalCount,
                currentPage: pageNumber,
                pageSize: pageSize);
        }

        /// <summary>
        /// 根据AppId获取唯一未删除的App应用详情。
        /// </summary>
        /// <param name="appId">AppId（如“__UNI__9B0E754”），用于精确匹配。</param>
        /// <returns>返回匹配的 DTO；若未找到或已删除，则返回 null。</returns>
        public async Task<AppDto?> GetByAppIdAsync(string appId)
        {
            if (string.IsNullOrWhiteSpace(appId))
                return null;

            var entity = await _appRepository.GetByAppIdAsync(appId.Trim());
            if (entity == null)
                return null;
            return MapToDto(entity);
        }

        /// <summary>
        /// 将领域实体映射为应用层数据传输对象。
        /// </summary>
        /// <param name="entity">源实体对象。</param>
        /// <returns>映射后的 AppDto 实例。</returns>
        private static AppDto MapToDto(App entity)
        {
            return new AppDto
            {
                Id = entity.Id,
                AppId = entity.AppId,
                Name = entity.Name,
                Description = entity.Description,
                Icon = entity.Icon,
                Screenshot = entity.Screenshot,
                H5Url = entity.H5Url,
                IsEnabled = entity.IsEnabled,
                CreatedAt = entity.CreatedAt,
                UpdatedAt = entity.UpdatedAt,
                DeletedAt = entity.DeletedAt,
                DeletedById = entity.DeletedById
            };
        }

    }
}