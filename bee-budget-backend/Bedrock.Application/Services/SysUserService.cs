using Bedrock.Application.DataTransferObjects;
using Bedrock.Application.Interfaces;
using Bedrock.Core.Entities;
using Microsoft.Extensions.Logging;
using SqlSugar;
using StackExchange.Redis;
using System.Data;

namespace Bedrock.Application.Services
{
    /// <summary>
    /// 系统用户应用服务实现。
    /// <para>
    /// 支持软删除（通过 DeletedAt 字段标记），所有查询默认过滤已删除记录。
    /// 业务规则：UserName 保证全局唯一。
    /// </para>
    /// </summary>
    public class SysUserService : ISysUserService
    {
        private readonly ILogger<SysUserService> _logger;
        private readonly ISqlSugarClient _db;
        private readonly ISysUserRepository _sysUserRepository;
        private readonly ISysRoleRepository _sysRoleRepository;
        private readonly ISysUserRoleRepository _sysUserRoleRepository;
        private readonly IPasswordHasher _passwordHasher; // 密码哈希器实例

        /// <summary>
        /// 初始化 <see cref="SysUserService"/> 类的新实例。
        /// </summary>
        /// <param name="db">SqlSugar 客户端，用于事务控制。</param>
        /// <param name="sysUserRepository">用户仓储。</param>
        /// <param name="sysRoleRepository">角色仓储。</param>
        /// <param name="sysUserRoleRepository">用户角色仓储。</param>
        public SysUserService(
            ILogger<SysUserService> logger,
            ISqlSugarClient db,
            ISysUserRepository sysUserRepository,
            ISysRoleRepository sysRoleRepository,
            ISysUserRoleRepository sysUserRoleRepository,
            IPasswordHasher passwordHasher)
        {
            _logger = logger;
            _db = db;
            _sysUserRepository = sysUserRepository;
            _sysRoleRepository = sysRoleRepository;
            _sysUserRoleRepository = sysUserRoleRepository;
            _passwordHasher = passwordHasher;
        }

        /// <summary>
        /// 创建新的用户。
        /// </summary>
        /// <param name="createDto">创建数据传输对象，包含 UserName 等字段。</param>
        /// <param name="operatorId">操作人用户 ID，用于审计。</param>
        /// <returns>返回新创建记录的主键 ID。</returns>
        /// <exception cref="ArgumentException">当 UserName 已存在时抛出。</exception>
        /// <exception cref="ArgumentNullException">当 createDto 为 null 时抛出。</exception>
        public async Task<long> CreateAsync(CreateSysUserDto createDto, long operatorId)
        {
            if (createDto == null)
                throw new ArgumentNullException(nameof(createDto));

            if (string.IsNullOrWhiteSpace(createDto.UserName))
                throw new ArgumentException("用户名称不能为空。", nameof(createDto.UserName));
            if (string.IsNullOrWhiteSpace(createDto.Password))
                throw new ArgumentException("用户密码不能为空。", nameof(createDto.Password));

            var trimmedName = createDto.UserName.Trim();

            var existsByName = await _sysUserRepository.GetByUserNameAsync(trimmedName);
            if (existsByName != null)
                throw new ArgumentException($"用户名称 '{trimmedName}' 已存在。");

            var hashedPassword = _passwordHasher.HashPassword(createDto.Password!); // 使用密码哈希器哈希密码
            var roleIds = createDto.RoleIds;

            var entity = new SysUser
            {
                UserName = trimmedName,
                Password = hashedPassword,
                NickName = createDto.NickName?.Trim(),
                PhoneNumber = createDto.PhoneNumber?.Trim(),
                Email = createDto.Email?.Trim(),
                Sex = createDto.Sex?.Trim(),
                Avatar = createDto.Avatar?.Trim(),
                Status = createDto.Status?.Trim() ?? "1",
                Remark = createDto.Remark?.Trim(),
                CreatedById = operatorId,
                CreatedAt = DateTime.UtcNow,
            };

            // 使用 UseTranAsync<long> 显式指定返回类型
            DbResult<long> result = await _db.Ado.UseTranAsync<long>(
                async () =>
                {
                    var id = await _sysUserRepository.CreateAsync(entity);

                    if (roleIds != null && roleIds.Count > 0)
                    {
                        // 创建关联数据
                        var userRoleEntities = roleIds.Select(roleId => new SysUserRole { UserId = id, RoleId = roleId }).ToList();
                        var rowsAffected = await _sysUserRoleRepository.CreateBatchAsync(userRoleEntities);

                        if (rowsAffected == 0)
                            throw new InvalidOperationException("创建关联数据失败，未影响任何数据库记录。");
                    }

                    return id; // 事务成功，返回 ID
                },
                // 可选：使用 errorCallBack 记录日志
                ex =>
                {
                    // _logger.LogError(ex, "删除用户 {UserId} 失败。", id);
                    //Console.WriteLine($"[ERROR] 删除用户 {id} 失败: {ex.Message}");
                }
            );

            // 检查结果并处理错误
            if (!result.IsSuccess)
            {
                // 关键：直接抛出原始异常以保留完整的 StackTrace
                throw result.ErrorException;
            }

            _logger.LogInformation("用户 {UserId} 创建用户 {UserName} (ID: {UserId}) 成功。", operatorId, createDto.UserName, result.Data);

            // 返回成功结果
            return result.Data;
        }

