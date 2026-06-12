using Bedrock.Application.Interfaces;
using Bedrock.Core.Entities;
using Bedrock.Application.DataTransferObjects;
using SqlSugar;

namespace Bedrock.Application.Services
{
    /// <summary>
    ///服务层实现，通过调用仓储层完成业务逻辑。
    /// </summary>
    public class TransactionService2
    {
        private readonly ISqlSugarClient _db;
        private readonly ITransactionRepository _transactionRepository;

        /// <summary>
        /// 构造函数注入依赖项。
        /// </summary>
        /// <param name="db">数据库上下文。</param>
        /// <param name="transactionRepository">交易仓储接口。</param>
        /// <param name="budgetRepository">预算仓储接口。</param>
        public TransactionService2(ISqlSugarClient db, ITransactionRepository transactionRepository)
        {
            _db = db;
            _transactionRepository = transactionRepository;
        }

        ///// <summary>
        ///// 根据ID获取单个交易记录。
        ///// </summary>
        ///// <param name="id">交易记录的唯一标识符。</param>
        ///// <returns>返回与ID匹配的交易数据传输对象。</returns>
        //public async Task<TransactionDto> GetByIdAsync(Guid id)
        //{
        //    var transaction = await _transactionRepository.GetByIdAsync(id);
        //    if (transaction == null)
        //    {
        //        return null;
        //    }

        //    return new TransactionDto
        //    {
        //        Id = transaction.Id,
        //        LedgerId = transaction.LedgerId,
        //        CategoryId = transaction.CategoryId,
        //        Amount = transaction.Amount,
        //        Description = transaction.Description,
        //        Date = transaction.Date,
        //        CreatedAt = transaction.CreatedAt,
        //        UpdatedAt = transaction.UpdatedAt,
        //    };
        //}

        ///// <summary>
        ///// 获取包含详细信息的单个交易记录。
        ///// </summary>
        ///// <param name="id">交易记录的唯一标识符。</param>
        ///// <returns>返回与ID匹配的交易摘要数据传输对象。</returns>
        //public async Task<TransactionDto> GetSummaryByIdAsync(Guid id)
        //{
        //    var transactionSummary = await _transactionRepository.GetSummaryByIdAsync(id);
        //    if (transactionSummary == null)
        //    {
        //        return null;
        //    }

        //    return new TransactionDto
        //    {
        //        Id = transactionSummary.Id,
        //        LedgerId = transactionSummary.LedgerId,
        //        LedgerName = transactionSummary.LedgerName,
        //        CategoryId = transactionSummary.CategoryId,
        //        CategoryName = transactionSummary.CategoryName,
        //        CategoryType = transactionSummary.CategoryType,
        //        CategoryIcon = transactionSummary.CategoryIcon,
        //        Amount = transactionSummary.Amount,
        //        Description = transactionSummary.Description,
        //        Date = transactionSummary.Date,
        //        CreatedAt = transactionSummary.CreatedAt,
        //        UpdatedAt = transactionSummary.UpdatedAt,
        //    };
        //}

        ///// <summary>
        ///// 获取所有交易记录的摘要信息，支持按账本ID和日期范围筛选。
        ///// </summary>
        ///// <param name="ledgerId">可选，账本的唯一标识符。</param>
        ///// <param name="startDate">可选，开始日期。</param>
        ///// <param name="endDate">可选，结束日期。</param>
        ///// <returns>返回符合条件的交易摘要数据传输对象集合。</returns>
        //public async Task<IEnumerable<TransactionDto>> GetAllSummaryAsync(Guid? ledgerId, DateTime? startDate, DateTime? endDate)
        //{
        //    var transactionList = await _transactionRepository.GetAllSummaryAsync(ledgerId, startDate, endDate);
        //    return transactionList.Select(transactionSummary => new TransactionDto
        //    {
        //        Id = transactionSummary.Id,
        //        LedgerId = transactionSummary.LedgerId,
        //        LedgerName = transactionSummary.LedgerName,
        //        CategoryId = transactionSummary.CategoryId,
        //        CategoryName = transactionSummary.CategoryName,
        //        CategoryType = transactionSummary.CategoryType,
        //        CategoryIcon = transactionSummary.CategoryIcon,
        //        Amount = transactionSummary.Amount,
        //        Description = transactionSummary.Description,
        //        Date = transactionSummary.Date,
        //        CreatedAt = transactionSummary.CreatedAt,
        //        UpdatedAt = transactionSummary.UpdatedAt,
        //    }).ToList();
        //}

