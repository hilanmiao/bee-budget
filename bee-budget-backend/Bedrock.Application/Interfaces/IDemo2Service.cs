using Bedrock.Application.DataTransferObjects;

namespace Bedrock.Application.Interfaces
{
    /// <summary>
    /// 样例2服务接口。
    /// <para>
    /// 支持软删除（通过 DeletedAt 字段标记），所有查询默认过滤已删除记录。
    /// 业务规则：Demo1Id 和 Code 保证全局唯一。
    /// </para>
    /// </summary>
    public interface IDemo2Service
    {
        /// <summary>
        /// 创建新的样例2。
        /// </summary>
        /// <param name="createDto">创建数据传输对象，包含 Name,Demo1Id 等字段。</param>
        /// <param name="operatorId">操作人 ID，用于审计。</param>
        /// <returns>返回新创建记录的主键 ID。</returns>
        /// <exception cref="ArgumentException">当 Demo1Id 和 Code 已存在时抛出。</exception>
        /// <exception cref="ArgumentNullException">当 createDto 为 null 时抛出。</exception>
        Task<long> CreateAsync(CreateDemo2Dto createDto, long operatorId);

        /// <summary>
        /// 批量创建样例2。
        /// </summary>
        /// <param name="createDtos">创建 DTO 集合。</param>
        /// <param name="operatorId">操作人 ID。</param>
        /// <returns>返回成功插入的记录数量；若集合为空则返回 0。</returns>
        /// <exception cref="ArgumentException">当存在重复名称时抛出。</exception>
        Task<int> CreateBatchAsync(IEnumerable<CreateDemo2Dto> createDtos, long operatorId);

        /// <summary>
        /// 更新指定的样例2。
        /// </summary>
        /// <param name="updateDto">更新数据传输对象，必须包含有效 ID。</param>
        /// <param name="operatorId">操作人 ID。</param>
        /// <returns>返回被更新的记录 ID。</returns>
        /// <exception cref="ArgumentException">当记录不存在、已删除或更新后违反唯一性约束时抛出。</exception>
        /// <exception cref="ArgumentNullException">当 updateDto 为 null 时抛出。</exception>
        /// <exception cref="InvalidOperationException">当数据库更新未影响任何行时抛出（并发冲突）。</exception>
        Task<long> UpdateAsync(UpdateDemo2Dto updateDto, long operatorId);

        /// <summary>
        /// 批量更新样例2。
        /// </summary>
        /// <param name="updateDtos">更新 DTO 集合，每个必须包含有效 ID。</param>
        /// <param name="operatorId">操作人 ID。</param>
        /// <returns>返回成功更新的记录数量；若集合为空则返回 0。</returns>
        /// <exception cref="ArgumentException">当任一记录不存在或违反业务规则时抛出。</exception>
        Task<int> UpdateBatchAsync(IEnumerable<UpdateDemo2Dto> updateDtos, long operatorId);

        /// <summary>
        /// 软删除指定的样例2。
        /// </summary>
        /// <param name="id">要删除的样例2 ID。</param>
        /// <param name="operatorId">操作人 ID。</param>
        /// <returns>返回被删除的记录 ID。</returns>
        /// <exception cref="ArgumentException">当记录不存在或已被删除时抛出。</exception>
        Task<long> DeleteAsync(long id, long operatorId);

        /// <summary>
        /// 批量软删除样例2。
        /// </summary>
        /// <param name="ids">要删除的样例2 ID 列表。</param>
        /// <param name="operatorId">操作人 ID。</param>
        /// <returns>返回成功标记为删除的记录数量；若 ID 列表为空则返回 0。</returns>
        /// <exception cref="ArgumentException">当部分 ID 不存在或已被删除时可能抛出。</exception>
        Task<int> DeleteBatchAsync(IEnumerable<long> ids, long operatorId);

        /// <summary>
        /// 更新样例2的状态。
        /// </summary>
        /// <param name="id">要更新的样例2 ID。</param>
        /// <param name="status">状态</param>
        /// <param name="operatorId">操作人 ID。</param>
        /// <returns>返回被更新的记录 ID。</returns>
        Task<long> ChangeStatusAsync(long id, string status, long operatorId);

        /// <summary>
        /// 根据主键获取单条未删除的样例2详情。
        /// </summary>
        /// <param name="id">样例2唯一标识。</param>
        /// <returns>返回匹配的 DTO；若未找到或已删除，则返回 null。</returns>
        Task<Demo2Dto?> GetAsync(long id);

