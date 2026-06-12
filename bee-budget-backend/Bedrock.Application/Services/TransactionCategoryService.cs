using Bedrock.Application.DataTransferObjects;
using Bedrock.Application.Interfaces;
using Bedrock.Core.Entities;
using Microsoft.Extensions.Logging;
using SqlSugar;
using System.Data;

namespace Bedrock.Application.Services
{
    /// <summary>
    /// 交易分类服务实现。
    /// <para>
    /// 支持软删除（通过 DeletedAt 字段标记），所有查询默认过滤已删除记录。
    /// 业务规则：UserId 和 Name 保证全局唯一。
    /// </para>
    /// </summary>
    public class TransactionCategoryService : ITransactionCategoryService
    {
        private readonly ILogger<TransactionCategoryService> _logger;
        private readonly ISqlSugarClient _db;
        private readonly ITransactionCategoryRepository _transactionCategoryRepository;
        private readonly ITransactionRepository _transactionRepository;

        /// <summary>
        /// 初始化 <see cref="TransactionCategoryService"/> 类的新实例。
        /// </summary>
        /// <param name="logger">日志记录器。</param>
        /// <param name="db">SqlSugar 客户端，用于事务控制。</param>
        /// <param name="transactionCategoryRepository">交易分类仓储。</param>
        /// <param name="transactionRepository">交易仓储。</param>
        public TransactionCategoryService(
            ILogger<TransactionCategoryService> logger,
            ISqlSugarClient db,
            ITransactionCategoryRepository transactionCategoryRepository,
            ITransactionRepository transactionRepository
            )
        {
            _logger = logger;
            _db = db;
            _transactionCategoryRepository = transactionCategoryRepository;
            _transactionRepository = transactionRepository;
        }

        /// <summary>
        /// 创建新的交易分类。
        /// </summary>
        /// <param name="createDto">创建数据传输对象，包含 UserId 等字段。</param>
        /// <param name="operatorId">操作人交易分类 ID，用于审计。</param>
        /// <returns>返回新创建记录的主键 ID。</returns>
        /// <exception cref="ArgumentException">当 UserId 和 Name 已存在时抛出。</exception>
        /// <exception cref="ArgumentNullException">当 createDto 为 null 时抛出。</exception>
        public async Task<long> CreateAsync(CreateTransactionCategoryDto createDto, long operatorId)
        {
            if (createDto == null)
                throw new ArgumentNullException(nameof(createDto));

            if (string.IsNullOrWhiteSpace(createDto.Name))
                throw new ArgumentException("交易分类名称不能为空。", nameof(createDto.Name));

            var trimmedName = createDto.Name.Trim();

            var existsByName = await _transactionCategoryRepository.GetByUserIdAndNameAsync(createDto.UserId, trimmedName);
            if (existsByName != null)
                throw new ArgumentException($"交易分类名称 '{trimmedName}' 已存在。");

            var entity = new TransactionCategory
            {
                UserId = createDto.UserId,
                Name = createDto.Name,
                IsPublic = createDto.IsPublic,
                Icon = createDto.Icon,
                Remark = createDto.Remark?.Trim(),
                CreatedById = operatorId,
                CreatedAt = DateTime.UtcNow
            };

            var newId = await _transactionCategoryRepository.CreateAsync(entity);

            _logger.LogInformation("用户 {UserId} 创建交易分类 {TransactionCategoryName} (ID: {TransactionCategoryId}) 成功。", operatorId, createDto.Name, newId);

            return newId;
        }

        /// <summary>
        /// 批量创建交易分类。
        /// </summary>
        /// <param name="createDtos">创建 DTO 集合。</param>
        /// <param name="operatorId">操作人用户 ID。</param>
        /// <returns>返回成功插入的记录数量；若集合为空则返回 0。</returns>
        /// <exception cref="ArgumentException">当存在重复名称时抛出。</exception>
        public async Task<int> CreateBatchAsync(IEnumerable<CreateTransactionCategoryDto> createDtos, long operatorId)
        {
            if (createDtos == null || !createDtos.Any())
                return 0;

            var dtoList = createDtos.ToList();

            foreach (var dto in dtoList)
            {
                if (string.IsNullOrWhiteSpace(dto.Name))
                    throw new ArgumentException("交易分类名称不能为空。", nameof(dto.Name));
                if (dto.UserId <= 0)
                    throw new ArgumentException("无效的交易分类 ID。", nameof(dto.UserId));
            }

            var userId = createDtos.First().UserId;
            var names = dtoList.Select(d => d.Name.Trim()).Distinct().ToList();

            var existingEntities = await _transactionCategoryRepository.GetAllAsync(null, userId);
            var existingNames = existingEntities.Select(e => e.Name).ToHashSet();

            if (names.Any(t => existingNames.Contains(t)))
                throw new ArgumentException("部分交易分类名称已存在。");

            var entities = dtoList.Select(dto => new TransactionCategory
            {
                UserId = dto.UserId,
                Name = dto.Name,
                IsPublic = dto.IsPublic,
                Icon = dto.Icon,
                Remark = dto.Remark?.Trim(),
                CreatedById = operatorId,
                CreatedAt = DateTime.UtcNow,
                UpdatedById = operatorId,
                UpdatedAt = DateTime.UtcNow
            }).ToList();

            var insertedCount = await _transactionCategoryRepository.CreateBatchAsync(entities);

            _logger.LogInformation("用户 {UserId} 批量创建交易分类成功，创建数量 {InsertedCount}。", operatorId, insertedCount);

            return insertedCount;
        }