        /// <summary>
        /// 更新指定的用户。
        /// </summary>
        /// <param name="updateDto">更新数据传输对象，必须包含有效 ID。</param>
        /// <param name="operatorId">操作人用户 ID。</param>
        /// <returns>返回受影响的记录数（通常为 1，若记录不存在则为 0）。</returns>
        /// <exception cref="ArgumentException">当记录不存在、已删除或更新后违反唯一性约束时抛出。</exception>
        /// <exception cref="ArgumentNullException">当 updateDto 为 null 时抛出。</exception>
        /// <exception cref="InvalidOperationException">当数据库更新未影响任何行时抛出（并发冲突）。</exception>
        public async Task<long> UpdateAsync(UpdateSysUserDto updateDto, long operatorId)
        {
            if (updateDto == null)
                throw new ArgumentNullException(nameof(updateDto));
            if (updateDto.Id <= 0)
                throw new ArgumentException("无效的用户 ID。", nameof(updateDto.Id));

            if (string.IsNullOrWhiteSpace(updateDto.UserName))
                throw new ArgumentException("用户名称不能为空。", nameof(updateDto.UserName));
            //if (string.IsNullOrWhiteSpace(updateDto.Password))
            //    throw new ArgumentException("用户密码不能为空。", nameof(updateDto.Password));

            var entity = await _sysUserRepository.GetAsync(updateDto.Id);
            if (entity == null)
                throw new ArgumentException("指定的用户不存在或已被删除。", nameof(updateDto.Id));

            var trimmedName = updateDto.UserName.Trim();

            if (entity.UserName != trimmedName)
            {
                var exists = await _sysUserRepository.GetByUserNameAsync(trimmedName);
                if (exists != null && exists.Id != entity.Id)
                    throw new ArgumentException($"用户名称 '{trimmedName}' 已被占用。");
            }

            entity.UserName = trimmedName;
            entity.NickName = updateDto.NickName?.Trim();
            entity.PhoneNumber = updateDto.PhoneNumber?.Trim();
            entity.Email = updateDto.Email?.Trim();
            entity.Sex = updateDto.Sex?.Trim();
            entity.Avatar = updateDto.Avatar?.Trim();
            entity.Status = updateDto.Status?.Trim() ?? entity.Status;
            entity.Remark = updateDto.Remark?.Trim();
            entity.UpdatedById = operatorId;
            entity.UpdatedAt = DateTime.UtcNow;

            var roleIds = updateDto.RoleIds;

            // 使用 UseTranAsync<long> 显式指定返回类型
            DbResult<long> result = await _db.Ado.UseTranAsync<long>(
                async () =>
                {
                    var rowsAffected = 0;

                    // 删除关联数据
                    rowsAffected = await _sysUserRoleRepository.DeleteByUserIdAsync(entity.Id);
                    //if (rowsAffected == 0)
                    //    throw new InvalidOperationException("删除关联数据失败：未影响任何数据库记录。");

                    // 更新用户数据
                    rowsAffected = await _sysUserRepository.UpdateAsync(entity);
                    if (rowsAffected == 0)
                        throw new InvalidOperationException("更新失败：未影响任何数据库记录。");

                    if (roleIds != null && roleIds.Count > 0)
                    {
                        // 创建关联数据
                        var userRoleEntities = roleIds.Select(roleId => new SysUserRole { UserId = entity.Id, RoleId = roleId }).ToList();
                        rowsAffected = await _sysUserRoleRepository.CreateBatchAsync(userRoleEntities);

                        if (rowsAffected == 0)
                            throw new InvalidOperationException("创建关联数据失败，未影响任何数据库记录。");
                    }

                    return entity.Id; // 事务成功，返回 ID
                },
                // 可选：使用 errorCallBack 记录日志
                ex =>
                {
                    // _logger.LogError(ex, "删除用户 {UserId} 失败。", id);
                    //Console.WriteLine($"[ERROR] 删除用户 {id} 失败: {ex.Message}");
                }
            );

            // 检查结果并处理错误
            if (!result.IsSuccess)
            {
                // 关键：直接抛出原始异常以保留完整的 StackTrace
                throw result.ErrorException;
            }

            _logger.LogInformation("用户 {UserId} 更新用户 {UserName} (ID: {UserId}) 成功。", operatorId, entity.UserName, entity.Id);

            // 返回成功结果
            return result.Data;
        }

