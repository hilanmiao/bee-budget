using Bedrock.Core.Entities;

namespace Bedrock.Application.Interfaces
{
    /// <summary>
    /// 系统字典项仓储接口。
    /// <para>
    /// 支持软删除（通过 <c>DeletedAt</c> 字段标记），所有查询方法默认过滤已删除记录。
    /// 业务规则：<c>Label</c> 与 <c>Value</c> 均保证全局唯一。
    /// </para>
    /// </summary>
    public interface ISysDictItemRepository
    {
        /// <summary>
        /// 创建一条新的字典项记录。
        /// </summary>
        /// <param name="entity">待创建的实体。主键（Id）由数据库生成。</param>
        /// <returns>返回插入后生成的主键 ID。</returns>
        Task<long> CreateAsync(SysDictItem entity);

        /// <summary>
        /// 批量创建字典项记录。
        /// </summary>
        /// <param name="entities">待创建的实体集合。</param>
        /// <returns>返回成功插入的记录数量；若集合为空，则返回 0。</returns>
        Task<int> CreateBatchAsync(IEnumerable<SysDictItem> entities);

        /// <summary>
        /// 更新一条字典项记录。
        /// </summary>
        /// <param name="entity">包含更新数据的实体，必须包含有效 <c>Id</c>。</param>
        /// <returns>返回受影响的记录数（通常为 1，若记录不存在则为 0）。</returns>
        Task<int> UpdateAsync(SysDictItem entity);

        /// <summary>
        /// 批量更新字典项记录。
        /// </summary>
        /// <param name="entities">待更新的实体集合，每个实体必须包含有效 <c>Id</c>。</param>
        /// <returns>返回所有更新操作受影响的总记录数；若集合为空，则返回 0。</returns>
        Task<int> UpdateBatchAsync(IEnumerable<SysDictItem> entities);

        /// <summary>
        /// 软删除指定的字典项记录（标记 <c>DeletedAt</c> 和 <c>DeletedById</c>）。
        /// </summary>
        /// <param name="entity">要删除的实体，用于获取 <c>Id</c> 和审计信息。</param>
        /// <returns>返回受影响的记录数（1 或 0）。</returns>
        Task<int> DeleteAsync(SysDictItem entity);

        /// <summary>
        /// 批量软删除字典项记录，并记录操作人。
        /// </summary>
        /// <param name="ids">要删除的记录 ID 列表。</param>
        /// <param name="operatorId">执行删除操作的用户 ID，用于审计。</param>
        /// <returns>返回成功标记为删除的记录数量；若 ID 列表为空，则返回 0。</returns>
        Task<int> DeleteBatchAsync(IEnumerable<long> ids, long operatorId);

        /// <summary>
        /// 根据主键获取单条未删除的字典项记录。
        /// </summary>
        /// <param name="id">记录的唯一标识符。</param>
        /// <returns>匹配的实体；若未找到或已删除，则返回 <c>null</c>。</returns>
        Task<SysDictItem> GetAsync(long id);

        /// <summary>
        /// 查询未删除的字典项记录，支持按标签模糊搜索和状态精确筛选。
        /// </summary>
        /// <param name="label">字典项标签，用于**模糊搜索**（可选）。不传则忽略该条件。</param>
        /// <param name="status">字典项状态，用于**精确匹配**（可选）。不传则忽略该条件。</param>
        /// <returns>匹配条件的未删除记录集合（可能为空）。</returns>
        Task<List<SysDictItem>> GetAllAsync(string? label = null, string? status = null);

        /// <summary>
        /// 分页查询未删除的字典项记录，支持标签、类型模糊搜索和状态精确筛选。
        /// </summary>
        /// <param name="pageNumber">页码，从 1 开始。</param>
        /// <param name="pageSize">每页记录数。</param>
        /// <param name="label">字典项标签，用于模糊搜索（可选）。</param>
        /// <param name="value">字典项实际值，用于模糊搜索（可选）。</param>
        /// <param name="status">字典项状态，用于精确匹配（可选）。</param>
        /// <param name="categoryCode">字典分类编码，精确匹配（可选）。</param>
        /// <param name="orderByField">排序字段（可选）。</param>
        /// <param name="orderByType">排序方式（可选）。</param>
        /// <returns>
        /// 元组包含分页结果：
        /// <list type="bullet">
        ///   <item><description><c>Data</c>：当前页的数据列表（可能为空）。</description></item>
        ///   <item><description><c>TotalCount</c>：满足查询条件的总记录数。</description></item>
        /// </list>
        /// </returns>
        Task<(List<SysDictItem> Data, int TotalCount)> GetPagedAsync(
            int pageNumber,
            int pageSize,
            string? label = null,
            string? value = null,
            string? status = null,
            string? categoryCode = null,
            string? orderByField = null,
            string? orderByType = null);

        /// <summary>
        /// 根据 ID 列表批量获取未删除的字典项实体。
        /// </summary>
        /// <param name="ids">字典项 ID 列表。</param>
        /// <returns>返回匹配的实体列表（可能为空）。</returns>
        Task<List<SysDictItem>> GetByIdsAsync(IEnumerable<long> ids);

        /// <summary>
        /// 根据字典项实际值获取唯一未删除的记录。
        /// </summary>
        /// <param name="value">字典项实际值（如 0），用于**精确匹配**。</param>
        /// <returns>匹配的实体；若未找到或已删除，则返回 <c>null</c>。由于 <c>Value</c> 唯一，最多返回一条。</returns>
        Task<SysDictItem?> GetByValueAsync(string value);

        /// <summary>
        /// 根据字典项标签获取唯一未删除的记录。
        /// </summary>
        /// <param name="label">字典项的显示标签（如“男”），用于**精确匹配**。</param>
        /// <returns>匹配的实体；若未找到或已删除，则返回 <c>null</c>。由于 <c>Label</c> 唯一，最多返回一条。</returns>
        Task<SysDictItem?> GetByLabelAsync(string label);

        /// <summary>
        /// 根据字典分类编码查询未删除的字典项记录。
        /// </summary>
        /// <param name="categoryCode">字典分类编码。</param>
        /// <returns>匹配条件的未删除记录集合（可能为空）。</returns>
        Task<List<SysDictItem>> GetAllByCategoryCodeAsync(string categoryCode);

        /// <summary>
        /// 根据字典分类编码，批量软删除字典项记录，并记录操作人。
        /// </summary>
        /// <param name="categoryCode">要删除的记录 字典分类编码。</param>
        /// <param name="operatorId">执行删除操作的用户 ID，用于审计。</param>
        /// <returns>返回成功标记为删除的记录数量；若 字典分类编码 列表为空，则返回 0。</returns>
        Task<int> DeleteByCategoryCodeAsync(string categoryCode, long operatorId);

        /// <summary>
        /// 根据字典分类编码列表，批量软删除字典项记录，并记录操作人。
        /// </summary>
        /// <param name="categoryCodes">要删除的记录 字典分类编码 列表。</param>
        /// <param name="operatorId">执行删除操作的用户 ID，用于审计。</param>
        /// <returns>返回成功标记为删除的记录数量；若 字典分类编码 列表为空，则返回 0。</returns>
        Task<int> DeleteByCategoryCodesAsync(IEnumerable<string> categoryCodes, long operatorId);
    }
}