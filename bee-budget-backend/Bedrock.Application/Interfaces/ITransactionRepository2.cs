using Bedrock.Application.DataTransferObjects;
using Bedrock.Core.Entities;

namespace Bedrock.Application.Interfaces
{
    /// <summary>
    /// 交易仓储接口，定义了对交易实体的基本数据操作。
    /// </summary>
    public interface ITransactionRepository2
    {
        /// <summary>
        /// 根据ID获取单个交易记录。
        /// </summary>
        /// <param name="id">交易的唯一标识符。</param>
        /// <returns>返回与ID匹配的交易实体对象。</returns>
        Task<Transaction> GetByIdAsync(Guid id);

        /// <summary>
        /// 根据ID获取单个交易摘要信息。
        /// </summary>
        /// <param name="id">交易的唯一标识符。</param>
        /// <returns>返回与ID匹配的交易摘要实体对象。</returns>
        Task<TransactionSummary> GetSummaryByIdAsync(Guid id);

        /// <summary>
        /// 获取所有交易摘要信息，可选根据账本ID和日期范围过滤。
        /// </summary>
        /// <param name="ledgerId">可选的账本唯一标识符，用于过滤特定账本的交易。</param>
        /// <param name="startDate">可选的起始日期，用于过滤交易。</param>
        /// <param name="endDate">可选的结束日期，用于过滤交易。</param>
        /// <returns>返回交易摘要实体对象的集合。</returns>
        Task<IEnumerable<TransactionSummary>> GetAllSummaryAsync(Guid? ledgerId, DateTime? startDate, DateTime? endDate);

        /// <summary>
        /// 创建新的交易记录。
        /// </summary>
        /// <param name="transaction">要创建的交易实体对象。</param>
        Task CreateAsync(Transaction transaction);

        /// <summary>
        /// 更新现有的交易记录。
        /// </summary>
        /// <param name="transaction">包含更新信息的交易实体对象。</param>
        Task UpdateAsync(Transaction transaction);

        /// <summary>
        /// 删除指定的交易记录。
        /// </summary>
        /// <param name="transaction">要删除的交易实体对象。</param>
        Task DeleteAsync(Transaction transaction);

        /// <summary>
        /// 分页获取交易摘要信息，支持账本ID、分类ID和日期范围过滤。
        /// </summary>
        /// <param name="pageNumber">当前页码（从1开始）。</param>
        /// <param name="pageSize">每页显示的记录数。</param>
        /// <param name="ledgerId">可选的账本唯一标识符，用于过滤特定账本的交易。</param>
        /// <param name="categoryId">可选的分类唯一标识符，用于过滤特定分类的交易。</param>
        /// <param name="startDate">可选的起始日期，用于过滤交易。</param>
        /// <param name="endDate">可选的结束日期，用于过滤交易。</param>
        /// <returns>返回分页结果，包含交易摘要数据和总记录数。</returns>
        Task<(IEnumerable<TransactionSummary> Data, int TotalCount)> GetPagedSummaryAsync(int pageNumber, int pageSize, Guid? ledgerId = null, Guid? categoryId = null, DateTime? startDate = null, DateTime? endDate = null);

        /// <summary>
        /// 获取指定账本在日期范围内的前N笔最高支出交易摘要。
        /// </summary>
        /// <param name="ledgerId">账本的唯一标识符。</param>
        /// <param name="startDate">起始日期。</param>
        /// <param name="endDate">结束日期。</param>
        /// <param name="top">需要返回的最高支出交易数量。</param>
        /// <returns>返回交易摘要实体对象的集合。</returns>
        Task<IEnumerable<TransactionSummary>> GetTopExpenseSummaryAsync(Guid ledgerId, DateTime startDate, DateTime endDate, int top);

        /// <summary>
        /// 获取指定账本在日期范围内的分类总金额排名。
        /// </summary>
        /// <param name="ledgerId">账本的唯一标识符。</param>
        /// <param name="startDate">起始日期。</param>
        /// <param name="endDate">结束日期。</param>
        /// <param name="categoryType">可选的分类类型，用于过滤分类。</param>
        /// <returns>返回分类总金额的数据传输对象集合。</returns>
        Task<IEnumerable<CategoryTotalAmountDto>> GetCategoryTotalAmountRankingAsync(Guid ledgerId, DateTime startDate, DateTime endDate, string? categoryType = null);

        /// <summary>
        /// 获取指定账本在某年内的每月总金额统计。
        /// </summary>
        /// <param name="ledgerId">账本的唯一标识符。</param>
        /// <param name="categoryType">分类类型。</param>
        /// <param name="year">统计的年份。</param>
        /// <returns>返回每月总金额的数据传输对象集合。</returns>
        Task<IEnumerable<MonthlyTotalAmountDto>> GetMonthlyTotalsAsync(Guid ledgerId, string categoryType, int year);

        /// <summary>
        /// 获取指定账本在某日期的财务汇总信息。
        /// </summary>
        /// <param name="ledgerId">账本的唯一标识符。</param>
        /// <param name="datetimeNow">指定的日期。</param>
        /// <returns>返回财务汇总的数据传输对象。</returns>
        Task<FinancialSummaryDto> GetFinancialSummariesAsync(Guid ledgerId, DateTime datetimeNow);

        /// <summary>
        /// 获取指定账本在日期范围内的总金额统计。
        /// </summary>
        /// <param name="ledgerId">账本的唯一标识符。</param>
        /// <param name="startDate">起始日期。</param>
        /// <param name="endDate">结束日期。</param>
        /// <returns>返回日期范围内总金额的数据传输对象。</returns>
        Task<DateRangeTotalAmountDto> GetDateRangeTotalAmountAsync(Guid ledgerId, DateTime startDate, DateTime endDate);

        /// <summary>
        /// 获取指定账本中最早的一笔交易摘要信息。
        /// </summary>
        /// <param name="ledgerId">账本的唯一标识符。</param>
        /// <returns>返回最早的交易摘要实体对象。</returns>
        Task<TransactionSummary> GetEarliestByLedgerIdAsync(Guid ledgerId);
    }
}