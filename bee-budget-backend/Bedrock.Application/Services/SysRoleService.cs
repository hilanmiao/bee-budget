using Bedrock.Application.DataTransferObjects;
using Bedrock.Application.Interfaces;
using Bedrock.Core.Entities;
using Microsoft.Extensions.Logging;
using SqlSugar;
using System.Data;
using static Dapper.SqlMapper;

namespace Bedrock.Application.Services
{
    /// <summary>
    /// 系统角色应用服务实现。
    /// <para>
    /// 支持软删除（通过 DeletedAt 字段标记），所有查询默认过滤已删除记录。
    /// 业务规则：Key 和 Name 均保证全局唯一。
    /// </para>
    /// </summary>
    public class SysRoleService : ISysRoleService
    {
        private readonly ILogger<SysRoleService> _logger;
        private readonly ISqlSugarClient _db;
        private readonly ISysRoleRepository _sysRoleRepository;
        private readonly ISysRoleMenuRepository _sysRoleMenuRepository;

        /// <summary>
        /// 初始化 <see cref="SysRoleService"/> 类的新实例。
        /// </summary>
        /// <param name="db">SqlSugar 客户端，用于事务控制。</param>
        /// <param name="sysRoleRepository">角色仓储。</param>
        /// <param name="sysRoleMenuRepository">角色菜单仓储。</param>
        public SysRoleService(
            ILogger<SysRoleService> logger,
            ISqlSugarClient db,
            ISysRoleRepository sysRoleRepository,
            ISysRoleMenuRepository sysRoleMenuRepository)
        {
            _logger = logger;
            _db = db;
            _sysRoleRepository = sysRoleRepository;
            _sysRoleMenuRepository = sysRoleMenuRepository;
        }

        /// <summary>
        /// 创建新的角色。
        /// </summary>
        /// <param name="createDto">创建数据传输对象，包含 Key、 Name 等字段。</param>
        /// <param name="operatorId">操作人角色 ID，用于审计。</param>
        /// <returns>返回新创建记录的主键 ID。</returns>
        /// <exception cref="ArgumentException">当 Key 或 Name 已存在时抛出。</exception>
        /// <exception cref="ArgumentNullException">当 createDto 为 null 时抛出。</exception>
        public async Task<long> CreateAsync(CreateSysRoleDto createDto, long operatorId)
        {
            if (createDto == null)
                throw new ArgumentNullException(nameof(createDto));

            if (string.IsNullOrWhiteSpace(createDto.Name))
                throw new ArgumentException("角色名称不能为空。", nameof(createDto.Name));
            if (string.IsNullOrWhiteSpace(createDto.Key))
                throw new ArgumentException("角色权限字符不能为空。", nameof(createDto.Key));

            var trimmedName = createDto.Name.Trim();
            var trimmedKey = createDto.Key.Trim();

            var existsByName = await _sysRoleRepository.GetByNameAsync(trimmedName);
            if (existsByName != null)
                throw new ArgumentException($"角色名称 '{trimmedName}' 已存在。");

            var existsByKey = await _sysRoleRepository.GetByKeyAsync(trimmedKey);
            if (existsByKey != null)
                throw new ArgumentException($"角色权限字符 '{trimmedKey}' 已存在。");

            var menuIds = createDto.MenuIds;

            var entity = new SysRole
            {
                Name = trimmedName,
                Key = trimmedKey,
                Sort = createDto.Sort,
                DataScope = createDto.DataScope?.Trim(),
                MenuCheckStrictly = createDto.MenuCheckStrictly,
                DeptCheckStrictly = createDto.DeptCheckStrictly,
                Status = createDto.Status?.Trim() ?? "1",
                Remark = createDto.Remark?.Trim(),
                CreatedById = operatorId,
                CreatedAt = DateTime.UtcNow,
            };

            // 使用 UseTranAsync<long> 显式指定返回类型
            DbResult<long> result = await _db.Ado.UseTranAsync<long>(
                async () =>
                {
                    var id = await _sysRoleRepository.CreateAsync(entity);

                    if (menuIds != null && menuIds.Count > 0)
                    {
                        // 创建关联数据
                        var roleMenuEntities = menuIds.Select(menuId => new SysRoleMenu { RoleId = id, MenuId = menuId }).ToList();
                        var rowsAffected = await _sysRoleMenuRepository.CreateBatchAsync(roleMenuEntities);

                        if (rowsAffected == 0)
                            throw new InvalidOperationException("创建关联数据失败，未影响任何数据库记录。");
                    }

                    return id; // 事务成功，返回 ID
                },
                // 可选：使用 errorCallBack 记录日志
                ex =>
                {
                    // _logger.LogError(ex, "删除角色 {RoleId} 失败。", id);
                    //Console.WriteLine($"[ERROR] 删除角色 {id} 失败: {ex.Message}");
                }
            );

            // 检查结果并处理错误
            if (!result.IsSuccess)
            {
                // 关键：直接抛出原始异常以保留完整的 StackTrace
                throw result.ErrorException;
            }

            _logger.LogInformation("用户 {UserId} 创建角色 {RoleName} (ID: {RoleId}) 成功。", operatorId, createDto.Name, result.Data);

            // 返回成功结果
            return result.Data;
        }

