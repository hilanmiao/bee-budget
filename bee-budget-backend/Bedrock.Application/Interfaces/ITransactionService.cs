using Bedrock.Application.DataTransferObjects;

namespace Bedrock.Application.Interfaces
{
    /// <summary>
    /// 交易服务接口。
    /// <para>
    /// 支持软删除（通过 DeletedAt 字段标记），所有查询默认过滤已删除记录。
    /// 业务规则：Id 保证全局唯一。
    /// </para>
    /// </summary>
    public interface ITransactionService
    {
        /// <summary>
        /// 创建新的交易。
        /// </summary>
        /// <param name="createDto">创建数据传输对象，包含 Amount、Date 等字段。</param>
        /// <param name="operatorId">操作人 ID，用于审计。</param>
        /// <returns>返回新创建记录的主键 ID。</returns>
        /// <exception cref="ArgumentException">当账本、交易分类已删除时抛出。</exception>
        /// <exception cref="ArgumentNullException">当 createDto 为 null 时抛出。</exception>
        Task<long> CreateAsync(CreateTransactionDto createDto, long operatorId);

        /// <summary>
        /// 更新指定的交易。
        /// </summary>
        /// <param name="updateDto">更新数据传输对象，必须包含有效 ID。</param>
        /// <param name="operatorId">操作人 ID。</param>
        /// <returns>返回被更新的记录 ID。</returns>
        /// <exception cref="ArgumentException">当记录不存在、已删除或更新后违反唯一性约束时抛出。</exception>
        /// <exception cref="ArgumentNullException">当 updateDto 为 null 时抛出。</exception>
        /// <exception cref="InvalidOperationException">当数据库更新未影响任何行时抛出（并发冲突）。</exception>
        Task<long> UpdateAsync(UpdateTransactionDto updateDto, long operatorId);

        /// <summary>
        /// 软删除指定的交易。
        /// </summary>
        /// <param name="id">要删除的交易 ID。</param>
        /// <param name="operatorId">操作人 ID。</param>
        /// <returns>返回被删除的记录 ID。</returns>
        /// <exception cref="ArgumentException">当记录不存在或已被删除时抛出。</exception>
        Task<long> DeleteAsync(long id, long operatorId);

        /// <summary>
        /// 批量软删除交易。
        /// </summary>
        /// <param name="ids">要删除的交易 ID 列表。</param>
        /// <param name="operatorId">操作人 ID。</param>
        /// <returns>返回成功标记为删除的记录数量；若 ID 列表为空则返回 0。</returns>
        /// <exception cref="ArgumentException">当部分 ID 不存在或已被删除时可能抛出。</exception>
        Task<int> DeleteBatchAsync(IEnumerable<long> ids, long operatorId);

        /// <summary>
        /// 更新交易的状态。
        /// </summary>
        /// <param name="id">要更新的交易 ID。</param>
        /// <param name="status">状态</param>
        /// <param name="operatorId">操作人 ID。</param>
        /// <returns>返回被更新的记录 ID。</returns>
        Task<long> ChangeStatusAsync(long id, string status, long operatorId);

        /// <summary>
        /// 根据主键获取单条未删除的交易详情。
        /// </summary>
        /// <param name="id">交易唯一标识。</param>
        /// <returns>返回匹配的 DTO；若未找到或已删除，则返回 null。</returns>
        Task<TransactionSummary?> GetSummaryAsync(long id);

        /// <summary>
        /// 查询未删除的交易记录，支持按名称和描述模糊搜索，用户Id、交易账本Id、交易分类Id、状态精确搜索，日期范围搜索。
        /// </summary>
        /// <param name="userId">用户Id。</param>
        /// <param name="ledgerId">交易账本Id，用于精确搜索（可选）。</param>
        /// <param name="transactionCategoryId">交易分类Id，用于精确搜索（可选）。</param>
        /// <param name="status">交易状态，用于模糊搜索（可选）。</param>
        /// <param name="startDate">交易开始日期，用于日期范围搜索（可选）。</param>
        /// <param name="endDate">交易结束日期，用于日期范围搜索（可选）。</param>
        /// <param name="description">交易描述，用于模糊搜索（可选）。</param>
        /// <returns>返回匹配条件的 DTO 列表（可能为空）。</returns>
        Task<List<TransactionSummary>> GetAllSummaryAsync(
            long userId,
            long? ledgerId = null,
            long? transactionCategoryId = null,
            string? status = null,
            DateTime? startDate = null,
            DateTime? endDate = null,
            string? description = null);