        /// <summary>
        /// 更新指定的交易分类。
        /// </summary>
        /// <param name="updateDto">更新数据传输对象，必须包含有效 ID。</param>
        /// <param name="operatorId">操作人交易分类 ID。</param>
        /// <returns>返回受影响的记录数（通常为 1，若记录不存在则为 0）。</returns>
        /// <exception cref="ArgumentException">当记录不存在、已删除或更新后违反唯一性约束时抛出。</exception>
        /// <exception cref="ArgumentNullException">当 updateDto 为 null 时抛出。</exception>
        /// <exception cref="InvalidOperationException">当数据库更新未影响任何行时抛出（并发冲突）。</exception>
        public async Task<long> UpdateAsync(UpdateTransactionCategoryDto updateDto, long operatorId)
        {
            if (updateDto == null)
                throw new ArgumentNullException(nameof(updateDto));
            if (updateDto.Id <= 0)
                throw new ArgumentException("无效的交易分类 ID。", nameof(updateDto.Id));
            if (string.IsNullOrWhiteSpace(updateDto.Name))
                throw new ArgumentException("交易分类名称不能为空。", nameof(updateDto.Name));

            var entity = await _transactionCategoryRepository.GetAsync(updateDto.Id);
            if (entity == null)
                throw new ArgumentException("指定的交易分类不存在或已被删除。", nameof(updateDto.Id));

            var trimmedName = updateDto.Name.Trim();

            if (entity.Name != trimmedName)
            {
                var exists = await _transactionCategoryRepository.GetByUserIdAndNameAsync(updateDto.UserId, trimmedName);
                if (exists != null && exists.Id != entity.Id)
                    throw new ArgumentException($"交易分类名称 '{trimmedName}' 已被占用。");
            }

            //entity.UserId = updateDto.UserId;
            entity.Name = updateDto.Name;
            entity.IsPublic = updateDto.IsPublic;
            entity.Icon = updateDto.Icon;
            entity.Remark = updateDto.Remark?.Trim();
            entity.UpdatedById = operatorId;
            entity.UpdatedAt = DateTime.UtcNow;

            var rowsAffected = await _transactionCategoryRepository.UpdateAsync(entity);
            if (rowsAffected == 0)
            {
                _logger.LogWarning("用户 {UserId} 更新交易分类 {TransactionCategoryName} (ID: {TransactionCategoryId}) 失败，未影响任何数据库记录。", operatorId, entity.Name, entity.Id);
                throw new InvalidOperationException($"用户 {operatorId} 更新交易分类 {entity.Name} (ID: {entity.Id}) 失败，未影响任何数据库记录。");
            }

            _logger.LogInformation("用户 {UserId} 更新交易分类 {TransactionCategoryName} (ID: {TransactionCategoryId}) 成功。", operatorId, entity.Name, entity.Id);

            return entity.Id;
        }