        /// <summary>
        /// 更新指定的角色。
        /// </summary>
        /// <param name="updateDto">更新数据传输对象，必须包含有效 ID。</param>
        /// <param name="operatorId">操作人角色 ID。</param>
        /// <returns>返回受影响的记录数（通常为 1，若记录不存在则为 0）。</returns>
        /// <exception cref="ArgumentException">当记录不存在、已删除或更新后违反唯一性约束时抛出。</exception>
        /// <exception cref="ArgumentNullException">当 updateDto 为 null 时抛出。</exception>
        /// <exception cref="InvalidOperationException">当数据库更新未影响任何行时抛出（并发冲突）。</exception>
        public async Task<long> UpdateAsync(UpdateSysRoleDto updateDto, long operatorId)
        {
            if (updateDto == null)
                throw new ArgumentNullException(nameof(updateDto));
            if (updateDto.Id <= 0)
                throw new ArgumentException("无效的角色 ID。", nameof(updateDto.Id));

            if (string.IsNullOrWhiteSpace(updateDto.Name))
                throw new ArgumentException("角色名称不能为空。", nameof(updateDto.Name));
            if (string.IsNullOrWhiteSpace(updateDto.Key))
                throw new ArgumentException("角色权限字符不能为空。", nameof(updateDto.Key));

            var entity = await _sysRoleRepository.GetAsync(updateDto.Id);
            if (entity == null)
                throw new ArgumentException("指定的角色不存在或已被删除。", nameof(updateDto.Id));

            var trimmedName = updateDto.Name.Trim();
            var trimmedKey = updateDto.Key.Trim();

            if (entity.Name != trimmedName)
            {
                var exists = await _sysRoleRepository.GetByNameAsync(trimmedName);
                if (exists != null && exists.Id != entity.Id)
                    throw new ArgumentException($"角色名称 '{trimmedName}' 已被占用。");
            }
            if (entity.Key != trimmedKey)
            {
                var exists = await _sysRoleRepository.GetByKeyAsync(trimmedKey);
                if (exists != null && exists.Id != entity.Id)
                    throw new ArgumentException($"角色权限字符 '{trimmedKey}' 已被占用。");
            }

            entity.Name = trimmedName;
            entity.Key = trimmedKey;
            entity.Sort = updateDto.Sort;
            entity.DataScope = updateDto.DataScope?.Trim();
            entity.MenuCheckStrictly = updateDto.MenuCheckStrictly;
            entity.DeptCheckStrictly = updateDto.DeptCheckStrictly;
            entity.Status = updateDto.Status?.Trim() ?? entity.Status;
            entity.Remark = updateDto.Remark?.Trim();
            entity.UpdatedById = operatorId;
            entity.UpdatedAt = DateTime.UtcNow;

            var menuIds = updateDto.MenuIds;

            // 使用 UseTranAsync<long> 显式指定返回类型
            DbResult<long> result = await _db.Ado.UseTranAsync<long>(
                async () =>
                {
                    var rowsAffected = 0;

                    // 删除关联数据
                    rowsAffected = await _sysRoleMenuRepository.DeleteByRoleIdAsync(entity.Id);
                    //if (rowsAffected == 0)
                    //    throw new InvalidOperationException("删除关联数据失败：未影响任何数据库记录。");

                    // 更新角色数据
                    rowsAffected = await _sysRoleRepository.UpdateAsync(entity);
                    if (rowsAffected == 0)
                        throw new InvalidOperationException("更新失败：未影响任何数据库记录。");

                    if (menuIds != null && menuIds.Count > 0)
                    {
                        // 创建关联数据
                        var roleMenuEntities = menuIds.Select(menuId => new SysRoleMenu { RoleId = entity.Id, MenuId = menuId }).ToList();
                        rowsAffected = await _sysRoleMenuRepository.CreateBatchAsync(roleMenuEntities);

                        if (rowsAffected == 0)
                            throw new InvalidOperationException("创建关联数据失败，未影响任何数据库记录。");
                    }

                    return entity.Id; // 事务成功，返回 ID
                },
                // 可选：使用 errorCallBack 记录日志
                ex =>
                {
                    // _logger.LogError(ex, "删除角色 {RoleId} 失败。", id);
                    //Console.WriteLine($"[ERROR] 删除角色 {id} 失败: {ex.Message}");
                }
            );

            // 检查结果并处理错误
            if (!result.IsSuccess)
            {
                // 关键：直接抛出原始异常以保留完整的 StackTrace
                throw result.ErrorException;
            }

            _logger.LogInformation("用户 {UserId} 更新角色 {RoleName} (ID: {RoleId}) 成功。", operatorId, entity.Name, entity.Id);

            // 返回成功结果
            return result.Data;
        }

