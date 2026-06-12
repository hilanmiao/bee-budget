using Bedrock.Application.DataTransferObjects;
using Bedrock.Application.Interfaces;
using Bedrock.Core.Entities;
using Microsoft.Extensions.Logging;
using SqlSugar;
using System.Data;

namespace Bedrock.Application.Services
{
    /// <summary>
    /// 产品服务实现。
    /// <para>
    /// 负责产品的创建、更新、删除、查询等业务逻辑，支持软删除语义。
    /// 所有查询默认过滤已删除记录。
    /// 业务规则：Name 保证全局唯一。
    /// </para>
    /// </summary>
    public class ProductionService : IProductionService
    {
        private readonly ILogger<ProductionService> _logger;
        private readonly ISqlSugarClient _db;
        private readonly IProductionRepository _productionRepository;

        /// <summary>
        /// 初始化 <see cref="ProductionService"/> 类的新实例。
        /// </summary>
        /// <param name="db">SqlSugar 客户端，用于事务控制。</param>
        /// <param name="productionRepository">产品仓储。</param>
        public ProductionService(
            ILogger<ProductionService> logger,
            ISqlSugarClient db,
            IProductionRepository productionRepository
            )
        {
            _logger = logger;
            _db = db;
            _productionRepository = productionRepository;
        }

        /// <summary>
        /// 创建新的产品。
        /// </summary>
        /// <param name="createDto">创建数据传输对象，包含 Model、Name 等字段。</param>
        /// <param name="operatorId">操作人用户 ID，用于审计。</param>
        /// <returns>返回新创建记录的主键 ID。</returns>
        /// <exception cref="ArgumentException">当 Model 或 Name 已存在时抛出。</exception>
        /// <exception cref="ArgumentNullException">当 createDto 为 null 时抛出。</exception>
        public async Task<long> CreateAsync(CreateProductionDto createDto, long operatorId)
        {
            if (createDto == null)
                throw new ArgumentNullException(nameof(createDto));

            if (string.IsNullOrWhiteSpace(createDto.Name))
                throw new ArgumentException("产品名称不能为空。", nameof(createDto.Name));

            var trimmedName = createDto.Name.Trim();

            var existsByName = await _productionRepository.GetByNameAsync(trimmedName);
            if (existsByName != null)
                throw new ArgumentException($"产品名称 '{trimmedName}' 已存在。");

            var entity = new Production
            {
                Name = trimmedName,
                Model = createDto.Model.Trim(),
                Characteristic = createDto.Characteristic.Trim(),
                Cover = createDto.Cover.Trim(),
                ModelThreeDimensional = createDto.ModelThreeDimensional?.Trim(),
                Album = createDto.Album?.Trim(),
                Files = createDto.Files?.Trim(),
                Description = createDto.Description.Trim(),
                Params = createDto.Params?.Trim(),
                Size = createDto.Size?.Trim(),
                Install = createDto.Install?.Trim(),
                Choose = createDto.Choose?.Trim(),
                Price = createDto.Price,
                Series = createDto.Series.Trim(),
                Status = createDto.Status?.Trim() ?? "1",
                Remark = createDto.Remark?.Trim(),
                CreatedById = operatorId,
                CreatedAt = DateTime.UtcNow,
                UpdatedById = operatorId,
                UpdatedAt = DateTime.UtcNow
            };

            var newId = await _productionRepository.CreateAsync(entity);

            _logger.LogInformation("用户 {UserId} 创建产品 {ProductName} (ID: {ProductId}) 成功。", operatorId, createDto.Name, newId);

            return newId;
        }

