using Bedrock.Application.DataTransferObjects;

namespace Bedrock.Application.Interfaces
{
    /// <summary>
    /// 交易服务接口，定义了与交易实体相关的业务逻辑操作。
    /// </summary>
    public interface ITransaction2Service
    {
        /// <summary>
        /// 根据ID获取单个交易记录。
        /// </summary>
        /// <param name="id">交易的唯一标识符。</param>
        /// <returns>返回与ID匹配的交易数据传输对象。</returns>
        Task<TransactionDto> GetByIdAsync(Guid id);

        /// <summary>
        /// 根据ID获取单个交易摘要信息。
        /// </summary>
        /// <param name="id">交易的唯一标识符。</param>
        /// <returns>返回与ID匹配的交易数据传输对象。</returns>
        Task<TransactionDto> GetSummaryByIdAsync(Guid id);

        /// <summary>
        /// 获取所有交易摘要信息，可选根据账本ID和日期范围过滤。
        /// </summary>
        /// <param name="ledgerId">可选的账本唯一标识符，用于过滤特定账本的交易。</param>
        /// <param name="startDate">可选的起始日期，用于过滤交易。</param>
        /// <param name="endDate">可选的结束日期，用于过滤交易。</param>
        /// <returns>返回交易数据传输对象的集合。</returns>
        Task<IEnumerable<TransactionDto>> GetAllSummaryAsync(Guid? ledgerId, DateTime? startDate, DateTime? endDate);

        /// <summary>
        /// 创建新的交易记录。
        /// </summary>
        /// <param name="createTransactionDto">包含创建信息的交易数据传输对象。</param>
        /// <param name="createdById">创建者的唯一标识符。</param>
        /// <returns>返回新创建交易的唯一标识符。</returns>
        Task<Guid> CreateAsync(CreateTransactionDto createTransactionDto, Guid createdById);

        /// <summary>
        /// 更新现有的交易记录。
        /// </summary>
        /// <param name="updateTransactionDto">包含更新信息的交易数据传输对象。</param>
        /// <param name="updatedById">更新者的唯一标识符。</param>
        /// <returns>返回被更新交易的唯一标识符。</returns>
        Task<Guid> UpdateAsync(UpdateTransactionDto updateTransactionDto, Guid updatedById);

        /// <summary>
        /// 删除指定的交易记录。
        /// </summary>
        /// <param name="id">交易的唯一标识符。</param>
        /// <param name="deletedById">删除者的唯一标识符。</param>
        /// <returns>返回被删除交易的唯一标识符。</returns>
        Task<Guid> DeleteAsync(Guid id, Guid deletedById);

        /// <summary>
        /// 分页获取交易摘要信息，支持账本ID、分类ID和日期范围过滤。
        /// </summary>
        /// <param name="pageNumber">当前页码（从1开始）。</param>
        /// <param name="pageSize">每页显示的记录数。</param>
        /// <param name="ledgerId">可选的账本唯一标识符，用于过滤特定账本的交易。</param>
        /// <param name="categoryId">可选的分类唯一标识符，用于过滤特定分类的交易。</param>
        /// <param name="startDate">可选的起始日期，用于过滤交易。</param>
        /// <param name="endDate">可选的结束日期，用于过滤交易。</param>
        /// <returns>返回分页结果，包含交易数据传输对象和分页信息。</returns>
        Task<PaginationResult<TransactionDto>> GetPagedSummaryAsync(int pageNumber, int pageSize, Guid? ledgerId = null, Guid? categoryId = null, DateTime? startDate = null, DateTime? endDate = null);

        /// <summary>
        /// 获取指定账本在日期范围内的前N笔最高支出交易摘要。
        /// </summary>
        /// <param name="ledgerId">账本的唯一标识符。</param>
        /// <param name="startDate">起始日期。</param>
        /// <param name="endDate">结束日期。</param>
        /// <param name="top">需要返回的最高支出交易数量。</param>
        /// <returns>返回交易数据传输对象的集合。</returns>
        Task<IEnumerable<TransactionDto>> GetTopExpenseSummaryAsync(Guid ledgerId, DateTime startDate, DateTime endDate, int top);

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
        /// <returns>返回最早的交易数据传输对象。</returns>
        Task<TransactionDto> GetEarliestByLedgerIdAsync(Guid ledgerId);
    }
}