        /// <summary>
        /// 软删除指定的角色。
        /// </summary>
        /// <param name="id">要删除的角色 ID。</param>
        /// <param name="operatorId">操作人角色 ID。</param>
        /// <returns>返回被删除的记录 ID。</returns>
        /// <exception cref="ArgumentException">当记录不存在或已被删除时抛出。</exception>
        public async Task<long> DeleteAsync(long id, long operatorId)
        {
            if (id <= 0)
                throw new ArgumentException("无效的角色 ID。", nameof(id));

            var entity = await _sysRoleRepository.GetAsync(id);
            if (entity == null)
                throw new ArgumentException("角色不存在或已被删除。");

            // 使用 UseTranAsync<long> 显式指定返回类型
            DbResult<long> result = await _db.Ado.UseTranAsync<long>(
                async () =>
                {
                    var rowsAffected = 0;

                    // 删除关联数据
                    rowsAffected = await _sysRoleMenuRepository.DeleteByRoleIdAsync(entity.Id);
                    //if (rowsAffected == 0)
                    //    throw new InvalidOperationException("删除关联数据失败：未影响任何数据库记录。");

                    // 执行软删除
                    entity.DeletedAt = DateTime.UtcNow;
                    entity.DeletedById = operatorId;
                    rowsAffected = await _sysRoleRepository.DeleteAsync(entity);

                    if (rowsAffected == 0)
                    {
                        _logger.LogWarning("用户 {UserId} 删除角色 {RoleName} (ID: {RoleId}) 失败，未影响任何数据库记录。", operatorId, entity.Name, entity.Id);
                        throw new InvalidOperationException($"用户 {operatorId} 删除角色 {entity.Name} (ID: {entity.Id}) 失败，未影响任何数据库记录。");
                    }

                    return id; // 事务成功，返回 ID
                },
                // 可选：使用 errorCallBack 记录日志
                ex =>
                {
                    _logger.LogWarning("用户 {UserId} 删除角色 {RoleName} (ID: {RoleId}) 失败，{ErrorMessage}。", operatorId, entity.Name, entity.Id, ex.Message);
                }
            );

            // 检查结果并处理错误
            if (!result.IsSuccess)
            {
                // 关键：直接抛出原始异常以保留完整的 StackTrace
                throw result.ErrorException;
            }

            _logger.LogInformation("用户 {UserId} 删除角色 {RoleName} (ID: {RoleId}) 成功。", operatorId, entity.Name, entity.Id);

            // 返回成功结果
            return result.Data;
        }