        ///// <summary>
        ///// 创建新的交易记录。
        ///// </summary>
        ///// <param name="createTransactionDto">包含创建信息的交易数据传输对象。</param>
        ///// <param name="createdById">创建者的唯一标识符。</param>
        ///// <returns>返回新创建交易记录的唯一标识符。</returns>
        //public async Task<Guid> CreateAsync(CreateTransactionDto createTransactionDto, Guid createdById)
        //{
        //    var transaction = new Transaction
        //    {
        //        Id = Guid.NewGuid(),
        //        LedgerId = createTransactionDto.LedgerId,
        //        CategoryId = createTransactionDto.CategoryId,
        //        Amount = createTransactionDto.Amount,
        //        Description = createTransactionDto.Description,
        //        Date = createTransactionDto.Date,
        //        CreatedById = createdById,
        //        CreatedAt = DateTime.UtcNow
        //    };

        //    // 开启事务
        //    _db.Ado.BeginTran();
        //    try
        //    {
        //        await _transactionRepository.CreateAsync(transaction);

        //        // 提交事务
        //        _db.Ado.CommitTran();

        //        return transaction.Id;
        //    }
        //    catch (Exception ex)
        //    {
        //        // 回滚事务
        //        _db.Ado.RollbackTran();
        //        throw; // 或者处理异常，例如记录日志等
        //    }
        //}

        ///// <summary>
        ///// 更新现有的交易记录。
        ///// </summary>
        ///// <param name="updateTransactionDto">包含更新信息的交易数据传输对象。</param>
        ///// <param name="updatedById">更新者的唯一标识符。</param>
        ///// <returns>返回被更新交易记录的唯一标识符。</returns>
        //public async Task<Guid> UpdateAsync(UpdateTransactionDto updateTransactionDto, Guid updatedById)
        //{
        //    // 开启事务
        //    _db.Ado.BeginTran();
        //    try
        //    {
        //        var transaction = await _transactionRepository.GetByIdAsync(updateTransactionDto.Id);
        //        transaction.CategoryId = updateTransactionDto.CategoryId;
        //        transaction.Amount = updateTransactionDto.Amount;
        //        transaction.Description = updateTransactionDto.Description;
        //        transaction.Date = updateTransactionDto.Date;
        //        transaction.UpdatedAt = DateTime.UtcNow;
        //        transaction.UpdatedById = updatedById;

        //        await _transactionRepository.UpdateAsync(transaction);

        //        // 提交事务
        //        _db.Ado.CommitTran();

        //        return transaction.Id;
        //    }
        //    catch (Exception ex)
        //    {
        //        // 回滚事务
        //        _db.Ado.RollbackTran();
        //        throw; // 或者处理异常，例如记录日志等
        //    }
        //}

        ///// <summary>
        ///// 删除指定的交易记录。
        ///// </summary>
        ///// <param name="id">要删除的交易记录的唯一标识符。</param>
        ///// <param name="deletedById">删除者的唯一标识符。</param>
        ///// <returns>返回被删除交易记录的唯一标识符。</returns>
        //public async Task<Guid> DeleteAsync(Guid id, Guid deletedById)
        //{
        //    // 开启事务
        //    _db.Ado.BeginTran();
        //    try
        //    {
        //        var transaction = await _transactionRepository.GetByIdAsync(id);
        //        if (transaction == null)
        //        {
        //            throw new InvalidOperationException("交易不存在");
        //        }
        //        transaction.DeletedAt = DateTime.UtcNow;
        //        transaction.DeletedById = deletedById;

        //        await _transactionRepository.DeleteAsync(transaction);

