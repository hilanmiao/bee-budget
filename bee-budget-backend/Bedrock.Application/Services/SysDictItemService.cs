using Bedrock.Application.DataTransferObjects;
using Bedrock.Application.Interfaces;
using Bedrock.Core.Entities;
using Microsoft.Extensions.Logging;
using SqlSugar;
using System.Data;

namespace Bedrock.Application.Services
{
    /// <summary>
    /// 系统字典项应用服务实现。
    /// <para>
    /// 负责字典项的创建、更新、删除、查询等业务逻辑，支持软删除语义。
    /// 所有查询方法默认不返回已删除记录。
    /// 业务规则：<c>Value</c> 与 <c>Label</c> 全局唯一。
    /// </para>
    /// </summary>
    public class SysDictItemService : ISysDictItemService
    {
        private readonly ILogger<SysDictItemService> _logger;
        private readonly ISqlSugarClient _db;
        private readonly ISysDictItemRepository _sysDictItemRepository;

        /// <summary>
        /// 初始化 <see cref="SysDictItemService"/> 类的新实例。
        /// </summary>
        /// <param name="db">SqlSugar 客户端，用于事务控制。</param>
        /// <param name="sysDictItemRepository">字典项仓储。</param>
        public SysDictItemService(
            ILogger<SysDictItemService> logger,
            ISqlSugarClient db,
            ISysDictItemRepository sysDictItemRepository
            )
        {
            _logger = logger;
            _db = db;
            _sysDictItemRepository = sysDictItemRepository;
        }

        /// <summary>
        /// 创建新的字典项。
        /// </summary>
        /// <param name="createDto">创建数据传输对象，包含 <c>Value</c>、<c>Label</c> 等字段。</param>
        /// <param name="operatorId">操作人用户 ID，用于审计。</param>
        /// <returns>返回新创建记录的主键 ID。</returns>
        /// <exception cref="ArgumentException">当 <c>Value</c> 或 <c>Label</c> 已存在时抛出。</exception>
        /// <exception cref="ArgumentNullException">当 <paramref name="createDto"/> 为 null 时抛出。</exception>
        public async Task<long> CreateAsync(CreateSysDictItemDto createDto, long operatorId)
        {
            if (createDto == null)
                throw new ArgumentNullException(nameof(createDto));

            if (string.IsNullOrWhiteSpace(createDto.Value))
                throw new ArgumentException("字典项实际值不能为空。", nameof(createDto.Value));
            if (string.IsNullOrWhiteSpace(createDto.Label))
                throw new ArgumentException("字典项标签不能为空。", nameof(createDto.Label));

            var trimmedValue = createDto.Value.Trim();
            var trimmedLabel = createDto.Label.Trim();

            var entity = new SysDictItem
            {
                Label = trimmedLabel,
                Value = trimmedValue,
                CategoryCode = createDto.CategoryCode,
                Sort = createDto.Sort,
                CssClass = createDto.CssClass?.Trim(),
                ListClass = createDto.ListClass?.Trim(),
                IsDefault = createDto.IsDefault?.Trim(),
                Status = createDto.Status?.Trim() ?? "1",
                Remark = createDto.Remark?.Trim(),
                CreatedById = operatorId,
                CreatedAt = DateTime.UtcNow,
                UpdatedById = operatorId,
                UpdatedAt = DateTime.UtcNow
            };

            var newId = await _sysDictItemRepository.CreateAsync(entity);

            _logger.LogInformation("用户 {UserId} 创建字典项 {DictItemName} (ID: {DictItemId}) 成功。", operatorId, createDto.Label, newId);

            return newId;
        }