        /// <summary>
        /// 批量更新交易分类。
        /// </summary>
        /// <param name="updateDtos">更新 DTO 集合，每个必须包含有效 ID。</param>
        /// <param name="operatorId">操作人用户 ID。</param>
        /// <returns>返回成功更新的记录数量；若集合为空则返回 0。</returns>
        /// <exception cref="ArgumentException">当任一记录不存在或违反业务规则时抛出。</exception>
        public async Task<int> UpdateBatchAsync(IEnumerable<UpdateTransactionCategoryDto> updateDtos, long operatorId)
        {
            if (updateDtos == null || !updateDtos.Any())
                return 0;

            var dtoList = updateDtos.ToList();
            var entities = new List<TransactionCategory>();

            // 1. 验证并准备实体（这部分在事务外，因为主要是查询和业务逻辑校验）
            foreach (var dto in dtoList)
            {
                if (dto.Id <= 0)
                    throw new ArgumentException($"无效的交易分类 ID: {dto.Id}");

                var entity = await _transactionCategoryRepository.GetAsync(dto.Id);
                if (entity == null)
                    throw new ArgumentException($"ID 为 {dto.Id} 的交易分类不存在或已被删除。");

                var userId = dto.UserId;
                var trimmedName = dto.Name.Trim();

                // 检查 Value 是否被占用（排除自身）
                if (entity.Name != trimmedName)
                {
                    var exists = await _transactionCategoryRepository.GetByUserIdAndNameAsync(userId, trimmedName);
                    if (exists != null && exists.Id != dto.Id)
                        throw new ArgumentException($"交易分类名称 '{trimmedName}' 已被占用。");
                }

                // 准备待更新的实体
                //entity.UserId = dto.UserId;
                entity.Name = trimmedName;
                entity.IsPublic = dto.IsPublic;
                entity.Icon = dto.Icon;
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
                    var updatedCount = await _transactionCategoryRepository.UpdateBatchAsync(entities);

                    // 校验更新数量
                    if (updatedCount != entities.Count)
                    {
                        _logger.LogWarning("用户 {UserId} 批量更新交易分类失败，期望更新 {Count} 条，但实际仅成功更新 {UpdatedCount} 条。", operatorId, entities.Count, updatedCount);
                        throw new ArgumentException($"用户 {operatorId} 批量更新交易分类失败，期望更新 {entities.Count} 条，但实际仅成功更新 {updatedCount} 条。");
                    }

                    // 事务成功，返回更新数量
                    return updatedCount;
                },
                // 可选：错误回调，记录日志
                ex =>
                {
                    _logger.LogWarning("用户 {UserId} 批量更新交易分类失败，{ErrorMessage}。", operatorId, ex.Message);
                }
            );

            // 3. 检查事务结果
            if (!result.IsSuccess)
            {
                // 直接抛出原始异常，保留完整的 StackTrace
                throw result.ErrorException;
            }

            _logger.LogInformation("用户 {UserId} 批量更新交易分类成功，更新数量 {UpdatedCount}。", operatorId, entities.Count);

            // 返回成功结果
            return result.Data;
        }

        /// <summary>
        /// 软删除指定的交易分类。
        /// </summary>
        /// <param name="id">要删除的交易分类 ID。</param>
        /// <param name="operatorId">操作人交易分类 ID。</param>
        /// <returns>返回被删除的记录 ID。</returns>
        /// <exception cref="ArgumentException">当记录不存在或已被删除时抛出。</exception>
        public async Task<long> DeleteAsync(long id, long operatorId)
        {
            if (id <= 0)
                throw new ArgumentException("无效的交易分类 ID。", nameof(id));

            var entity = await _transactionCategoryRepository.GetAsync(id);
            if (entity == null)
                throw new ArgumentException("交易分类不存在或已被删除。");

            // 使用 UseTranAsync<long> 显式指定返回类型
            DbResult<long> result = await _db.Ado.UseTranAsync<long>(
                async () =>
                {
                    var rowsAffected = 0;

                    // 删除关联数据
                    await _transactionRepository.DeleteByTransactionCategoryIdAsync(entity.Id, operatorId);

                    // 执行软删除
                    entity.DeletedAt = DateTime.UtcNow;
                    entity.DeletedById = operatorId;
                    rowsAffected = await _transactionCategoryRepository.DeleteAsync(entity);

                    if (rowsAffected == 0)
                    {
                        _logger.LogWarning("用户 {UserId} 删除交易分类 {TransactionCategoryName} (ID: {TransactionCategoryId}) 失败，未影响任何数据库记录。", operatorId, entity.Name, entity.Id);
                        throw new InvalidOperationException($"用户 {operatorId} 删除交易分类 {entity.Name} (ID: {entity.Id}) 失败，未影响任何数据库记录。");
                    }

                    return id; // 事务成功，返回 ID
                },
                // 可选：使用 errorCallBack 记录日志
                ex =>
                {
                    _logger.LogWarning("用户 {UserId} 删除交易分类 {TransactionCategoryName} (ID: {TransactionCategoryId}) 失败，{ErrorMessage}。", operatorId, entity.Name, entity.Id, ex.Message);
                }
            );

            // 检查结果并处理错误
            if (!result.IsSuccess)
            {
                // 关键：直接抛出原始异常以保留完整的 StackTrace
                throw result.ErrorException;
            }

            _logger.LogInformation("用户 {UserId} 删除交易分类 {TransactionCategoryName} (ID: {TransactionCategoryId}) 成功。", operatorId, entity.Name, entity.Id);

            // 返回成功结果
            return result.Data;
        }

        /// <summary>
        /// 批量软删除交易分类。
        /// </summary>
        /// <param name="ids">要删除的交易分类 ID 列表。</param>
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