        /// <summary>
        /// 软删除指定的用户。
        /// </summary>
        /// <param name="id">要删除的用户 ID。</param>
        /// <param name="operatorId">操作人用户 ID。</param>
        /// <returns>返回被删除的记录 ID。</returns>
        /// <exception cref="ArgumentException">当记录不存在或已被删除时抛出。</exception>
        public async Task<long> DeleteAsync(long id, long operatorId)
        {
            if (id <= 0)
                throw new ArgumentException("无效的用户 ID。", nameof(id));

            var entity = await _sysUserRepository.GetAsync(id);
            if (entity == null)
                throw new ArgumentException("用户不存在或已被删除。");

            // 使用 UseTranAsync<long> 显式指定返回类型
            DbResult<long> result = await _db.Ado.UseTranAsync<long>(
                async () =>
                {
                    var rowsAffected = 0;

                    // 删除关联数据
                    rowsAffected = await _sysUserRoleRepository.DeleteByUserIdAsync(entity.Id);
                    //if (rowsAffected == 0)
                    //    throw new InvalidOperationException("删除关联数据失败：未影响任何数据库记录。");

                    // 执行软删除
                    entity.DeletedAt = DateTime.UtcNow;
                    entity.DeletedById = operatorId;
                    rowsAffected = await _sysUserRepository.DeleteAsync(entity);

                    if (rowsAffected == 0)
                    {
                        _logger.LogWarning("用户 {UserId} 删除用户 {UserName} (ID: {UserId}) 失败，未影响任何数据库记录。", operatorId, entity.UserName, entity.Id);
                        throw new InvalidOperationException($"用户 {operatorId} 删除用户 {entity.UserName} (ID: {entity.Id}) 失败，未影响任何数据库记录。");
                    }

                    return id; // 事务成功，返回 ID
                },
                // 可选：使用 errorCallBack 记录日志
                ex =>
                {
                    _logger.LogWarning("用户 {UserId} 删除用户 {UserName} (ID: {UserId}) 失败，{ErrorMessage}。", operatorId, entity.UserName, entity.Id, ex.Message);
                }
            );

            // 检查结果并处理错误
            if (!result.IsSuccess)
            {
                // 关键：直接抛出原始异常以保留完整的 StackTrace
                throw result.ErrorException;
            }

            _logger.LogInformation("用户 {UserId} 删除用户 {UserName} (ID: {UserId}) 成功。", operatorId, entity.UserName, entity.Id);

            // 返回成功结果
            return result.Data;
        }