        /// <summary>
        /// 批量创建字典项。
        /// </summary>
        /// <param name="createDtos">创建 DTO 集合。</param>
        /// <param name="operatorId">操作人用户 ID。</param>
        /// <returns>返回成功插入的记录数量；若集合为空则返回 0。</returns>
        /// <exception cref="ArgumentException">当存在重复实际值或标签时抛出。</exception>
        public async Task<int> CreateBatchAsync(IEnumerable<CreateSysDictItemDto> createDtos, long operatorId)
        {
            if (createDtos == null || !createDtos.Any())
                return 0;

            var dtoList = createDtos.ToList();

            foreach (var dto in dtoList)
            {
                if (string.IsNullOrWhiteSpace(dto.Value))
                    throw new ArgumentException("字典项实际值不能为空。");
                if (string.IsNullOrWhiteSpace(dto.Label))
                    throw new ArgumentException("字典项标签不能为空。");
            }

            var values = dtoList.Select(d => d.Value.Trim()).Distinct().ToList();
            var labels = dtoList.Select(d => d.Label.Trim()).Distinct().ToList();

            var existingEntities = await _sysDictItemRepository.GetAllAsync(null, null);
            var existingValues = existingEntities.Select(e => e.Value).ToHashSet();
            var existingLabels = existingEntities.Select(e => e.Label).ToHashSet();

            if (values.Any(t => existingValues.Contains(t)))
                throw new ArgumentException("部分字典项实际值已存在。");
            if (labels.Any(n => existingLabels.Contains(n)))
                throw new ArgumentException("部分字典项标签已存在。");

            var entities = dtoList.Select(dto => new SysDictItem
            {
                Value = dto.Value.Trim(),
                Label = dto.Label.Trim(),
                CategoryCode = dto.CategoryCode,
                Sort = dto.Sort,
                CssClass = dto.CssClass?.Trim(),
                ListClass = dto.ListClass?.Trim(),
                IsDefault = dto.IsDefault?.Trim(),
                Status = dto.Status?.Trim() ?? "1",
                Remark = dto.Remark?.Trim(),
                CreatedById = operatorId,
                CreatedAt = DateTime.UtcNow,
                UpdatedById = operatorId,
                UpdatedAt = DateTime.UtcNow
            }).ToList();

            var insertedCount = await _sysDictItemRepository.CreateBatchAsync(entities);

            _logger.LogInformation("用户 {UserId} 批量创建字典项成功，创建数量 {InsertedCount}。", operatorId, insertedCount);

            return insertedCount;
        }

        /// <summary>
        /// 更新指定的字典项。
        /// </summary>
        /// <param name="updateDto">更新数据传输对象，必须包含有效 ID。</param>
        /// <param name="operatorId">操作人用户 ID。</param>
        /// <returns>返回更新后的记录 ID。</returns>
        /// <exception cref="ArgumentException">当记录不存在、已删除或更新后违反唯一性约束时抛出。</exception>
        /// <exception cref="ArgumentNullException">当 <paramref name="updateDto"/> 为 null 时抛出。</exception>
        /// <exception cref="InvalidOperationException">当数据库更新未影响任何行时抛出（并发冲突）。</exception>
        public async Task<long> UpdateAsync(UpdateSysDictItemDto updateDto, long operatorId)
        {
            if (updateDto == null)
                throw new ArgumentNullException(nameof(updateDto));
            if (updateDto.Id <= 0)
                throw new ArgumentException("无效的字典项 ID。", nameof(updateDto.Id));

            if (string.IsNullOrWhiteSpace(updateDto.Value))
                throw new ArgumentException("字典项实际值不能为空。", nameof(updateDto.Value));
            if (string.IsNullOrWhiteSpace(updateDto.Label))
                throw new ArgumentException("字典项标签不能为空。", nameof(updateDto.Label));

            var entity = await _sysDictItemRepository.GetAsync(updateDto.Id);
            if (entity == null)
                throw new ArgumentException("指定的字典项不存在或已被删除。", nameof(updateDto.Id));

            var trimmedValue = updateDto.Value.Trim();
            var trimmedLabel = updateDto.Label.Trim();

            entity.Value = trimmedValue;
            entity.Label = trimmedLabel;
            entity.CategoryCode = updateDto.CategoryCode;
            entity.Sort = updateDto.Sort;
            entity.CssClass = updateDto.CssClass?.Trim();
            entity.ListClass = updateDto.ListClass?.Trim();
            entity.IsDefault = updateDto.IsDefault?.Trim();
            entity.Status = updateDto.Status?.Trim() ?? entity.Status;
            entity.Remark = updateDto.Remark?.Trim();
            entity.UpdatedById = operatorId;
            entity.UpdatedAt = DateTime.UtcNow;

            var rowsAffected = await _sysDictItemRepository.UpdateAsync(entity);
            if (rowsAffected == 0)
            {
                _logger.LogWarning("用户 {UserId} 更新字典项 {DictItemName} (ID: {DictItemId}) 失败，未影响任何数据库记录。", operatorId, entity.Label, entity.Id);
                throw new InvalidOperationException($"用户 {operatorId} 更新字典项 {entity.Label} (ID: {entity.Id}) 失败，未影响任何数据库记录。");
            }

            _logger.LogInformation("用户 {UserId} 更新字典项 {DictItemName} (ID: {DictItemId}) 成功。", operatorId, entity.Label, entity.Id);

            return entity.Id;
        }