        /// <summary>
        /// 批量软删除角色。
        /// </summary>
        /// <param name="ids">要删除的角色 ID 列表。</param>
        /// <param name="operatorId">操作人角色 ID。</param>
        /// <returns>返回成功标记为删除的记录数量；若 ID 列表为空则返回 0。</returns>
        /// <exception cref="ArgumentException">当部分 ID 不存在或已被删除时可能抛出。</exception>
        public async Task<int> DeleteBatchAsync(IEnumerable<long> ids, long operatorId)
        {
            if (ids == null || !ids.Any())
                return 0;

            var idList = ids.Distinct().Where(id => id > 0).ToList();
            if (!idList.Any())
                return 0;

            // 查询待删除的角色实体
            var entities = await _sysRoleRepository.GetByIdsAsync(idList);
            if (entities.Count != idList.Count)
                throw new ArgumentException("部分角色不存在或已被删除。");

            // 使用 UseTranAsync<int> 显式指定返回类型
            DbResult<int> result = await _db.Ado.UseTranAsync<int>(
                async () =>
                {
                    // 删除关联数据
                    var rowsAffected = 0;
                    rowsAffected = await _sysRoleMenuRepository.DeleteByRoleIdsAsync(idList);
                    //if (rowsAffected == 0)
                    //    throw new InvalidOperationException("删除关联数据失败：未影响任何数据库记录。");

                    // 批量软删除角色
                    var deletedCount = await _sysRoleRepository.DeleteBatchAsync(idList, operatorId);

                    // 校验删除数量
                    if (deletedCount != idList.Count)
                    {
                        _logger.LogWarning("用户 {UserId} 批量删除角色失败，期望删除 {Count} 条，但实际仅成功删除 {DeletedCount} 条。", operatorId, idList.Count, deletedCount);
                        throw new ArgumentException($"用户 {operatorId} 批量删除角色失败，期望删除 {idList.Count} 条，但实际仅成功删除 {deletedCount} 条。");
                    }

                    // 事务成功，返回删除数量
                    return deletedCount;
                },
                // 可选：错误回调，用于记录日志
                ex =>
                {
                    _logger.LogWarning("用户 {UserId} 批量删除角色失败，{ErrorMessage}。", operatorId, ex.Message);
                }
            );

            // 检查事务结果
            if (!result.IsSuccess)
            {
                // 直接抛出原始异常，保留完整的 StackTrace
                throw result.ErrorException;
            }

            _logger.LogInformation("用户 {UserId} 批量删除角色成功，删除数量 {DeletedCount}。", operatorId, entities.Count);

            // 返回成功结果
            return result.Data;
        }

        /// <summary>
        /// 根据主键获取单条未删除的角色详情。
        /// </summary>
        /// <param name="id">角色唯一标识。</param>
        /// <returns>返回匹配的 DTO；若未找到或已删除，则返回 null。</returns>
        public async Task<SysRoleDto?> GetAsync(long id)
        {
            var entity = await _sysRoleRepository.GetAsync(id);
            if (entity == null)
                return null;
            return MapToDto(entity);
        }

        /// <summary>
        /// 查询未删除的角色列表，支持按角色名称模糊搜索和状态精确筛选。
        /// </summary>
        /// <param name="name">角色名称，用于模糊搜索（可选）。</param>
        /// <param name="status">角色状态，用于精确匹配（可选）。</param>
        /// <returns>返回匹配条件的 DTO 列表（可能为空）。</returns>
        public async Task<List<SysRoleDto>> GetAllAsync(string? name = null, string? status = null)
        {
            var entities = await _sysRoleRepository.GetAllAsync(name, status);
            return entities.Select(MapToDto).ToList();
        }

        /// <summary>
        /// 分页查询未删除的角色列表，支持名称、权限字符模糊搜索和状态精确筛选。
        /// </summary>
        /// <param name="pageNumber">页码，从 1 开始。</param>
        /// <param name="pageSize">每页数量。</param>
        /// <param name="name">角色名称，用于模糊搜索（可选）。</param>
        /// <param name="key">角色权限字符，用于模糊搜索（可选）。</param>
        /// <param name="status">角色状态，用于精确匹配（可选）。</param>
        /// <param name="orderByField">排序字段（可选）。</param>
        /// <param name="orderByType">排序方式（可选）。</param>
        /// <returns>返回分页结果对象，包含数据和分页元信息。</returns>
        public async Task<PaginationResult<SysRoleDto>> GetPagedAsync(
            int pageNumber,
            int pageSize,
            string? name = null,
            string? key = null,
            string? status = null,
            string? orderByField = null,
            string? orderByType = null)
        {
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = 10;

            // 调用仓储层获取分页数据和总数
            var (data, totalCount) = await _sysRoleRepository.GetPagedAsync(pageNumber, pageSize, name, key, status, orderByField, orderByType);
            var pagedData = data.Select(entity => MapToDto(entity)).ToList();

            // 计算总页数
            int totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            // 封装为 PaginationResult
            return new PaginationResult<SysRoleDto>(
                items: pagedData,
                totalPages: totalPages,
                totalItems: totalCount,
                currentPage: pageNumber,
                pageSize: pageSize);
        }