        /// <summary>
        /// 批量软删除用户。
        /// </summary>
        /// <param name="ids">要删除的用户 ID 列表。</param>
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

            // 查询待删除的用户实体
            var entities = await _sysUserRepository.GetByIdsAsync(idList);
            if (entities.Count != idList.Count)
                throw new ArgumentException("部分用户不存在或已被删除。");

            // 使用 UseTranAsync<int> 显式指定返回类型
            DbResult<int> result = await _db.Ado.UseTranAsync<int>(
                async () =>
                {
                    // 删除关联数据
                    var rowsAffected = 0;
                    rowsAffected = await _sysUserRoleRepository.DeleteByUserIdsAsync(idList);
                    //if (rowsAffected == 0)
                    //    throw new InvalidOperationException("删除关联数据失败：未影响任何数据库记录。");

                    // 批量软删除用户
                    var deletedCount = await _sysUserRepository.DeleteBatchAsync(idList, operatorId);

                    // 校验删除数量
                    if (deletedCount != idList.Count)
                    {
                        _logger.LogWarning("用户 {UserId} 批量删除用户失败，期望删除 {Count} 条，但实际仅成功删除 {DeletedCount} 条。", operatorId, idList.Count, deletedCount);
                        throw new ArgumentException($"用户 {operatorId} 批量删除用户失败，期望删除 {idList.Count} 条，但实际仅成功删除 {deletedCount} 条。");
                    }

                    // 事务成功，返回删除数量
                    return deletedCount;
                },
                // 可选：错误回调，用于记录日志
                ex =>
                {
                    _logger.LogWarning("用户 {UserId} 批量删除用户失败，{ErrorMessage}。", operatorId, ex.Message);
                }
            );

            // 检查事务结果
            if (!result.IsSuccess)
            {
                // 直接抛出原始异常，保留完整的 StackTrace
                throw result.ErrorException;
            }

            _logger.LogInformation("用户 {UserId} 批量删除用户成功，删除数量 {DeletedCount}。", operatorId, entities.Count);

            // 返回成功结果
            return result.Data;
        }

        /// <summary>
        /// 根据主键获取单条未删除的用户详情。
        /// </summary>
        /// <param name="id">用户唯一标识。</param>
        /// <returns>返回匹配的 DTO；若未找到或已删除，则返回 null。</returns>
        public async Task<SysUserDto?> GetAsync(long id)
        {
            var entity = await _sysUserRepository.GetAsync(id);
            if (entity == null)
                return null;
            var roles = await _sysRoleRepository.GetAllByUserIdAsync(id);
            return MapToDto(entity, roles);
        }

        /// <summary>
        /// 查询未删除的用户列表，支持按用户名称模糊搜索和状态精确筛选。
        /// </summary>
        /// <param name="userName">用户名称，用于模糊搜索（可选）。</param>
        /// <param name="status">用户状态，用于精确匹配（可选）。</param>
        /// <returns>返回匹配条件的 DTO 列表（可能为空）。</returns>
        public async Task<List<SysUserDto>> GetAllAsync(string? userName = null, string? status = null)
        {
            var entities = await _sysUserRepository.GetAllAsync(userName, status);
            var list = new List<SysUserDto>();
            foreach (var entity in entities)
            {
                var roles = await _sysRoleRepository.GetAllByUserIdAsync(entity.Id);
                list.Add(MapToDto(entity, roles));
            }
            return list;
        }