        /// <summary>
        /// 查询未删除的样例2记录，支持按名称模糊搜索、状态和Demo1Id精确搜索。
        /// </summary>
        /// <param name="name">样例2名称，用于模糊搜索（可选）。</param>
        /// <param name="status">样例2状态，用于模糊搜索（可选）。</param>
        /// <param name="demo1Id">样例2Demo1Id，用于精确搜索（可选）。</param>
        /// <param name="startDate">开始日期，用于日期范围搜索（可选）。</param>
        /// <param name="endDate">结束日期，用于日期范围搜索（可选）。</param>
        /// <returns>返回匹配条件的 DTO 列表（可能为空）。</returns>
        Task<List<Demo2Dto>> GetAllAsync(
            string? name = null,
            string? status = null,
            long? demo1Id = null,
            DateTime? startDate = null,
            DateTime? endDate = null);

        /// <summary>
        /// 分页查询未删除的样例2记录，支持名称模糊搜索、状态和Demo1Id精确搜索。
        /// </summary>
        /// <param name="pageNumber">页码，从 1 开始。</param>
        /// <param name="pageSize">每页记录数。</param>
        /// <param name="name">样例2名称，用于模糊搜索（可选）。</param>
        /// <param name="status">样例2状态，用于模糊搜索（可选）。</param>
        /// <param name="demo1Id">样例2Demo1Id，用于精确搜索（可选）。</param>
        /// <param name="startDate">开始日期，用于日期范围搜索（可选）。</param>
        /// <param name="endDate">结束日期，用于日期范围搜索（可选）。</param>
        /// <param name="orderByField">排序字段（可选）。</param>
        /// <param name="orderByType">排序方式（可选）。</param>
        /// <returns>返回分页结果对象，包含数据和分页元信息。</returns>
        Task<PaginationResult<Demo2Dto>> GetPagedAsync(
            int pageNumber,
            int pageSize,
            string? name = null,
            string? status = null,
            long? demo1Id = null,
            DateTime? startDate = null,
            DateTime? endDate = null,
            string? orderByField = null,
            string? orderByType = null);

        /// <summary>
        /// 根据样例2Demo1Id查询未删除的样例2记录。
        /// </summary>
        /// <param name="demo1Id">样例2Demo1Id。</param>
        /// <returns>返回匹配条件的 DTO 列表（可能为空）。</returns>
        Task<List<Demo2Dto>> GetAllByDemo1IdAsync(long demo1Id);

        /// <summary>
        /// 根据主键获取单条未删除的样例2详情。
        /// </summary>
        /// <param name="id">样例2唯一标识。</param>
        /// <returns>返回匹配的 DTO；若未找到或已删除，则返回 null。</returns>
        Task<Demo2Summary?> GetSummaryAsync(long id);

        /// <summary>
        /// 查询未删除的样例2记录，支持按Demo1Id精确搜索、状态和Demo1Id精确搜索。
        /// </summary>
        /// <param name="name">样例2名称，用于模糊搜索（可选）。</param>
        /// <param name="status">样例2状态，用于模糊搜索（可选）。</param>
        /// <param name="demo1Id">样例2Demo1Id，用于精确搜索（可选）。</param>
        /// <param name="startDate">开始日期，用于日期范围搜索（可选）。</param>
        /// <param name="endDate">结束日期，用于日期范围搜索（可选）。</param>
        /// <returns>返回匹配条件的 DTO 列表（可能为空）。</returns>
        Task<List<Demo2Summary>> GetAllSummaryAsync(
            string? name = null,
            string? status = null,
            long? demo1Id = null,
            DateTime? startDate = null,
            DateTime? endDate = null);

        /// <summary>
        /// 分页查询未删除的样例2记录，支持名称模糊搜索、状态和Demo1Id精确搜索。
        /// </summary>
        /// <param name="pageNumber">页码，从 1 开始。</param>
        /// <param name="pageSize">每页记录数。</param>
        /// <param name="name">样例2名称，用于模糊搜索（可选）。</param>
        /// <param name="status">样例2状态，用于模糊搜索（可选）。</param>
        /// <param name="demo1Id">样例2Demo1Id，用于精确搜索（可选）。</param>
        /// <param name="startDate">开始日期，用于日期范围搜索（可选）。</param>
        /// <param name="endDate">结束日期，用于日期范围搜索（可选）。</param>
        /// <param name="orderByField">排序字段（可选）。</param>
        /// <param name="orderByType">排序方式（可选）。</param>
        /// <returns>返回分页结果对象，包含数据和分页元信息。</returns>
        Task<PaginationResult<Demo2Summary>> GetPagedSummaryAsync(
            int pageNumber,
            int pageSize,
            string? name = null,
            string? status = null,
            long? demo1Id = null,
            DateTime? startDate = null,
            DateTime? endDate = null,
            string? orderByField = null,
            string? orderByType = null);

    }
}