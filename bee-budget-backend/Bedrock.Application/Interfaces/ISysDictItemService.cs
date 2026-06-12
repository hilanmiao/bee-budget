using Bedrock.Application.DataTransferObjects;

namespace Bedrock.Application.Interfaces
{
    /// <summary>
    /// 系统字典项应用服务接口。
    /// <para>
    /// 负责字典项的创建、更新、删除、查询等业务逻辑，支持软删除语义。
    /// 所有查询方法默认不返回已删除记录。
    /// 业务规则：<c>Value</c> 与 <c>Label</c> 全局唯一。
    /// </para>
    /// </summary>
    public interface ISysDictItemService
    {
        /// <summary>
        /// 创建新的字典项。
        /// </summary>
        /// <param name="createDto">创建数据传输对象，包含 <c>Value</c>、<c>Label</c> 等字段。</param>
        /// <param name="operatorId">操作人用户 ID，用于审计。</param>
        /// <returns>返回新创建记录的主键 ID。</returns>
        /// <exception cref="ArgumentException">当 <c>Value</c> 或 <c>Label</c> 已存在时抛出。</exception>
        Task<long> CreateAsync(CreateSysDictItemDto createDto, long operatorId);

        /// <summary>
        /// 批量创建字典项。
        /// </summary>
        /// <param name="createDtos">创建 DTO 集合。</param>
        /// <param name="operatorId">操作人用户 ID。</param>
        /// <returns>返回成功插入的记录数量；若集合为空则返回 0。</returns>
        /// <exception cref="ArgumentException">当存在重复实际值或标签时抛出。</exception>
        Task<int> CreateBatchAsync(IEnumerable<CreateSysDictItemDto> createDtos, long operatorId);

        /// <summary>
        /// 更新指定的字典项。
        /// </summary>
        /// <param name="updateDto">更新数据传输对象，必须包含有效 ID。</param>
        /// <param name="operatorId">操作人用户 ID。</param>
        /// <returns>返回更新后的记录 ID。</returns>
        /// <exception cref="ArgumentException">当记录不存在、已删除或更新后违反唯一性约束时抛出。</exception>
        Task<long> UpdateAsync(UpdateSysDictItemDto updateDto, long operatorId);

        /// <summary>
        /// 批量更新字典项。
        /// </summary>
        /// <param name="updateDtos">更新 DTO 集合，每个必须包含有效 ID。</param>
        /// <param name="operatorId">操作人用户 ID。</param>
        /// <returns>返回成功更新的记录数量；若集合为空则返回 0。</returns>
        /// <exception cref="ArgumentException">当任一记录不存在或违反业务规则时抛出。</exception>
        Task<int> UpdateBatchAsync(IEnumerable<UpdateSysDictItemDto> updateDtos, long operatorId);

        /// <summary>
        /// 软删除指定的字典项。
        /// </summary>
        /// <param name="id">要删除的字典项 ID。</param>
        /// <param name="operatorId">操作人用户 ID。</param>
        /// <returns>返回被删除的记录 ID。</returns>
        /// <exception cref="ArgumentException">当记录不存在或已被删除时抛出。</exception>
        Task<long> DeleteAsync(long id, long operatorId);

        /// <summary>
        /// 批量软删除字典项。
        /// </summary>
        /// <param name="ids">要删除的字典项 ID 列表。</param>
        /// <param name="operatorId">操作人用户 ID。</param>
        /// <returns>返回成功标记为删除的记录数量；若 ID 列表为空则返回 0。</returns>
        /// <exception cref="ArgumentException">当部分 ID 不存在或已被删除时可能抛出。</exception>
        Task<int> DeleteBatchAsync(IEnumerable<long> ids, long operatorId);

        /// <summary>
        /// 根据主键获取字典项详情。
        /// </summary>
        /// <param name="id">字典项唯一标识。</param>
        /// <returns>返回匹配的 DTO；若未找到或已删除，则返回 <c>null</c>。</returns>
        Task<SysDictItemDto?> GetAsync(long id);

        /// <summary>
        /// 查询字典项列表，支持按标签模糊搜索和状态精确筛选。
        /// </summary>
        /// <param name="label">字典项标签，用于模糊搜索（可选）。</param>
        /// <param name="status">字典项状态，用于精确匹配（可选）。</param>
        /// <returns>返回匹配条件的 DTO 列表（可能为空）。</returns>
        Task<List<SysDictItemDto>> GetAllAsync(string? label = null, string? status = null);

        /// <summary>
        /// 分页查询字典项列表。
        /// </summary>
        /// <param name="pageNumber">页码，从 1 开始。</param>
        /// <param name="pageSize">每页数量。</param>
        /// <param name="label">字典项标签，模糊搜索（可选）。</param>
        /// <param name="value">字典项实际值，模糊搜索（可选）。</param>
        /// <param name="status">字典项状态，精确匹配（可选）。</param>
        /// <param name="categoryCode">字典分类编码，精确匹配（可选）。</param>
        /// <param name="orderByField">排序字段（可选）。</param>
        /// <param name="orderByType">排序方式（可选）。</param>
        /// <returns>返回分页结果对象，包含数据和分页元信息。</returns>
        Task<PaginationResult<SysDictItemDto>> GetPagedAsync(
            int pageNumber,
            int pageSize,
            string? label = null,
            string? value = null,
            string? status = null, 
            string? categoryCode = null,
            string? orderByField = null,
            string? orderByType = null);

        /// <summary>
        /// 根据字典项实际值获取其详情。
        /// </summary>
        /// <param name="value">字典项实际值（如 0）。</param>
        /// <returns>返回匹配的 DTO；若未找到或已删除，则返回 <c>null</c>。</returns>
        Task<SysDictItemDto?> GetByValueAsync(string value);

        /// <summary>
        /// 根据字典项标签获取其详情。
        /// </summary>
        /// <param name="label">字典项的显示标签（如“男”）。</param>
        /// <returns>返回匹配的 DTO；若未找到或已删除，则返回 <c>null</c>。</returns>
        Task<SysDictItemDto?> GetByLabelAsync(string label);
        
        /// <summary>
        /// 根据字典分类编码查询字典项列表。
        /// </summary>
        /// <param name="categoryCode">字典分类编码。</param>
        /// <returns>返回匹配条件的 DTO 列表（可能为空）。</returns>
        Task<List<SysDictItemDto>> GetAllByCategoryCodeAsync(string categoryCode);

        /// <summary>
        /// 更新字典项的状态。
        /// </summary>
        /// <param name="id">要更新的字典项 ID。</param>
        /// <param name="status">状态</param>
        /// <param name="operatorId">操作人用户 ID。</param>
        /// <returns>返回被更新的记录 ID。</returns>
        Task<long> ChangeStatusAsync(long id, string status, long operatorId);
    }
}