        /// <summary>
        /// 分页查询未删除的用户列表，支持名称、手机号码模糊搜索和状态精确筛选。
        /// </summary>
        /// <param name="pageNumber">页码，从 1 开始。</param>
        /// <param name="pageSize">每页数量。</param>
        /// <param name="userName">用户名称，用于模糊搜索（可选）。</param>
        /// <param name="phoneNumber">用户手机号码，用于模糊搜索（可选）。</param>
        /// <param name="status">用户状态，用于精确匹配（可选）。</param>
        /// <param name="orderByField">排序字段（可选）。</param>
        /// <param name="orderByType">排序方式（可选）。</param>
        /// <returns>返回分页结果对象，包含数据和分页元信息。</returns>
        public async Task<PaginationResult<SysUserDto>> GetPagedAsync(
            int pageNumber,
            int pageSize,
            string? userName = null,
            string? phoneNumber = null,
            string? status = null,
            string? orderByField = null,
            string? orderByType = null)
        {
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = 10;

            // 调用仓储层获取分页数据和总数
            var (data, totalCount) = await _sysUserRepository.GetPagedAsync(pageNumber, pageSize, userName, phoneNumber, status, orderByField, orderByType);
            var pagedData = new List<SysUserDto>();
            foreach (var entity in data)
            {
                var roles = await _sysRoleRepository.GetAllByUserIdAsync(entity.Id);
                pagedData.Add(MapToDto(entity, roles));
            }

            // 计算总页数
            int totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            // 封装为 PaginationResult
            return new PaginationResult<SysUserDto>(
                items: pagedData,
                totalPages: totalPages,
                totalItems: totalCount,
                currentPage: pageNumber,
                pageSize: pageSize);
        }

        /// <summary>
        /// 根据用户名称获取唯一未删除的用户详情。
        /// </summary>
        /// <param name="userName">用户的名称（如“admin”），用于精确匹配。</param>
        /// <returns>返回匹配的 DTO；若未找到或已删除，则返回 null。</returns>
        public async Task<SysUserDto?> GetByUserNameAsync(string userName)
        {
            if (string.IsNullOrWhiteSpace(userName))
                return null;

            var entity = await _sysUserRepository.GetByUserNameAsync(userName.Trim());
            if (entity == null)
                return null;
            var roles = await _sysRoleRepository.GetAllByUserIdAsync(entity.Id);
            return MapToDto(entity, roles);
        }

        /// <summary>
        /// 更新用户的状态。
        /// </summary>
        /// <param name="id">要更新的用户 ID。</param>
        /// <param name="status">用户状态</param>
        /// <param name="operatorId">操作人用户 ID。</param>
        /// <returns>返回被更新的记录 ID。</returns>
        public async Task<long> ChangeStatusAsync(long id, string status, long operatorId)
        {
            if (id <= 0)
                throw new ArgumentException("无效的用户 ID。", nameof(id));

            if (string.IsNullOrWhiteSpace(status) || (status != "0" && status != "1"))
                throw new ArgumentException("无效的状态值，状态值必须为 '0'（正常）或 '1'（停用）");

            var entity = await _sysUserRepository.GetAsync(id);
            if (entity == null)
                throw new ArgumentException("用户不存在或已被删除。");

            entity.Status = status;
            entity.UpdatedById = operatorId;
            entity.UpdatedAt = DateTime.UtcNow;

            var rowsAffected = await _sysUserRepository.UpdateAsync(entity);
            if (rowsAffected == 0)
            {
                _logger.LogWarning("用户 {UserId} 更新用户状态 {UserName} (ID: {UserId},Status:{Status}) 失败，未影响任何数据库记录。", operatorId, entity.UserName, entity.Id, status);
                throw new InvalidOperationException($"用户 {operatorId} 更新用户状态 {entity.UserName} (ID: {entity.Id},Status: {status}) 失败，未影响任何数据库记录。");
            }

            _logger.LogInformation("用户 {UserId} 更新用户状态 {UserName} (ID: {UserId},Status:{Status}) 成功。", operatorId, entity.UserName, entity.Id, status);

            return entity.Id;
        }

