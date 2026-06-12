using Bedrock.Core.Entities;

namespace Bedrock.Application.Interfaces
{
    /// <summary>
    /// 交易分类仓储接口。
    /// <para>
    /// 支持软删除（通过 DeletedAt 字段标记），所有查询默认过滤已删除记录。
    /// 业务规则：UserId 和 Name 保证全局唯一。
    /// </para>
    /// </summary>
    public interface ITransactionCategoryRepository
    {
        /// <summary>
        /// 创建一条新的交易分类记录。
        /// </summary>
        /// <param name="entity">待创建的实体。主键由数据库生成。</param>
        /// <returns>返回插入后生成的主键 ID。</returns>
        Task<long> CreateAsync(TransactionCategory entity);

        /// <summary>
        /// 批量创建交易分类记录。
        /// </summary>
        /// <param name="entities">待创建的实体集合。</param>
        /// <returns>返回成功插入的记录数量；若集合为空，则返回 0。</returns>
        Task<int> CreateBatchAsync(IEnumerable<TransactionCategory> entities);

        /// <summary>
        /// 更新一条交易分类记录。
        /// </summary>
        /// <param name="entity">包含更新数据的实体，必须包含有效 Id。</param>
        /// <returns>返回受影响的记录数（通常为 1，若记录不存在则为 0）。</returns>
        Task<int> UpdateAsync(TransactionCategory entity);

        /// <summary>
        /// 批量更新交易分类记录。
        /// </summary>
        /// <param name="entities">待更新的实体集合，每个实体必须包含有效 <c>Id</c>。</param>
        /// <returns>返回所有更新操作受影响的总记录数；若集合为空，则返回 0。</returns>
        Task<int> UpdateBatchAsync(IEnumerable<TransactionCategory> entities);

        /// <summary>
        /// 软删除指定的交易分类记录（标记 DeletedAt 和 DeletedById）。
        /// </summary>
        /// <param name="entity">要删除的实体，用于获取 Id 和审计信息。</param>
        /// <returns>返回受影响的记录数（1 或 0）。</returns>
        Task<int> DeleteAsync(TransactionCategory entity);

        /// <summary>
        /// 批量软删除交易分类记录，并记录操作人。
        /// </summary>
        /// <param name="ids">要删除的记录 ID 列表。</param>
        /// <param name="operatorId">执行删除操作的交易分类 ID，用于审计。</param>
        /// <returns>返回成功标记为删除的记录数量；若 ID 列表为空，则返回 0。</returns>
        Task<int> DeleteBatchAsync(IEnumerable<long> ids, long operatorId);

        /// <summary>
        /// 根据主键获取单条未删除的交易分类记录。
        /// </summary>
        /// <param name="id">记录的唯一标识符。</param>
        /// <returns>匹配的实体；若未找到或已删除，则返回 null。</returns>
        Task<TransactionCategory> GetAsync(long id);

        /// <summary>
        /// 查询未删除的交易分类记录，支持按名称模糊搜索和UserId精确搜索。
        /// </summary>
        /// <param name="name">交易分类名称，用于模糊搜索（可选）。</param>
        /// <param name="userId">交易分类UserId，用于精确搜索（可选）。</param>
        /// <returns>匹配条件的未删除记录集合（可能为空）。</returns>
        Task<List<TransactionCategory>> GetAllAsync(string? name = null, long? userId = null);

        /// <summary>
        /// 分页查询未删除的交易分类记录，支持名称模糊搜索和UserId精确搜索。
        /// </summary>
        /// <param name="pageNumber">页码，从 1 开始。</param>
        /// <param name="pageSize">每页记录数。</param>
        /// <param name="name">交易分类名称，用于模糊搜索（可选）。</param>
        /// <param name="userId">交易分类UserId，用于精确搜索（可选）。</param>
        /// <param name="orderByField">排序字段（可选）。</param>
        /// <param name="orderByType">排序方式（可选）。</param>
        /// <returns>
        /// 元组包含分页结果：
        /// - Data：当前页的数据列表（可能为空）。
        /// - TotalCount：满足查询条件的总记录数。
        /// </returns>
        Task<(List<TransactionCategory> Data, int TotalCount)> GetPagedAsync(
            int pageNumber,
            int pageSize,
            string? name = null,
            long? userId = null,
            string? orderByField = null,
            string? orderByType = null);

        /// <summary>
        /// 根据 ID 列表批量获取未删除的交易分类实体。
        /// </summary>
        /// <param name="ids">交易分类 ID 列表。</param>
        /// <returns>返回匹配的实体列表（可能为空）。</returns>
        Task<List<TransactionCategory>> GetByIdsAsync(IEnumerable<long> ids);

        /// <summary>
        /// 根据交易分类UserId和名称获取一条记录。
        /// </summary>
        /// <param name="userId">交易分类UserId（如 0），精确匹配。</param>
        /// <param name="name">交易分类的名称（如“测试”），用于精确匹配。</param>
        /// <returns>匹配的实体；若未找到或已删除，则返回 null。由于 UserId 和 Name 唯一，最多返回一条。</returns>
        Task<TransactionCategory?> GetByUserIdAndNameAsync(long userId, string name);

        /// <summary>
        /// 根据UserId，批量软删除交易分类记录，并记录操作人。
        /// </summary>
        /// <param name="userId">要删除的记录 UserId。</param>
        /// <param name="operatorId">执行删除操作的用户 ID，用于审计。</param>
        /// <returns>返回成功标记为删除的记录数量；若 UserId 列表为空，则返回 0。</returns>
        Task<int> DeleteByUserIdAsync(long userId, long operatorId);

        /// <summary>
        /// 根据UserId列表，批量软删除交易分类记录，并记录操作人。
        /// </summary>
        /// <param name="userIds">要删除的记录 UserId 列表。</param>
        /// <param name="operatorId">执行删除操作的用户 ID，用于审计。</param>
        /// <returns>返回成功标记为删除的记录数量；若 UserId 列表为空，则返回 0。</returns>
        Task<int> DeleteByUserIdsAsync(IEnumerable<long> userIds, long operatorId);

    }
}