        /// <summary>
        /// 批量创建产品。
        /// </summary>
        /// <param name="createDtos">创建 DTO 集合。</param>
        /// <param name="operatorId">操作人用户 ID。</param>
        /// <returns>返回成功插入的记录数量；若集合为空，则返回 0。</returns>
        /// <exception cref="ArgumentException">当存在重复型号或名称时抛出。</exception>
        public async Task<int> CreateBatchAsync(IEnumerable<CreateProductionDto> createDtos, long operatorId)
        {
            if (createDtos == null || !createDtos.Any())
                return 0;

            var dtoList = createDtos.ToList();

            foreach (var dto in dtoList)
            {
                if (string.IsNullOrWhiteSpace(dto.Name))
                {
                    throw new ArgumentException("产品名称不能为空。");
                }
            }

            var names = dtoList.Select(d => d.Name.Trim()).Distinct().ToList();

            var existingEntities = await _productionRepository.GetAllAsync(null, null);
            var existingNames = existingEntities.Select(e => e.Name).ToHashSet();

            if (names.Any(n => existingNames.Contains(n)))
            {
                throw new ArgumentException("部分产品名称已存在。");
            }

            var entities = dtoList.Select(dto => new Production
            {
                Name = dto.Name.Trim(),
                Model = dto.Model.Trim(),
                Characteristic = dto.Characteristic.Trim(),
                Cover = dto.Cover.Trim(),
                ModelThreeDimensional = dto.ModelThreeDimensional?.Trim(),
                Album = dto.Album?.Trim(),
                Files = dto.Files?.Trim(),
                Description = dto.Description.Trim(),
                Params = dto.Params?.Trim(),
                Size = dto.Size?.Trim(),
                Install = dto.Install?.Trim(),
                Choose = dto.Choose?.Trim(),
                Price = dto.Price,
                Series = dto.Series.Trim(),
                Status = dto.Status?.Trim() ?? "1",
                Remark = dto.Remark?.Trim(),
                CreatedById = operatorId,
                CreatedAt = DateTime.UtcNow,
                UpdatedById = operatorId,
                UpdatedAt = DateTime.UtcNow
            }).ToList();

            var insertedCount = await _productionRepository.CreateBatchAsync(entities);

            _logger.LogInformation("用户 {UserId} 批量创建产品成功，创建数量 {InsertedCount}。", operatorId, insertedCount);

            return insertedCount;
        }

        /// <summary>
        /// 更新指定的产品。
        /// </summary>
        /// <param name="updateDto">更新数据传输对象，必须包含有效 ID。</param>
        /// <param name="operatorId">操作人用户 ID。</param>
        /// <returns>返回被更新的记录 ID。</returns>
        /// <exception cref="ArgumentException">当记录不存在、已删除或更新后违反唯一性约束时抛出。</exception>
        /// <exception cref="ArgumentNullException">当 updateDto 为 null 时抛出。</exception>
        /// <exception cref="InvalidOperationException">当数据库更新未影响任何行时抛出（并发冲突）。</exception>
        public async Task<long> UpdateAsync(UpdateProductionDto updateDto, long operatorId)
        {
            if (updateDto == null)
                throw new ArgumentNullException(nameof(updateDto));
            if (updateDto.Id <= 0)
                throw new ArgumentException("无效的产品 ID。", nameof(updateDto.Id));

            if (string.IsNullOrWhiteSpace(updateDto.Name))
                throw new ArgumentException("产品名称不能为空。", nameof(updateDto.Name));

            var entity = await _productionRepository.GetAsync(updateDto.Id);
            if (entity == null)
                throw new ArgumentException("指定的产品不存在或已被删除。", nameof(updateDto.Id));

            var trimmedName = updateDto.Name.Trim();

            if (entity.Name != trimmedName)
            {
                var exists = await _productionRepository.GetByNameAsync(trimmedName);
                if (exists != null && exists.Id != entity.Id)
                    throw new ArgumentException($"产品名称 '{trimmedName}' 已被占用。");
            }

            entity.Name = trimmedName;
            entity.Model = updateDto.Model.Trim();
            entity.Characteristic = updateDto.Characteristic.Trim();
            entity.Cover = updateDto.Cover.Trim();
            entity.ModelThreeDimensional = updateDto.ModelThreeDimensional?.Trim();
            entity.Album = updateDto.Album?.Trim();
            entity.Files = updateDto.Files?.Trim();
            entity.Description = updateDto.Description.Trim();
            entity.Params = updateDto.Params?.Trim();
            entity.Size = updateDto.Size?.Trim();
            entity.Install = updateDto.Install?.Trim();
            entity.Choose = updateDto.Choose?.Trim();
            entity.Price = updateDto.Price;
            entity.Series = updateDto.Series.Trim();
            entity.Status = updateDto.Status?.Trim() ?? entity.Status;
            entity.Remark = updateDto.Remark?.Trim();
            entity.UpdatedById = operatorId;
            entity.UpdatedAt = DateTime.UtcNow;

            var rowsAffected = await _productionRepository.UpdateAsync(entity);
            if (rowsAffected == 0)
            {
                _logger.LogWarning("用户 {UserId} 更新产品 {ProductName} (ID: {ProductId}) 失败，未影响任何数据库记录。", operatorId, entity.Name, entity.Id);
                throw new InvalidOperationException($"用户 {operatorId} 更新产品 {entity.Name} (ID: {entity.Id}) 失败，未影响任何数据库记录。");
            }

            _logger.LogInformation("用户 {UserId} 更新产品 {ProductName} (ID: {ProductId}) 成功。", operatorId, entity.Name, entity.Id);

            return entity.Id;
        }