        /// <summary>
        /// 批量更新字典项。
        /// </summary>
        /// <param name="updateDtos">更新 DTO 集合，每个必须包含有效 ID。</param>
        /// <param name="operatorId">操作人用户 ID。</param>
        /// <returns>返回成功更新的记录数量；若集合为空则返回 0。</returns>
        /// <exception cref="ArgumentException">当任一记录不存在或违反业务规则时抛出。</exception>
        public async Task<int> UpdateBatchAsync(IEnumerable<UpdateSysDictItemDto> updateDtos, long operatorId)
        {
            if (updateDtos == null || !updateDtos.Any())
                return 0;

            var dtoList = updateDtos.ToList();
            var entities = new List<SysDictItem>();

            // 1. 验证并准备实体（这部分在事务外，因为主要是查询和业务逻辑校验）
            foreach (var dto in dtoList)
            {
                if (dto.Id <= 0)
                    throw new ArgumentException($"无效的 ID: {dto.Id}");

                var entity = await _sysDictItemRepository.GetAsync(dto.Id);
                if (entity == null)
                    throw new ArgumentException($"ID 为 {dto.Id} 的字典项不存在或已被删除。");

                var trimmedValue = dto.Value.Trim();
                var trimmedLabel = dto.Label.Trim();

                //// 检查 Value 是否被占用（排除自身）
                //if (entity.Value != trimmedValue)
                //{
                //    var exists = await _sysDictItemRepository.GetByValueAsync(trimmedValue);
                //    if (exists != null && exists.Id != dto.Id)
                //        throw new ArgumentException($"字典项实际值 '{trimmedValue}' 已被占用。");
                //}

                //// 检查 Label 是否被占用（排除自身）
                //if (entity.Label != trimmedLabel)
                //{
                //    var exists = await _sysDictItemRepository.GetByLabelAsync(trimmedLabel);
                //    if (exists != null && exists.Id != dto.Id)
                //        throw new ArgumentException($"字典项标签 '{trimmedLabel}' 已被占用。");
                //}

                // 准备待更新的实体
                entity.Value = trimmedValue;
                entity.Label = trimmedLabel;
                entity.CategoryCode = dto.CategoryCode;
                entity.Sort = dto.Sort;
                entity.CssClass = dto.CssClass?.Trim();
                entity.ListClass = dto.ListClass?.Trim();
                entity.IsDefault = dto.IsDefault?.Trim();
                entity.Status = dto.Status?.Trim() ?? entity.Status;
                entity.Remark = dto.Remark?.Trim();
                entity.UpdatedById = operatorId;
                entity.UpdatedAt = DateTime.UtcNow;

                entities.Add(entity);
            }

            // 2. 使用 UseTranAsync 执行数据库更新操作（核心事务部分）
            DbResult<int> result = await _db.Ado.UseTranAsync<int>(
                async () =>
                {
                    // 执行批量更新
                    var updatedCount = await _sysDictItemRepository.UpdateBatchAsync(entities);

                    // 校验更新数量
                    if (updatedCount != entities.Count)
                    {
                        _logger.LogWarning("用户 {UserId} 批量更新字典项失败，期望更新 {Count} 条，但实际仅成功更新 {UpdatedCount} 条。", operatorId, entities.Count, updatedCount);
                        throw new ArgumentException($"用户 {operatorId} 批量更新字典项失败，期望更新 {entities.Count} 条，但实际仅成功更新 {updatedCount} 条。");
                    }

                    // 事务成功，返回更新数量
                    return updatedCount;
                },
                // 可选：错误回调，记录日志
                ex =>
                {
                    _logger.LogWarning("用户 {UserId} 批量更新字典项失败，{ErrorMessage}。", operatorId, ex.Message);
                }
            );

            // 3. 检查事务结果
            if (!result.IsSuccess)
            {
                // 直接抛出原始异常，保留完整的 StackTrace
                throw result.ErrorException;
            }

            _logger.LogInformation("用户 {UserId} 批量更新字典项成功，更新数量 {UpdatedCount}。", operatorId, entities.Count);

            // 返回成功结果
            return result.Data;
        }