        /// <summary>
        /// 重置用户密码。
        /// </summary>
        /// <param name="id">要更新的用户 ID。</param>
        /// <param name="changeUserPasswordDto">数据传输对象</param>
        /// <param name="operatorId">操作人用户 ID。</param>
        /// <returns>返回被更新的记录 ID。</returns>
        public async Task<long> ResetPasswordAsync(long id, ResetUserPasswordDto changeUserPasswordDto, long operatorId)
        {
            if (id <= 0)
                throw new ArgumentException("无效的用户 ID。", nameof(id));

            var entity = await _sysUserRepository.GetAsync(id);
            if (entity == null)
                throw new ArgumentException("用户不存在或已被删除。");

            var hashedPassword = _passwordHasher.HashPassword(changeUserPasswordDto.Password); // 使用密码哈希器哈希密码

            entity.Password = hashedPassword;
            entity.UpdatedById = operatorId;
            entity.UpdatedAt = DateTime.UtcNow;

            var rowsAffected = await _sysUserRepository.UpdateAsync(entity);
            if (rowsAffected == 0)
                throw new InvalidOperationException("更新失败：未影响任何数据库记录。");

            return entity.Id;
        }

        /// <summary>
        /// 更新个人信息
        /// </summary>
        /// <param name="updateDto">数据传输对象</param>
        /// <param name="operatorId">操作人用户 ID。</param>
        /// <returns>返回被更新的记录 ID。</returns>
        public async Task<long> UpdateProfileAsync(UpdateSysUserProfileDto updateDto, long operatorId)
        {
            if (updateDto == null)
                throw new ArgumentNullException(nameof(updateDto));
            if (updateDto.Id <= 0)
                throw new ArgumentException("无效的用户 ID。", nameof(updateDto.Id));

            var entity = await _sysUserRepository.GetAsync(updateDto.Id);
            if (entity == null)
                throw new ArgumentException("用户不存在或已被删除。");

            entity.NickName = updateDto.NickName;
            entity.PhoneNumber = updateDto.PhoneNumber;
            entity.Email = updateDto.Email;
            entity.Sex = updateDto.Sex;
            entity.UpdatedById = operatorId;
            entity.UpdatedAt = DateTime.UtcNow;

            var rowsAffected = await _sysUserRepository.UpdateAsync(entity);
            if (rowsAffected == 0)
                throw new InvalidOperationException("更新失败：未影响任何数据库记录。");

            return entity.Id;
        }

        /// <summary>
        /// 更新个人密码
        /// </summary>
        /// <param name="id">要更新的用户 ID。</param>
        /// <param name="changeUserProfilePasswordDto">数据传输对象</param>
        /// <param name="operatorId">操作人用户 ID。</param>
        /// <returns>返回被更新的记录 ID。</returns>
        public async Task<long> UpdateProfilePasswordAsync(long id, ChangeUserProfilePasswordDto changeUserProfilePasswordDto, long operatorId)
        {
            if (id <= 0)
                throw new ArgumentException("无效的用户 ID。", nameof(id));

            var entity = await _sysUserRepository.GetAsync(id);
            if (entity == null)
                throw new ArgumentException("用户不存在或已被删除。");

            // 旧密码错误
            if (!_passwordHasher.VerifyHashedPassword(entity.Password!, changeUserProfilePasswordDto.OldPassword))
            {
                throw new ArgumentException("旧密码错误");
            }
            // 新密码不能与旧密码相同
            if (changeUserProfilePasswordDto.NewPassword == changeUserProfilePasswordDto.OldPassword)
            {
                throw new ArgumentException("新密码不能与旧密码相同");
            }
            var hashedPassword = _passwordHasher.HashPassword(changeUserProfilePasswordDto.NewPassword!); // 使用密码哈希器哈希密码

            entity.Password = hashedPassword;
            entity.UpdatedById = operatorId;
            entity.UpdatedAt = DateTime.UtcNow;

            var rowsAffected = await _sysUserRepository.UpdateAsync(entity);
            if (rowsAffected == 0)
                throw new InvalidOperationException("更新失败：未影响任何数据库记录。");

            return entity.Id;
        }