        //        // 提交事务
        //        _db.Ado.CommitTran();

        //        return transaction.Id;
        //    }
        //    catch (Exception ex)
        //    {
        //        // 回滚事务
        //        _db.Ado.RollbackTran();
        //        throw; // 或者处理异常，例如记录日志等
        //    }
        //}

        ///// <summary>
        ///// 分页获取交易记录的摘要信息，支持按账本ID、分类ID和日期范围筛选。
        ///// </summary>
        ///// <param name="pageNumber">当前页码（从1开始）。</param>
        ///// <param name="pageSize">每页显示的记录数。</param>
        ///// <param name="ledgerId">可选，账本的唯一标识符。</param>
        ///// <param name="categoryId">可选，分类的唯一标识符。</param>
        ///// <param name="startDate">可选，开始日期。</param>
        ///// <param name="endDate">可选，结束日期。</param>
        ///// <returns>返回分页结果，包含当前页的交易摘要数据和分页信息。</returns>
        //public async Task<PaginationResult<TransactionDto>> GetPagedSummaryAsync(int pageNumber, int pageSize, Guid? ledgerId = null, Guid? categoryId = null, DateTime? startDate = null, DateTime? endDate = null)
        //{
        //    var (data, totalCount) = await _transactionRepository.GetPagedSummaryAsync(pageNumber, pageSize, ledgerId, categoryId, startDate, endDate);
        //    var finallyData = data.Select(transactionSummary => new TransactionDto
        //    {
        //        Id = transactionSummary.Id,
        //        LedgerId = transactionSummary.LedgerId,
        //        LedgerName = transactionSummary.LedgerName,
        //        CategoryId = transactionSummary.CategoryId,
        //        CategoryName = transactionSummary.CategoryName,
        //        CategoryType = transactionSummary.CategoryType,
        //        CategoryIcon = transactionSummary.CategoryIcon,
        //        Amount = transactionSummary.Amount,
        //        Description = transactionSummary.Description,
        //        Date = transactionSummary.Date,
        //        CreatedAt = transactionSummary.CreatedAt,
        //        UpdatedAt = transactionSummary.UpdatedAt,
        //    }).ToList();
        //    // 计算总页数
        //    int totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

        //    // 封装为 PaginationResult
        //    return new PaginationResult<TransactionDto>(
        //        items: finallyData,
        //        totalPages: totalPages,
        //        totalItems: totalCount,
        //        currentPage: pageNumber,
        //        pageSize: pageSize);
        //}

        ///// <summary>
        ///// 获取指定账本的前N笔最高支出交易记录。
        ///// </summary>
        ///// <param name="ledgerId">账本的唯一标识符。</param>
        ///// <param name="startDate">开始日期。</param>
        ///// <param name="endDate">结束日期。</param>
        ///// <param name="top">返回的记录数。</param>
        ///// <returns>返回符合条件的交易摘要数据传输对象集合。</returns>
        //public async Task<IEnumerable<TransactionDto>> GetTopExpenseSummaryAsync(Guid ledgerId, DateTime startDate, DateTime endDate, int top)
        //{
        //    var transactionList = await _transactionRepository.GetTopExpenseSummaryAsync(ledgerId, startDate, endDate, top);
        //    return transactionList.Select(transactionSummary => new TransactionDto
        //    {
        //        Id = transactionSummary.Id,
        //        LedgerId = transactionSummary.LedgerId,
        //        LedgerName = transactionSummary.LedgerName,
        //        CategoryId = transactionSummary.CategoryId,
        //        CategoryName = transactionSummary.CategoryName,
        //        CategoryType = transactionSummary.CategoryType,
        //        CategoryIcon = transactionSummary.CategoryIcon,
        //        Amount = transactionSummary.Amount,
        //        Description = transactionSummary.Description,
        //        Date = transactionSummary.Date,
        //        CreatedAt = transactionSummary.CreatedAt,
        //        UpdatedAt = transactionSummary.UpdatedAt,
        //    }).ToList();
        //}

