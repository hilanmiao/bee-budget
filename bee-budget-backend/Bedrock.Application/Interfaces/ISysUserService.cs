using Bedrock.Application.DataTransferObjects;

namespace Bedrock.Application.Interfaces
{
    /// <summary>
    /// 系统用户应用服务接口。
    /// <para>
    /// 支持软删除（通过 DeletedAt 字段标记），所有查询默认过滤已删除记录。
    /// 业务规则：UserName 保证全局唯一。
    /// </para>
    /// </summary>
    public interface ISysUserService
    {
        /// <summary>
        /// 创建新的用户。
        /// </summary>
        /// <param name="createDto">创建数据传输对象，包含 UserName 等字段。</param>
        /// <param name="operatorId">操作人用户 ID，用于审计。</param>
        /// <returns>返回新创建记录的主键 ID。</returns>
        /// <exception cref="ArgumentException">当 UserName 已存在时抛出。</exception>
        /// <exception cref="ArgumentNullException">当 createDto 为 null 时抛出。</exception>
        Task<long> CreateAsync(CreateSysUserDto createDto, long operatorId);

        /// <summary>
        /// 更新指定的用户。
        /// </summary>
        /// <param name="updateDto">更新数据传输对象，必须包含有效 ID。</param>
        /// <param name="operatorId">操作人用户 ID。</param>
        /// <returns>返回被更新的记录 ID。</returns>
        /// <exception cref="ArgumentException">当记录不存在、已删除或更新后违反唯一性约束时抛出。</exception>
        /// <exception cref="ArgumentNullException">当 updateDto 为 null 时抛出。</exception>
        /// <exception cref="InvalidOperationException">当数据库更新未影响任何行时抛出（并发冲突）。</exception>
        Task<long> UpdateAsync(UpdateSysUserDto updateDto, long operatorId);

        /// <summary>
        /// 软删除指定的用户。
        /// </summary>
        /// <param name="id">要删除的用户 ID。</param>
        /// <param name="operatorId">操作人用户 ID。</param>
        /// <returns>返回被删除的记录 ID。</returns>
        /// <exception cref="ArgumentException">当记录不存在或已被删除时抛出。</exception>
        Task<long> DeleteAsync(long id, long operatorId);

        /// <summary>
        /// 批量软删除用户。
        /// </summary>
        /// <param name="ids">要删除的用户 ID 列表。</param>
        /// <param name="operatorId">操作人用户 ID。</param>
        /// <returns>返回成功标记为删除的记录数量；若 ID 列表为空则返回 0。</returns>
        /// <exception cref="ArgumentException">当部分 ID 不存在或已被删除时可能抛出。</exception>
        Task<int> DeleteBatchAsync(IEnumerable<long> ids, long operatorId);

        /// <summary>
        /// 根据主键获取单条未删除的用户详情。
        /// </summary>
        /// <param name="id">用户唯一标识。</param>
        /// <returns>返回匹配的 DTO；若未找到或已删除，则返回 null。</returns>
        Task<SysUserDto?> GetAsync(long id);

        /// <summary>
        /// 查询未删除的用户列表，支持按用户名称模糊搜索和状态精确筛选。
        /// </summary>
        /// <param name="userName">用户名称，用于模糊搜索（可选）。</param>
        /// <param name="status">用户状态，用于精确匹配（可选）。</param>
        /// <returns>返回匹配条件的 DTO 列表（可能为空）。</returns>
        Task<List<SysUserDto>> GetAllAsync(string? userName = null, string? status = null);

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
        Task<PaginationResult<SysUserDto>> GetPagedAsync(
            int pageNumber,
            int pageSize,
            string? userName = null,
            string? phoneNumber = null,
            string? status = null,
            string? orderByField = null,
            string? orderByType = null);

        /// <summary>
        /// 根据用户名称获取唯一未删除的用户详情。
        /// </summary>
        /// <param name="userName">用户的名称（如“admin”），用于精确匹配。</param>
        /// <returns>返回匹配的 DTO；若未找到或已删除，则返回 null。</returns>
        Task<SysUserDto?> GetByUserNameAsync(string userName);

        /// <summary>
        /// 更新用户的状态。
        /// </summary>
        /// <param name="id">要更新的用户 ID。</param>
        /// <param name="status">用户状态</param>
        /// <param name="operatorId">操作人用户 ID。</param>
        /// <returns>返回被更新的记录 ID。</returns>
        Task<long> ChangeStatusAsync(long id, string status, long operatorId);

        /// <summary>
        /// 重置用户密码。
        /// </summary>
        /// <param name="id">要更新的用户 ID。</param>
        /// <param name="resetUserPasswordDto">数据传输对象</param>
        /// <param name="operatorId">操作人用户 ID。</param>
        /// <returns>返回被更新的记录 ID。</returns>
        Task<long> ResetPasswordAsync(long id, ResetUserPasswordDto resetUserPasswordDto, long operatorId);

        /// <summary>
        /// 更新个人信息
        /// </summary>
        /// <param name="updateDto">数据传输对象</param>
        /// <param name="operatorId">操作人用户 ID。</param>
        /// <returns>返回被更新的记录 ID。</returns>
        Task<long> UpdateProfileAsync(UpdateSysUserProfileDto updateDto, long operatorId);

        /// <summary>
        /// 更新个人密码
        /// </summary>
        /// <param name="id">要更新的用户 ID。</param>
        /// <param name="changeUserProfilePasswordDto">数据传输对象</param>
        /// <param name="operatorId">操作人用户 ID。</param>
        /// <returns>返回被更新的记录 ID。</returns>
        Task<long> UpdateProfilePasswordAsync(long id, ChangeUserProfilePasswordDto changeUserProfilePasswordDto, long operatorId);

        /// <summary>
        /// 更新个人头像
        /// </summary>
        /// <param name="id">要更新的用户 ID。</param>
        /// <param name="avatar">头像 URL。</param>
        /// <param name="operatorId">操作人用户 ID。</param>
        /// <returns>返回被更新的记录 ID。</returns>
        Task<long> UpdateProfileAvatarAsync(long id, string avatar, long operatorId);

        /// <summary>
        /// 根据主键获取单条未删除的用户详情。
        /// </summary>
        /// <param name="id">用户唯一标识。</param>
        /// <returns>返回匹配的 DTO；若未找到或已删除，则返回 null。</returns>
        Task<SysUserSummary?> GetSummaryAsync(long id);

        /// <summary>
        /// 查询未删除的用户列表，支持按用户名称模糊搜索和状态精确筛选。
        /// </summary>
        /// <param name="userName">用户名称，用于模糊搜索（可选）。</param>
        /// <param name="status">用户状态，用于精确匹配（可选）。</param>
        /// <returns>返回匹配条件的 DTO 列表（可能为空）。</returns>
        Task<List<SysUserSummary>> GetAllSummaryAsync(string? userName = null, string? status = null);

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
        Task<PaginationResult<SysUserSummary>> GetPagedSummaryAsync(
            int pageNumber,
            int pageSize,
            string? userName = null,
            string? phoneNumber = null,
            string? status = null,
            string? orderByField = null,
            string? orderByType = null);
    }
}