        /// <summary>
        /// 根据角色权限字符获取唯一未删除的角色详情。
        /// </summary>
        /// <param name="key">角色的唯一权限字符（如 admin），用于精确匹配。</param>
        /// <returns>返回匹配的 DTO；若未找到或已删除，则返回 null。</returns>
        public async Task<SysRoleDto?> GetByKeyAsync(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
                return null;

            var entity = await _sysRoleRepository.GetByNameAsync(key.Trim());
            if (entity == null)
                return null;
            return MapToDto(entity);
        }

        /// <summary>
        /// 根据角色名称获取唯一未删除的角色详情。
        /// </summary>
        /// <param name="name">角色的名称（如“admin”），用于精确匹配。</param>
        /// <returns>返回匹配的 DTO；若未找到或已删除，则返回 null。</returns>
        public async Task<SysRoleDto?> GetByNameAsync(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return null;

            var entity = await _sysRoleRepository.GetByNameAsync(name.Trim());
            if (entity == null)
                return null;
            return MapToDto(entity);
        }

        /// <summary>
        /// 更新角色的状态。
        /// </summary>
        /// <param name="id">要更新的角色 ID。</param>
        /// <param name="status">角色状态</param>
        /// <param name="operatorId">操作人角色 ID。</param>
        /// <returns>返回被更新的记录 ID。</returns>
        public async Task<long> ChangeStatusAsync(long id, string status, long operatorId)
        {
            if (id <= 0)
                throw new ArgumentException("无效的角色 ID。", nameof(id));

            if (string.IsNullOrWhiteSpace(status) || (status != "0" && status != "1"))
                throw new ArgumentException("无效的状态值，状态值必须为 '0'（正常）或 '1'（停用）");

            var entity = await _sysRoleRepository.GetAsync(id);
            if (entity == null)
                throw new ArgumentException("角色不存在或已被删除。");

            entity.Status = status;
            entity.UpdatedById = operatorId;
            entity.UpdatedAt = DateTime.UtcNow;

            var rowsAffected = await _sysRoleRepository.UpdateAsync(entity);
            if (rowsAffected == 0)
            {
                _logger.LogWarning("用户 {UserId} 更新角色状态 {RoleName} (ID: {RoleId},Status:{Status}) 失败，未影响任何数据库记录。", operatorId, entity.Name, entity.Id, status);
                throw new InvalidOperationException($"用户 {operatorId} 更新角色状态 {entity.Name} (ID: {entity.Id},Status: {status}) 失败，未影响任何数据库记录。");
            }

            _logger.LogInformation("用户 {UserId} 更新角色状态 {RoleName} (ID: {RoleId},Status:{Status}) 成功。", operatorId, entity.Name, entity.Id, status);

            return entity.Id;
        }

        /// <summary>
        /// 将领域实体映射为应用层数据传输对象。
        /// </summary>
        /// <param name="entity">源实体对象。</param>
        /// <returns>映射后的 SysRoleDto 实例。</returns>
        private static SysRoleDto MapToDto(SysRole entity)
        {
            return new SysRoleDto
            {
                Id = entity.Id,
                Name = entity.Name,
                Key = entity.Key,
                Sort = entity.Sort,
                DataScope = entity.DataScope,
                MenuCheckStrictly = entity.MenuCheckStrictly,
                DeptCheckStrictly = entity.DeptCheckStrictly,
                Status = entity.Status,
                Remark = entity.Remark,
                CreatedAt = entity.CreatedAt,
            };
        }
    
    }
}