        /// <summary>
        /// 批量更新产品。
        /// </summary>
        /// <param name="updateDtos">更新 DTO 集合，每个必须包含有效 ID。</param>
        /// <param name="operatorId">操作人用户 ID。</param>
        /// <returns>返回所有更新操作受影响的总记录数；若集合为空，则返回 0。</returns>
        /// <exception cref="ArgumentException">当任一记录不存在或违反业务规则时抛出。</exception>
        public async Task<int> UpdateBatchAsync(IEnumerable<UpdateProductionDto> updateDtos, long operatorId)
        {
            if (updateDtos == null || !updateDtos.Any())
                return 0;

            var dtoList = updateDtos.ToList();
            var entities = new List<Production>();

            // 1. 验证并准备实体（这部分在事务外，因为主要是查询和业务逻辑校验）
            foreach (var dto in dtoList)
            {
                if (dto.Id <= 0)
                    throw new ArgumentException($"无效的 ID: {dto.Id}");

                var entity = await _productionRepository.GetAsync(dto.Id);
                if (entity == null)
                    throw new ArgumentException($"ID 为 {dto.Id} 的产品不存在或已被删除。");

                var trimmedName = dto.Name.Trim();

                // 检查 Name 是否被占用（排除自身）
                if (entity.Name != trimmedName)
                {
                    var exists = await _productionRepository.GetByNameAsync(trimmedName);
                    if (exists != null && exists.Id != dto.Id)
                        throw new ArgumentException($"产品名称 '{trimmedName}' 已被占用。");
                }

                // 准备待更新的实体
                entity.Name = trimmedName;
                entity.Model = dto.Model.Trim();
                entity.Characteristic = dto.Characteristic.Trim();
                entity.Cover = dto.Cover.Trim();
                entity.ModelThreeDimensional = dto.ModelThreeDimensional?.Trim();
                entity.Album = dto.Album?.Trim();
                entity.Files = dto.Files?.Trim();
                entity.Description = dto.Description.Trim();
                entity.Params = dto.Params?.Trim();
                entity.Size = dto.Size?.Trim();
                entity.Install = dto.Install?.Trim();
                entity.Choose = dto.Choose?.Trim();
                entity.Price = dto.Price;
                entity.Series = dto.Series.Trim();
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
                    var updatedCount = await _productionRepository.UpdateBatchAsync(entities);

                    // 校验更新数量
                    if (updatedCount != entities.Count)
                    {
                        _logger.LogWarning("用户 {UserId} 批量更新产品失败，期望更新 {Count} 条，但实际仅成功更新 {UpdatedCount} 条。", operatorId, entities.Count, updatedCount);
                        throw new ArgumentException($"用户 {operatorId} 批量更新产品失败，期望更新 {entities.Count} 条，但实际仅成功更新 {updatedCount} 条。");
                    }

                    // 事务成功，返回更新数量
                    return updatedCount;
                },
                // 可选：错误回调，记录日志
                ex =>
                {
                    _logger.LogWarning("用户 {UserId} 批量更新产品失败，{ErrorMessage}。", operatorId, ex.Message);
                }
            );

            // 3. 检查事务结果
            if (!result.IsSuccess)
            {
                // 直接抛出原始异常，保留完整的 StackTrace
                throw result.ErrorException;
            }

            _logger.LogInformation("用户 {UserId} 批量更新产品成功，更新数量 {UpdatedCount}。", operatorId, entities.Count);

            // 返回成功结果
            return result.Data;
        }