        /// <summary>
        /// 更新个人头像
        /// </summary>
        /// <param name="id">要更新的用户 ID。</param>
        /// <param name="avatar">头像 URL。</param>
        /// <param name="operatorId">操作人用户 ID。</param>
        /// <returns>返回被更新的记录 ID。</returns>
        public async Task<long> UpdateProfileAvatarAsync(long id, string avatar, long operatorId)
        {
            if (id <= 0)
                throw new ArgumentException("无效的用户 ID。", nameof(id));

            var entity = await _sysUserRepository.GetAsync(id);
            if (entity == null)
                throw new ArgumentException("用户不存在或已被删除。");

            entity.Avatar = avatar;
            entity.UpdatedById = operatorId;
            entity.UpdatedAt = DateTime.UtcNow;

            var rowsAffected = await _sysUserRepository.UpdateAsync(entity);
            if (rowsAffected == 0)
                throw new InvalidOperationException("更新失败：未影响任何数据库记录。");

            return entity.Id;
        }

        /// <summary>
        /// 根据主键获取单条未删除的用户详情。
        /// </summary>
        /// <param name="id">用户唯一标识。</param>
        /// <returns>返回匹配的 DTO；若未找到或已删除，则返回 null。</returns>
        public async Task<SysUserSummary?> GetSummaryAsync(long id)
        {
            var summary = await _sysUserRepository.GetSummaryAsync(id);
            if (summary == null)
                return null;

            return summary;
        }

        /// <summary>
        /// 查询未删除的用户列表，支持按用户名称模糊搜索和状态精确筛选。
        /// </summary>
        /// <param name="userName">用户名称，用于模糊搜索（可选）。</param>
        /// <param name="status">用户状态，用于精确匹配（可选）。</param>
        /// <returns>返回匹配条件的 DTO 列表（可能为空）。</returns>
        public async Task<List<SysUserSummary>> GetAllSummaryAsync(string? userName = null, string? status = null)
        {
            var summaries = await _sysUserRepository.GetAllSummaryAsync(userName, status);

            return summaries;
        }

        /// <summary>
        /// 分页查询未删除的用户列表，支持名称、手机号码模糊搜索和状态精确筛选。
        /// </summary>
        /// <param name="pageNumber">页码，从 1 开始。</param>
        /// <param name="pageSize">每页数量。</param>
        /// <param name="userName">用户名称，用于模糊搜索（可选）。</param>
        /// <param name="phoneNumber">用户手机号码，用于模糊搜索（可选）。</param>
        /// <param name="status">用户状态，用于精确匹配（可选）。</param>
        /// <param name="orderByField">排序字段（可选）。</param>
        /// <param name="orderByType">排序方式（可选）。</param>
        /// <returns>返回分页结果对象，包含数据和分页元信息。</returns>
        public async Task<PaginationResult<SysUserSummary>> GetPagedSummaryAsync(
            int pageNumber,
            int pageSize,
            string? userName = null,
            string? phoneNumber = null,
            string? status = null,
            string? orderByField = null,
            string? orderByType = null)
        {
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = 10;

            // 调用仓储层获取分页数据和总数
            var (data, totalCount) = await _sysUserRepository.GetPagedSummaryAsync(pageNumber, pageSize, userName, phoneNumber, status, orderByField, orderByType);

            // 计算总页数
            int totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            // 封装为 PaginationResult
            return new PaginationResult<SysUserSummary>(
                items: data,
                totalPages: totalPages,
                totalItems: totalCount,
                currentPage: pageNumber,
                pageSize: pageSize);
        }

        /// <summary>
        /// 将领域实体映射为应用层数据传输对象。
        /// </summary>
        /// <param name="entity">源实体对象。</param>
        /// <param name="roles">关联的角色列表。</param>
        /// <returns>映射后的 SysUserDto 实例。</returns>
        private static SysUserDto MapToDto(SysUser entity, IEnumerable<SysRole> roles)
        {
            return new SysUserDto
            {
                Id = entity.Id,
                UserName = entity.UserName,
                NickName = entity.NickName,
                PhoneNumber = entity.PhoneNumber,
                Email = entity.Email,
                Sex = entity.Sex,
                Avatar = entity.Avatar,
                Status = entity.Status,
                Remark = entity.Remark,
                CreatedAt = entity.CreatedAt,
                Roles = roles.Select(role => new SysRoleMiniDto
                {
                    Id = role.Id,
                    Name = role.Name,
                    Key = role.Key
                })
            };
        }

    }
}