        /// <summary>
        /// 软删除指定的字典项。
        /// </summary>
        /// <param name="id">要删除的字典项 ID。</param>
        /// <param name="operatorId">操作人用户 ID。</param>
        /// <returns>返回被删除的记录 ID。</returns>
        /// <exception cref="ArgumentException">当记录不存在或已被删除时抛出。</exception>
        public async Task<long> DeleteAsync(long id, long operatorId)
        {
            if (id <= 0)
                throw new ArgumentException("无效的字典项 ID。", nameof(id));

            var entity = await _sysDictItemRepository.GetAsync(id);
            if (entity == null)
                throw new ArgumentException("字典项不存在或已被删除。");

            // 使用 UseTranAsync<long> 显式指定返回类型
            DbResult<long> result = await _db.Ado.UseTranAsync<long>(
                async () =>
                {
                    // 执行软删除
                    entity.DeletedAt = DateTime.UtcNow;
                    entity.DeletedById = operatorId;
                    var rowsAffected = await _sysDictItemRepository.DeleteAsync(entity);

                    if (rowsAffected == 0)
                    {
                        _logger.LogWarning("用户 {UserId} 删除字典项 {DictItemName} (ID: {DictItemId}) 失败，未影响任何数据库记录。", operatorId, entity.Label, entity.Id);
                        throw new InvalidOperationException($"用户 {operatorId} 删除字典项 {entity.Label} (ID: {entity.Id}) 失败，未影响任何数据库记录。");
                    }

                    return id; // 事务成功，返回 ID
                },
                // 可选：使用 errorCallBack 记录日志
                ex =>
                {
                    _logger.LogWarning("用户 {UserId} 删除字典项 {DictItemName} (ID: {DictItemId}) 失败，{ErrorMessage}。", operatorId, entity.Label, entity.Id, ex.Message);
                }
            );

            // 检查结果并处理错误
            if (!result.IsSuccess)
            {
                // 关键：直接抛出原始异常以保留完整的 StackTrace
                throw result.ErrorException;
            }

            _logger.LogInformation("用户 {UserId} 删除字典项 {DictItemName} (ID: {DictItemId}) 成功。", operatorId, entity.Label, entity.Id);

            // 返回成功结果
            return result.Data;
        }

        /// <summary>
        /// 批量软删除字典项。
        /// </summary>
        /// <param name="ids">要删除的字典项 ID 列表。</param>
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

            // 查询待删除的字典项实体
            var entities = await _sysDictItemRepository.GetByIdsAsync(idList);
            if (entities.Count != idList.Count)
                throw new ArgumentException("部分字典项不存在或已被删除。");