        /// <summary>
        /// 软删除指定的产品。
        /// </summary>
        /// <param name="id">要删除的产品 ID。</param>
        /// <param name="operatorId">操作人用户 ID。</param>
        /// <returns>返回被删除的记录 ID。</returns>
        /// <exception cref="ArgumentException">当记录不存在或已被删除时抛出。</exception>
        public async Task<long> DeleteAsync(long id, long operatorId)
        {
            if (id <= 0)
                throw new ArgumentException("无效的产品 ID。", nameof(id));

            var entity = await _productionRepository.GetAsync(id);
            if (entity == null)
                throw new ArgumentException("产品不存在或已被删除。");

            // 使用 UseTranAsync<long> 显式指定返回型号
            DbResult<long> result = await _db.Ado.UseTranAsync<long>(
                async () =>
                {
                    // 执行软删除
                    entity.DeletedAt = DateTime.UtcNow;
                    entity.DeletedById = operatorId;
                    var rowsAffected = await _productionRepository.DeleteAsync(entity);

                    if (rowsAffected == 0)
                    {
                        _logger.LogWarning("用户 {UserId} 删除产品 {ProductName} (ID: {ProductId}) 失败，未影响任何数据库记录。", operatorId, entity.Name, entity.Id);
                        throw new InvalidOperationException($"用户 {operatorId} 删除产品 {entity.Name} (ID: {entity.Id}) 失败，未影响任何数据库记录。");
                    }

                    return id; // 事务成功，返回 ID
                },
                // 可选：使用 errorCallBack 记录日志
                ex =>
                {
                    _logger.LogWarning("用户 {UserId} 删除产品 {ProductName} (ID: {ProductId}) 失败，{ErrorMessage}。", operatorId, entity.Name, entity.Id, ex.Message);
                }
            );

            // 检查结果并处理错误
            if (!result.IsSuccess)
            {
                // 关键：直接抛出原始异常以保留完整的 StackTrace
                throw result.ErrorException;
            }

            _logger.LogInformation("用户 {UserId} 删除产品 {ProductName} (ID: {ProductId}) 成功。", operatorId, entity.Name, entity.Id);

            // 返回成功结果
            return result.Data;
        }

        /// <summary>
        /// 批量软删除产品。
        /// </summary>
        /// <param name="ids">要删除的产品 ID 列表。</param>
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

            // 查询待删除的产品实体
            var entities = await _productionRepository.GetByIdsAsync(idList);
            if (entities.Count != idList.Count)
                throw new ArgumentException("部分产品不存在或已被删除。");

            var models = entities.Select(e => e.Model).ToList();

            // 使用 UseTranAsync 替代 BeginTranAsync
            DbResult<int> result = await _db.Ado.UseTranAsync<int>(
                async () =>
                {
                    // 批量软删除产品
                    var deletedCount = await _productionRepository.DeleteBatchAsync(idList, operatorId);

                    // 校验删除数量
                    if (deletedCount != idList.Count)
                    {
                        _logger.LogWarning("用户 {UserId} 批量删除产品失败，期望删除 {Count} 条，但实际仅成功删除 {DeletedCount} 条。", operatorId, idList.Count, deletedCount);
                        throw new ArgumentException($"用户 {operatorId} 批量删除产品失败，期望删除 {idList.Count} 条，但实际仅成功删除 {deletedCount} 条。");
                    }

                    // 事务成功，返回删除数量
                    return deletedCount;
                },
                // 可选：错误回调，用于记录日志
                ex =>
                {
                    _logger.LogWarning("用户 {UserId} 批量删除产品失败，{ErrorMessage}。", operatorId, ex.Message);
                }
            );

            // 检查事务结果
            if (!result.IsSuccess)
            {
                // 直接抛出原始异常，保留完整的 StackTrace
                throw result.ErrorException;
            }

            _logger.LogInformation("用户 {UserId} 批量删除产品成功，删除数量 {DeletedCount}。", operatorId, entities.Count);