            // 查询待删除的交易分类实体
            var entities = await _transactionCategoryRepository.GetByIdsAsync(idList);
            if (entities.Count != idList.Count)
                throw new ArgumentException("部分交易分类不存在或已被删除。");

            var userIds = entities.Select(e => e.UserId).ToList();

            // 使用 UseTranAsync 替代 BeginTranAsync
            DbResult<int> result = await _db.Ado.UseTranAsync<int>(
                async () =>
                {
                    // 批量删除关联数据
                    await _transactionRepository.DeleteByTransactionCategoryIdsAsync(idList, operatorId);

                    // 批量软删除交易分类
                    var deletedCount = await _transactionCategoryRepository.DeleteBatchAsync(idList, operatorId);

                    // 校验删除数量
                    if (deletedCount != idList.Count)
                    {
                        _logger.LogWarning("用户 {UserId} 批量删除交易分类失败，期望删除 {Count} 条，但实际仅成功删除 {DeletedCount} 条。", operatorId, idList.Count, deletedCount);
                        throw new ArgumentException($"用户 {operatorId} 批量删除交易分类失败，期望删除 {idList.Count} 条，但实际仅成功删除 {deletedCount} 条。");
                    }

                    // 事务成功，返回删除数量
                    return deletedCount;
                },
                // 可选：错误回调，用于记录日志
                ex =>
                {
                    _logger.LogWarning("用户 {UserId} 批量删除交易分类失败，{ErrorMessage}。", operatorId, ex.Message);
                }
            );

            // 检查事务结果
            if (!result.IsSuccess)
            {
                // 直接抛出原始异常，保留完整的 StackTrace
                throw result.ErrorException;
            }

            _logger.LogInformation("用户 {UserId} 批量删除交易分类成功，删除数量 {DeletedCount}。", operatorId, entities.Count);

            // 返回成功结果
            return result.Data;
        }

        /// <summary>
        /// 根据主键获取单条未删除的交易分类详情。
        /// </summary>
        /// <param name="id">交易分类唯一标识。</param>
        /// <returns>返回匹配的 DTO；若未找到或已删除，则返回 null。</returns>
        public async Task<TransactionCategoryDto?> GetAsync(long id)
        {
            var entity = await _transactionCategoryRepository.GetAsync(id);
            if (entity == null)
                return null;
            return MapToDto(entity);
        }

        /// <summary>
        /// 查询未删除的交易分类记录，支持按名称模糊搜索和UserId精确搜索。
        /// </summary>
        /// <param name="name">交易分类名称，用于模糊搜索（可选）。</param>
        /// <param name="userId">交易分类UserId，用于精确搜索（可选）。</param>
        /// <returns>返回匹配条件的 DTO 列表（可能为空）。</returns>
        public async Task<List<TransactionCategoryDto>> GetAllAsync(string? name = null, long? userId = null)
        {
            var entities = await _transactionCategoryRepository.GetAllAsync(name, userId);
            return entities.Select(MapToDto).ToList();
        }

        /// <summary>
        /// 分页查询未删除的交易分类记录，支持名称模糊搜索和UserId精确搜索。
        /// </summary>
        /// <param name="pageNumber">页码，从 1 开始。</param>
        /// <param name="pageSize">每页记录数。</param>
        /// <param name="name">交易分类名称，用于模糊搜索（可选）。</param>
        /// <param name="userId">交易分类UserId，用于精确搜索（可选）。</param>
        /// <param name="orderByField">排序字段（可选）。</param>
        /// <param name="orderByType">排序方式（可选）。</param>
        /// <returns>返回分页结果对象，包含数据和分页元信息。</returns>
        public async Task<PaginationResult<TransactionCategoryDto>> GetPagedAsync(
            int pageNumber,
            int pageSize,
            string? name = null,
            long? userId = null,
            string? orderByField = null,
            string? orderByType = null)
        {
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = 20;

            // 调用仓储层获取分页数据和总数
            var (data, totalCount) = await _transactionCategoryRepository.GetPagedAsync(pageNumber, pageSize, name, userId, orderByField, orderByType);
            var pagedData = data.Select(entity => MapToDto(entity)).ToList();

            // 计算总页数
            int totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            // 封装为 PaginationResult
            return new PaginationResult<TransactionCategoryDto>(
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
        /// <returns>映射后的 TransactionCategoryDto 实例。</returns>
        private static TransactionCategoryDto MapToDto(TransactionCategory entity)
        {
            return new TransactionCategoryDto
            {
                Id = entity.Id,
                UserId = entity.UserId,
                Name = entity.Name,
                IsPublic = entity.IsPublic,
                Icon = entity.Icon,
                Remark = entity.Remark,
                CreatedAt = entity.CreatedAt,
                UpdatedAt = entity.UpdatedAt,
            };
        }

    }
}