            // 使用 UseTranAsync 替代 BeginTranAsync
            DbResult<int> result = await _db.Ado.UseTranAsync<int>(
                async () =>
                {
                    // 批量软删除字典项
                    var deletedCount = await _sysDictItemRepository.DeleteBatchAsync(idList, operatorId);

                    // 校验删除数量
                    if (deletedCount != idList.Count)
                    {
                        _logger.LogWarning("用户 {UserId} 批量删除字典项失败，期望删除 {Count} 条，但实际仅成功删除 {DeletedCount} 条。", operatorId, idList.Count, deletedCount);
                        throw new ArgumentException($"用户 {operatorId} 批量删除字典项失败，期望删除 {idList.Count} 条，但实际仅成功删除 {deletedCount} 条。");
                    }

                    // 事务成功，返回删除数量
                    return deletedCount;
                },
                // 可选：错误回调，用于记录日志
                ex =>
                {
                    _logger.LogWarning("用户 {UserId} 批量删除字典项失败，{ErrorMessage}。", operatorId, ex.Message);
                }
            );

            // 检查事务结果
            if (!result.IsSuccess)
            {
                // 直接抛出原始异常，保留完整的 StackTrace
                throw result.ErrorException;
            }

            _logger.LogInformation("用户 {UserId} 批量删除字典项成功，删除数量 {DeletedCount}。", operatorId, entities.Count);

            // 返回成功结果
            return result.Data;
        }

        /// <summary>
        /// 根据主键获取字典项详情。
        /// </summary>
        /// <param name="id">字典项唯一标识。</param>
        /// <returns>返回匹配的 DTO；若未找到或已删除，则返回 <c>null</c>。</returns>
        public async Task<SysDictItemDto?> GetAsync(long id)
        {
            var entity = await _sysDictItemRepository.GetAsync(id);
            if (entity == null)
                return null;
            return MapToDto(entity);
        }

        /// <summary>
        /// 查询字典项列表，支持按标签模糊搜索和状态精确筛选。
        /// </summary>
        /// <param name="label">字典项标签，用于模糊搜索（可选）。</param>
        /// <param name="status">字典项状态，用于精确匹配（可选）。</param>
        /// <returns>返回匹配条件的 DTO 列表（可能为空）。</returns>
        public async Task<List<SysDictItemDto>> GetAllAsync(string? label = null, string? status = null)
        {
            var entities = await _sysDictItemRepository.GetAllAsync(label, status);
            return entities.Select(MapToDto).ToList();
        }

        /// <summary>
        /// 分页查询字典项列表。
        /// </summary>
        /// <param name="pageNumber">页码，从 1 开始。</param>
        /// <param name="pageSize">每页数量。</param>
        /// <param name="label">字典项标签，模糊搜索（可选）。</param>
        /// <param name="value">字典项实际值，模糊搜索（可选）。</param>
        /// <param name="status">字典项状态，精确匹配（可选）。</param>
        /// <param name="categoryCode">字典项分类，精确匹配（可选）。</param>
        /// <param name="orderByField">排序字段（可选）。</param>
        /// <param name="orderByType">排序方式（可选）。</param>
        /// <returns>返回分页结果对象，包含数据和分页元信息。</returns>
        public async Task<PaginationResult<SysDictItemDto>> GetPagedAsync(
            int pageNumber,
            int pageSize,
            string? label = null,
            string? value = null,
            string? status = null,
            string? categoryCode = null,
            string? orderByField = null,
            string? orderByType = null)
        {
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = 10;

            // 调用仓储层获取分页数据和总数
            var (data, totalCount) = await _sysDictItemRepository.GetPagedAsync(pageNumber, pageSize, label, value, status, categoryCode, orderByField, orderByType);
            var pagedData = data.Select(entity => MapToDto(entity)).ToList();

            // 计算总页数
            int totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            // 封装为 PaginationResult
            return new PaginationResult<SysDictItemDto>(
                items: pagedData,
                totalPages: totalPages,
                totalItems: totalCount,
                currentPage: pageNumber,
                pageSize: pageSize);
        }