            // 返回成功结果
            return result.Data;
        }

        /// <summary>
        /// 根据主键获取单条未删除的产品详情。
        /// </summary>
        /// <param name="id">产品唯一标识。</param>
        /// <returns>返回匹配的 DTO；若未找到或已删除，则返回 null。</returns>
        public async Task<ProductionDto?> GetAsync(long id)
        {
            var entity = await _productionRepository.GetAsync(id);
            if (entity == null)
                return null;
            return MapToDto(entity);
        }

        /// <summary>
        /// 查询未删除的产品列表，支持按名称模糊搜索和状态精确筛选。
        /// </summary>
        /// <param name="name">产品名称，用于模糊搜索（可选）。</param>
        /// <param name="status">产品状态，用于精确匹配（可选）。</param>
        /// <returns>返回匹配条件的 DTO 列表（可能为空）。</returns>
        public async Task<List<ProductionDto>> GetAllAsync(string? name = null, string? status = null)
        {
            var entities = await _productionRepository.GetAllAsync(name, status);
            return entities.Select(MapToDto).ToList();
        }

        /// <summary>
        /// 分页查询未删除的产品列表，支持名称、型号模糊搜索和状态精确筛选。
        /// </summary>
        /// <param name="pageNumber">页码，从 1 开始。</param>
        /// <param name="pageSize">每页数量。</param>
        /// <param name="name">产品名称，用于模糊搜索（可选）。</param>
        /// <param name="model">产品型号，用于模糊搜索（可选）。</param>
        /// <param name="status">产品状态，用于精确匹配（可选）。</param>
        /// <param name="orderByField">排序字段（可选）。</param>
        /// <param name="orderByType">排序方式（可选）。</param>
        /// <returns>返回分页结果对象，包含数据和分页元信息。</returns>
        public async Task<PaginationResult<ProductionDto>> GetPagedAsync(
            int pageNumber,
            int pageSize,
            string? name = null,
            string? model = null,
            string? status = null,
            string? orderByField = null,
            string? orderByType = null)
        {
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = 10;

            // 调用仓储层获取分页数据和总数
            var (data, totalCount) = await _productionRepository.GetPagedAsync(pageNumber, pageSize, name, model, status, orderByField, orderByType);
            var pagedData = data.Select(entity => MapToDto(entity)).ToList();

            // 计算总页数
            int totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            // 封装为 PaginationResult
            return new PaginationResult<ProductionDto>(
                items: pagedData,
                totalPages: totalPages,
                totalItems: totalCount,
                currentPage: pageNumber,
                pageSize: pageSize);
        }

        /// <summary>
        /// 根据产品名称获取唯一未删除的产品详情。
        /// </summary>
        /// <param name="name">字典的显示名称（如“性别”），用于精确匹配。</param>
        /// <returns>返回匹配的 DTO；若未找到或已删除，则返回 null。</returns>
        public async Task<ProductionDto?> GetByNameAsync(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return null;

            var entity = await _productionRepository.GetByNameAsync(name.Trim());
            if (entity == null)
                return null;
            return MapToDto(entity);
        }

        /// <summary>
        /// 更新产品的状态。
        /// </summary>
        /// <param name="id">要更新的产品 ID。</param>
        /// <param name="status">状态</param>
        /// <param name="operatorId">操作人用户 ID。</param>
        /// <returns>返回被更新的记录 ID。</returns>
        public async Task<long> ChangeStatusAsync(long id, string status, long operatorId)
        {
            if (id <= 0)
                throw new ArgumentException("无效的产品 ID。", nameof(id));

            if (string.IsNullOrWhiteSpace(status) || (status != "0" && status != "1"))
                throw new ArgumentException("无效的状态值，状态值必须为 '0'（正常）或 '1'（停用）");

            var entity = await _productionRepository.GetAsync(id);
            if (entity == null)
                throw new ArgumentException("产品不存在或已被删除。");

            entity.Status = status;
            entity.UpdatedById = operatorId;
            entity.UpdatedAt = DateTime.UtcNow;

            var rowsAffected = await _productionRepository.UpdateAsync(entity);
            if (rowsAffected == 0)
            {
                _logger.LogWarning("用户 {UserId} 更新产品状态 {ProductName} (ID: {ProductId},Status:{Status}) 失败，未影响任何数据库记录。", operatorId, entity.Name, entity.Id, status);
                throw new InvalidOperationException($"用户 {operatorId} 更新产品状态 {entity.Name} (ID: {entity.Id},Status: {status}) 失败，未影响任何数据库记录。");
            }

            _logger.LogInformation("用户 {UserId} 更新产品状态 {ProductName} (ID: {ProductId},Status:{Status}) 成功。", operatorId, entity.Name, entity.Id, status);

            return entity.Id;
        }

        /// <summary>
        /// 将领域实体映射为应用层数据传输对象。
        /// </summary>
        /// <param name="entity">源实体对象。</param>
        /// <returns>映射后的 ProductionDto 实例。</returns>
        private static ProductionDto MapToDto(Production entity)
        {
            return new ProductionDto
            {
                Id = entity.Id,
                Name = entity.Name,
                Series = entity.Series,
                Model = entity.Model,
                Characteristic = entity.Characteristic,
                Cover = entity.Cover,
                ModelThreeDimensional = entity.ModelThreeDimensional,
                Album = entity.Album,
                Files = entity.Files,
                Description = entity.Description,
                Params = entity.Params,
                Size = entity.Size,
                Install = entity.Install,
                Choose = entity.Choose,
                Price = entity.Price,
                Status = entity.Status,
                Remark = entity.Remark,
                CreatedAt = entity.CreatedAt,
                UpdatedAt = entity.UpdatedAt
            };
        }
    }
}