        /// <summary>
        /// 分页查询未删除的交易记录，支持按名称和描述模糊搜索，用户Id、交易账本Id、交易分类Id、状态精确搜索，日期范围搜索。
        /// </summary>
        /// <param name="pageNumber">页码，从 1 开始。</param>
        /// <param name="pageSize">每页记录数。</param>
        /// <param name="userId">用户Id。</param>
        /// <param name="ledgerId">交易账本Id，用于精确搜索（可选）。</param>
        /// <param name="transactionCategoryId">交易分类Id，用于精确搜索（可选）。</param>
        /// <param name="status">交易状态，用于模糊搜索（可选）。</param>
        /// <param name="startDate">交易开始日期，用于日期范围搜索（可选）。</param>
        /// <param name="endDate">交易结束日期，用于日期范围搜索（可选）。</param>
        /// <param name="description">交易描述，用于模糊搜索（可选）。</param>
        /// <param name="orderByField">排序字段（可选）。</param>
        /// <param name="orderByType">排序方式（可选）。</param>
        /// <returns>返回分页结果对象，包含数据和分页元信息。</returns>
        Task<PaginationResult<TransactionSummary>> GetPagedSummaryAsync(
            int pageNumber,
            int pageSize,
            long userId,
            long? ledgerId = null,
            long? transactionCategoryId = null,
            string? status = null,
            DateTime? startDate = null,
            DateTime? endDate = null,
            string? description = null,
            string? orderByField = null,
            string? orderByType = null);

        /// <summary>
        /// 获取指定账本的实时数据快照（包含日/周/月/年收支统计）。
        /// <param name="ledgerId">要查询的账本Id</param>
        /// <returns>返回账本实时数据快照 Dto，即使未查询到数据也不会返回 null</returns>
        Task<LedgerSnapshotDto> GetLedgerSnapshotAsync(long ledgerId);

        /// <summary>
        /// 获取本月支出分类排名 TopN
        /// </summary>
        /// <param name="ledgerId">账本Id</param>
        /// <param name="top">排名前 N 的数量</param>
        /// <returns>按金额降序排列的前 N 笔支出交易的分类 Dto 列表。若无数据返回空列表。</returns>
        Task<List<TransactionCategoryStatDto>> GetMonthlyExpenseTransactionCategoryTopNAsync(long ledgerId, int top);

        /// <summary>
        /// 获取本月收入分类排名 TopN
        /// </summary>
        /// <param name="ledgerId">账本Id</param>
        /// <param name="top">排名前 N 的数量</param>
        /// <returns>按金额降序排列的前 N 笔收入交易的 Dto 列表。若无数据返回空列表。</returns>
        Task<List<TransactionCategoryStatDto>> GetMonthlyIncomeTransactionCategoryTopNAsync(long ledgerId, int top);

        /// <summary>
        /// 获取本月支出分类构成
        /// </summary>
        /// <param name="ledgerId">账本Id</param>
        /// <returns>按金额降序排列的本月所有交易的分类 Dto 列表。若无数据返回空列表。</returns>
        Task<List<TransactionCategoryStatDto>> GetMonthlyExpenseTransactionCategoryAsync(long ledgerId);

        /// <summary>
        /// 获取本月单笔金额最大的前 N 笔支出交易记录。
        /// </summary>
        /// <param name="ledgerId">账本Id</param>
        /// <param name="top">排名前 N 的数量</param>
        /// <returns>按金额降序排列的前 N 笔交易 Dto 列表。若无数据返回空列表。</returns>
        Task<List<TransactionSummary>> GetMonthlyExpenseTransactionTopNAsync(long ledgerId, int top);

        /// <summary>
        /// 统计指定账本本年度每月的收入与支出金额。
        /// </summary>
        /// <param name="ledgerId">账本 ID</param>
        /// <returns>
        /// 本年 1-12 月的收支统计 Dto 列表。
        /// 注意：未来未开始的月份返回 null，已过去但无交易的月份返回 0。
        /// </returns>
        Task<List<TransactionMonthlyStatDto>> GetYearlyStatsAsync(long ledgerId);

        /// <summary>
        /// 统计指定时间范围内的收入和支出总金额。
        /// </summary>
        /// <param name="ledgerId">账本 ID</param>
        /// <param name="startDate">开始日期</param>
        /// <param name="endDate">结束日期</param>
        /// <returns>包含总收入和总支出的统计对象</returns>
        Task<TransactionTotalStatDto> GetRangeStatsAsync(long ledgerId, DateTime startDate, DateTime endDate);

        /// <summary>
        /// 获取指定账本最早的一笔交易。
        /// </summary>
        /// <param name="ledgerId">账本Id</param>
        /// <returns>返回匹配的 DTO；若未找到或已删除，则返回 null。</returns>
        Task<TransactionDto?> GetEarliestAsync(long ledgerId);

    }
}