        /// <summary>
        /// 根据字典项实际值获取其详情。
        /// </summary>
        /// <param name="value">字典项的唯一实际值（如 gender）。</param>
        /// <returns>返回匹配的 DTO；若未找到或已删除，则返回 <c>null</c>。</returns>
        public async Task<SysDictItemDto?> GetByValueAsync(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return null;

            var entity = await _sysDictItemRepository.GetByValueAsync(value.Trim());
            if (entity == null)
                return null;
            return MapToDto(entity);
        }

        /// <summary>
        /// 根据字典项标签获取其详情。
        /// </summary>
        /// <param name="label">字典项的显示标签（如“男”）。</param>
        /// <returns>返回匹配的 DTO；若未找到或已删除，则返回 <c>null</c>。</returns>
        public async Task<SysDictItemDto?> GetByLabelAsync(string label)
        {
            if (string.IsNullOrWhiteSpace(label))
                return null;

            var entity = await _sysDictItemRepository.GetByLabelAsync(label.Trim());
            if (entity == null)
                return null;
            return MapToDto(entity);
        }

        /// <summary>
        /// 根据字典项分类查询字典项列表。
        /// </summary>
        /// <param name="categoryCode">字典项分类。</param>
        /// <returns>返回匹配条件的 DTO 列表（可能为空）。</returns>
        public async Task<List<SysDictItemDto>> GetAllByCategoryCodeAsync(string categoryCode)
        {
            var entities = await _sysDictItemRepository.GetAllByCategoryCodeAsync(categoryCode);
            return entities.Select(MapToDto).ToList();
        }

        /// <summary>
        /// 更新字典项分类的状态。
        /// </summary>
        /// <param name="id">要更新的字典项分类 ID。</param>
        /// <param name="status">状态</param>
        /// <param name="operatorId">操作人用户 ID。</param>
        /// <returns>返回被更新的记录 ID。</returns>
        public async Task<long> ChangeStatusAsync(long id, string status, long operatorId)
        {
            if (id <= 0)
                throw new ArgumentException("无效的字典项 ID。", nameof(id));

            if (string.IsNullOrWhiteSpace(status) || (status != "0" && status != "1"))
                throw new ArgumentException("无效的状态值，状态值必须为 '0'（正常）或 '1'（停用）");

            var entity = await _sysDictItemRepository.GetAsync(id);
            if (entity == null)
                throw new ArgumentException("字典项不存在或已被删除。");

            entity.Status = status;
            entity.UpdatedById = operatorId;
            entity.UpdatedAt = DateTime.UtcNow;

            var rowsAffected = await _sysDictItemRepository.UpdateAsync(entity);
            if (rowsAffected == 0)
            {
                _logger.LogWarning("用户 {UserId} 更新字典项状态 {DictItemName} (ID: {DictItemId},Status:{Status}) 失败，未影响任何数据库记录。", operatorId, entity.Label, entity.Id, status);
                throw new InvalidOperationException($"用户 {operatorId} 更新字典项状态 {entity.Label} (ID: {entity.Id},Status: {status}) 失败，未影响任何数据库记录。");
            }

            _logger.LogInformation("用户 {UserId} 更新字典项状态 {DictItemName} (ID: {DictItemId},Status:{Status}) 成功。", operatorId, entity.Label, entity.Id, status);

            return entity.Id;
        }

        /// <summary>
        /// 将领域实体映射为应用层数据传输对象。
        /// </summary>
        /// <param name="entity">源实体对象。</param>
        /// <returns>映射后的 <see cref="SysDictItemDto"/> 实例。</returns>
        private static SysDictItemDto MapToDto(SysDictItem entity)
        {
            return new SysDictItemDto
            {
                Id = entity.Id,
                Value = entity.Value,
                Label = entity.Label,
                CategoryCode = entity.CategoryCode,
                Sort = entity.Sort,
                CssClass = entity.CssClass,
                ListClass = entity.ListClass,
                IsDefault = entity.IsDefault,
                Status = entity.Status,
                Remark = entity.Remark,
                CreatedAt = entity.CreatedAt,
                UpdatedAt = entity.UpdatedAt
            };
        }
    }
}