        ///// <summary>
        ///// 获取指定账本在指定时间段内的分类总金额排名。
        ///// </summary>
        ///// <param name="ledgerId">账本的唯一标识符。</param>
        ///// <param name="startDate">开始日期。</param>
        ///// <param name="endDate">结束日期。</param>
        ///// <param name="categoryType">可选，分类类型。</param>
        ///// <returns>返回分类总金额排名的数据传输对象集合。</returns>
        //public async Task<IEnumerable<CategoryTotalAmountDto>> GetCategoryTotalAmountRankingAsync(Guid ledgerId, DateTime startDate, DateTime endDate, string? categoryType = null)
        //{
        //    var queryResults = await _transactionRepository.GetCategoryTotalAmountRankingAsync(ledgerId, startDate, endDate, categoryType);
        //    return queryResults;
        //}

        ///// <summary>
        ///// 获取指定账本在指定年份的月度总金额汇总。
        ///// </summary>
        ///// <param name="ledgerId">账本的唯一标识符。</param>
        ///// <param name="categoryType">分类类型。</param>
        ///// <param name="year">年份。</param>
        ///// <returns>返回月度总金额汇总的数据传输对象集合。</returns>
        //public async Task<IEnumerable<MonthlyTotalAmountDto>> GetMonthlyTotalsAsync(Guid ledgerId, string categoryType, int year)
        //{
        //    var queryResults = await _transactionRepository.GetMonthlyTotalsAsync(ledgerId, categoryType, year);
        //    return queryResults;
        //}
        ///// <summary>
        ///// 获取指定账本在当前时间的财务汇总信息。
        ///// </summary>
        ///// <param name="ledgerId">账本的唯一标识符。</param>
        ///// <param name="datetimeNow">当前时间。</param>
        ///// <returns>返回账本的财务汇总数据传输对象。</returns>
        //public async Task<FinancialSummaryDto> GetFinancialSummariesAsync(Guid ledgerId, DateTime datetimeNow)
        //{
        //    var queryResults = await _transactionRepository.GetFinancialSummariesAsync(ledgerId, datetimeNow);
        //    return queryResults;
        //}

        ///// <summary>
        ///// 获取指定账本在指定日期范围内的总金额汇总信息。
        ///// </summary>
        ///// <param name="ledgerId">账本的唯一标识符。</param>
        ///// <param name="startDate">开始日期。</param>
        ///// <param name="endDate">结束日期。</param>
        ///// <returns>返回日期范围内的总金额汇总数据传输对象。</returns>
        //public async Task<DateRangeTotalAmountDto> GetDateRangeTotalAmountAsync(Guid ledgerId, DateTime startDate, DateTime endDate)
        //{
        //    var queryResults = await _transactionRepository.GetDateRangeTotalAmountAsync(ledgerId, startDate, endDate);
        //    return queryResults;
        //}

        ///// <summary>
        ///// 获取指定账本中最早的交易记录。
        ///// </summary>
        ///// <param name="ledgerId">账本的唯一标识符。</param>
        ///// <returns>返回最早交易记录的数据传输对象。如果不存在记录，则返回 null。</returns>
        //public async Task<TransactionDto> GetEarliestByLedgerIdAsync(Guid ledgerId)
        //{
        //    var transactionSummary = await _transactionRepository.GetEarliestByLedgerIdAsync(ledgerId);
        //    if (transactionSummary == null)
        //    {
        //        return null;
        //    }

        //    return new TransactionDto
        //    {
        //        Id = transactionSummary.Id,
        //        LedgerId = transactionSummary.LedgerId,
        //        LedgerName = transactionSummary.LedgerName,
        //        CategoryId = transactionSummary.CategoryId,
        //        CategoryName = transactionSummary.CategoryName,
        //        CategoryType = transactionSummary.CategoryType,
        //        CategoryIcon = transactionSummary.CategoryIcon,
        //        Amount = transactionSummary.Amount,
        //        Description = transactionSummary.Description,
        //        Date = transactionSummary.Date,
        //        CreatedAt = transactionSummary.CreatedAt,
        //        UpdatedAt = transactionSummary.UpdatedAt,
        //    };
        //}

    }
}