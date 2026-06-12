using Bedrock.Application.DataTransferObjects;
using Bedrock.Application.Interfaces;
using Bedrock.WebApi.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace Bedrock.WebApi.Controllers
{
    /// <summary>
    /// 控制器用于处理与 Transaction 资源相关的请求。
    /// </summary>
    [ApiController]
    [AllowAnonymous] // 默认允许匿名访问
    [Route("api/transaction")]
    public class TransactionController2 : ControllerBase
    {
        //private string loggerName = typeof(TransactionController).FullName;
        ////private static readonly ILog log = LogManager.GetLogger(typeof(TransactionController));
        //private readonly ITransactionService _transactionService;
        //private readonly IApiLogHelper _apiLogHelper;
        //private Guid tempUserId = Guid.Parse("d02fbcce-c771-47d0-bfff-cff9c7496624");


        ///// <summary>
        ///// 构造函数注入 TransactionService。
        ///// </summary>
        ///// <param name="transactionService">服务层接口，用于处理业务逻辑。</param>
        //public TransactionController2(ITransactionService transactionService, IApiLogHelper apiLogHelper)
        //{
        //    _transactionService = transactionService;
        //    _apiLogHelper = apiLogHelper;
        //}

        ///// <summary>
        ///// 根据ID获取单个记录的汇总信息。
        ///// </summary>
        ///// <param name="id">资源的唯一标识符。</param>
        ///// <returns>返回包含指定记录汇总信息的 ApiResponse；如果记录不存在，则返回 NotFound。</returns>
        //[HttpGet]
        //[HttpGet]
        //[Route("{id}")]
        //public async Task<IActionResult> GetById(Guid id)
        //{
        //    // 高精度计时
        //    var stopwatch = Stopwatch.StartNew();
        //    object? responseBody = null;

        //    try
        //    {
        //        var transaction = await _transactionService.GetSummaryByIdAsync(id);
        //        if (transaction == null)
        //        {
        //            responseBody = ApiResponse<TransactionDto>.NotFound();
        //            return NotFound(responseBody);
        //        }
        //        responseBody = ApiResponse<TransactionDto>.OperationSuccess(transaction);

        //        return Ok(responseBody);
        //    }
        //    finally
        //    {
        //        // 记录请求日志
        //        stopwatch.Stop();
        //        //_apiLogHelper.LogApiRequest(log, responseBody, stopwatch, loggerName);
        //    }

        //}

        ///// <summary>
        ///// 获取所有记录的汇总信息，支持按账本ID和时间范围过滤。
        ///// </summary>
        ///// <param name="ledgerId">账本ID（可选）。</param>
        ///// <param name="startDate">开始日期（可选）。</param>
        ///// <param name="endDate">结束日期（可选）。</param>
        ///// <returns>返回包含所有记录汇总信息的 ApiResponse。</returns>
        //[HttpGet]
        //[Route("all")]
        //public async Task<IActionResult> GetAll(Guid? ledgerId, DateTime? startDate, DateTime? endDate)
        //{
        //    // 高精度计时
        //    var stopwatch = Stopwatch.StartNew();
        //    object? responseBody = null;

        //    try
        //    {
        //        var transactions = await _transactionService.GetAllSummaryAsync(ledgerId, startDate, endDate);
        //        responseBody = ApiResponse<IEnumerable<TransactionDto>>.OperationSuccess(transactions);

        //        return Ok(responseBody);
        //    }
        //    finally
        //    {
        //        // 记录请求日志
        //        stopwatch.Stop();
        //        //_apiLogHelper.LogApiRequest(log, responseBody, stopwatch, loggerName);
        //    }

        //}

        ///// <summary>
        ///// 创建新记录。
        ///// </summary>
        ///// <param name="createTransactionDto">要创建的新记录DTO。</param>
        ///// <returns>返回包含已创建记录汇总信息的 ApiResponse。</returns>
        //[HttpPost]
        //[Route("")]
        //public async Task<IActionResult> Create([FromBody] CreateTransactionDto createTransactionDto)
        //{
        //    // 高精度计时
        //    var stopwatch = Stopwatch.StartNew();
        //    object? responseBody = null;

        //    try
        //    {
        //        var id = await _transactionService.CreateAsync(createTransactionDto, tempUserId);
        //        var addedTransaction = await _transactionService.GetSummaryByIdAsync(id);
        //        responseBody = ApiResponse<TransactionDto>.OperationSuccess(addedTransaction);

        //        return Ok(responseBody);
        //    }
        //    finally
        //    {
        //        // 记录请求日志
        //        stopwatch.Stop();
        //        //_apiLogHelper.LogApiRequest(log, responseBody, stopwatch, loggerName, createTransactionDto);
        //    }

        //}

        ///// <summary>
        ///// 更新记录。
        ///// </summary>
        ///// <param name="id">记录的唯一标识符。</param>
        ///// <param name="updateTransactionDto">要更新的记录DTO。</param>
        ///// <returns>返回包含更新结果的 ApiResponse。</returns>
        //[HttpPut]
        //[Route("{id}")]
        //public async Task<IActionResult> Update(Guid id, [FromBody] UpdateTransactionDto updateTransactionDto)
        //{
        //    // 高精度计时
        //    var stopwatch = Stopwatch.StartNew();
        //    object? responseBody = null;

        //    try
        //    {
        //        var updatedId = await _transactionService.UpdateAsync(updateTransactionDto, tempUserId);
        //        responseBody = ApiResponse<Guid>.OperationSuccess(updatedId);

        //        return Ok(responseBody);
        //    }
        //    finally
        //    {
        //        // 记录请求日志
        //        stopwatch.Stop();
        //        //_apiLogHelper.LogApiRequest(log, responseBody, stopwatch, loggerName, updateTransactionDto);
        //    }

        //}

        ///// <summary>
        ///// 删除指定ID的记录。
        ///// </summary>
        ///// <param name="id">要删除记录的唯一标识符。</param>
        ///// <returns>返回包含删除结果的 ApiResponse。</returns>
        //[HttpDelete]
        //[Route("{id}")]
        //public async Task<IActionResult> Delete(Guid id)
        //{
        //    // 高精度计时
        //    var stopwatch = Stopwatch.StartNew();
        //    object? responseBody = null;

        //    try
        //    {
        //        var deletedId = await _transactionService.DeleteAsync(id, tempUserId);
        //        responseBody = ApiResponse<Guid>.OperationSuccess(deletedId);

        //        return Ok(responseBody);
        //    }
        //    finally
        //    {
        //        // 记录请求日志
        //        stopwatch.Stop();
        //        //_apiLogHelper.LogApiRequest(log, responseBody, stopwatch, loggerName);
        //    }
        //}

        ///// <summary>
        ///// 获取分页记录的汇总信息，支持按账本ID、分类ID及时间范围过滤。
        ///// </summary>
        ///// <param name="pageNumber">当前页码（默认值为1）。</param>
        ///// <param name="pageSize">每页大小（默认值为10）。</param>
        ///// <param name="ledgerId">账本ID（可选）。</param>
        ///// <param name="categoryId">分类ID（可选）。</param>
        ///// <param name="startDate">开始日期（可选）。</param>
        ///// <param name="endDate">结束日期（可选）。</param>
        ///// <returns>返回包含分页记录汇总信息的 ApiResponse。</returns>
        //[HttpGet]
        //[Route("paged")]
        //public async Task<IActionResult> GetPaged(int pageNumber = 1, int pageSize = 10, Guid? ledgerId = null, Guid? categoryId = null, DateTime? startDate = null, DateTime? endDate = null)
        //{
        //    // 高精度计时
        //    var stopwatch = Stopwatch.StartNew();
        //    object? responseBody = null;

        //    try
        //    {
        //        var pagedResult = await _transactionService.GetPagedSummaryAsync(pageNumber, pageSize, ledgerId, categoryId, startDate, endDate);
        //        responseBody = ApiResponse<PaginationResult<TransactionDto>>.OperationSuccess(pagedResult);

        //        return Ok(responseBody);
        //    }
        //    finally
        //    {
        //        // 记录请求日志
        //        stopwatch.Stop();
        //        //_apiLogHelper.LogApiRequest(log, responseBody, stopwatch, loggerName);
        //    }
        //}

        ///// <summary>
        ///// 获取指定账本在时间范围内支出最高的前N条记录。
        ///// </summary>
        ///// <param name="ledgerId">账本ID。</param>
        ///// <param name="startDate">开始日期。</param>
        ///// <param name="endDate">结束日期。</param>
        ///// <param name="top">获取的记录数量（默认值为10）。</param>
        ///// <returns>返回包含支出最高记录汇总信息的 ApiResponse。</returns>
        //[HttpGet]
        //[Route("top-expense")]
        //public async Task<IActionResult> GetTopExpense(Guid ledgerId, DateTime startDate, DateTime endDate, int top = 10)
        //{
        //    // 高精度计时
        //    var stopwatch = Stopwatch.StartNew();
        //    object? responseBody = null;

        //    try
        //    {
        //        var transactions = await _transactionService.GetTopExpenseSummaryAsync(ledgerId, startDate, endDate, top);
        //        responseBody = ApiResponse<IEnumerable<TransactionDto>>.OperationSuccess(transactions);

        //        return Ok(responseBody);
        //    }
        //    finally
        //    {
        //        // 记录请求日志
        //        stopwatch.Stop();
        //        //_apiLogHelper.LogApiRequest(log, responseBody, stopwatch, loggerName);
        //    }
        //}

        ///// <summary>
        ///// 获取指定账本在时间范围内分类总金额排名。
        ///// </summary>
        ///// <param name="ledgerId">账本ID。</param>
        ///// <param name="startDate">开始日期。</param>
        ///// <param name="endDate">结束日期。</param>
        ///// <param name="categoryType">分类类型（可选）。</param>
        ///// <returns>返回包含分类总金额排名信息的 ApiResponse。</returns>
        //[HttpGet]
        //[Route("category-total-amount-ranking")]
        //public async Task<IActionResult> GetCategoryTotalAmountRanking(Guid ledgerId, DateTime startDate, DateTime endDate, string? categoryType = null)
        //{
        //    // 高精度计时
        //    var stopwatch = Stopwatch.StartNew();
        //    object? responseBody = null;

        //    try
        //    {
        //        var queryResults = await _transactionService.GetCategoryTotalAmountRankingAsync(ledgerId, startDate, endDate, categoryType);
        //        responseBody = ApiResponse<IEnumerable<CategoryTotalAmountDto>>.OperationSuccess(queryResults);

        //        return Ok(responseBody);
        //    }
        //    finally
        //    {
        //        // 记录请求日志
        //        stopwatch.Stop();
        //        //_apiLogHelper.LogApiRequest(log, responseBody, stopwatch, loggerName);
        //    }
        //}

        ///// <summary>
        ///// 获取指定账本某年的月度总金额统计。
        ///// </summary>
        ///// <param name="ledgerId">账本ID。</param>
        ///// <param name="categoryType">分类类型。</param>
        ///// <param name="year">年份。</param>
        ///// <returns>返回包含月度总金额统计信息的 ApiResponse。</returns>
        //[HttpGet]
        //[Route("monthly-totals")]
        //public async Task<IActionResult> GetMonthlyTotals(Guid ledgerId, string categoryType, int year)
        //{
        //    // 高精度计时
        //    var stopwatch = Stopwatch.StartNew();
        //    object? responseBody = null;

        //    try
        //    {
        //        var queryResults = await _transactionService.GetMonthlyTotalsAsync(ledgerId, categoryType, year);
        //        responseBody = ApiResponse<IEnumerable<MonthlyTotalAmountDto>>.OperationSuccess(queryResults);

        //        return Ok(responseBody);
        //    }
        //    finally
        //    {
        //        // 记录请求日志
        //        stopwatch.Stop();
        //        //_apiLogHelper.LogApiRequest(log, responseBody, stopwatch, loggerName);
        //    }
        //}

        ///// <summary>
        ///// 获取指定账本在当前时间点的财务汇总信息。
        ///// </summary>
        ///// <param name="ledgerId">账本ID。</param>
        ///// <param name="datetimeNow">当前时间。</param>
        ///// <returns>返回包含财务汇总信息的 ApiResponse。</returns>
        //[HttpGet]
        //[Route("financial-summaries")]
        //public async Task<IActionResult> GetFinancialSummaries(Guid ledgerId, DateTime datetimeNow)
        //{
        //    // 高精度计时
        //    var stopwatch = Stopwatch.StartNew();
        //    object? responseBody = null;

        //    try
        //    {
        //        var queryResults = await _transactionService.GetFinancialSummariesAsync(ledgerId, datetimeNow);
        //        responseBody = ApiResponse<FinancialSummaryDto>.OperationSuccess(queryResults);

        //        return Ok(responseBody);
        //    }
        //    finally
        //    {
        //        // 记录请求日志
        //        stopwatch.Stop();
        //        //_apiLogHelper.LogApiRequest(log, responseBody, stopwatch, loggerName);
        //    }
        //}

        ///// <summary>
        ///// 获取指定账本在时间范围内的总金额统计。
        ///// </summary>
        ///// <param name="ledgerId">账本ID。</param>
        ///// <param name="startDate">开始日期。</param>
        ///// <param name="endDate">结束日期。</param>
        ///// <returns>返回包含时间范围内总金额统计信息的 ApiResponse。</returns>
        //[HttpGet]
        //[Route("date-range-total-amount")]
        //public async Task<IActionResult> GetDateRangeTotalAmount(Guid ledgerId, DateTime startDate, DateTime endDate)
        //{
        //    // 高精度计时
        //    var stopwatch = Stopwatch.StartNew();
        //    object? responseBody = null;

        //    try
        //    {
        //        var queryResults = await _transactionService.GetDateRangeTotalAmountAsync(ledgerId, startDate, endDate);
        //        responseBody = ApiResponse<DateRangeTotalAmountDto>.OperationSuccess(queryResults);

        //        return Ok(responseBody);
        //    }
        //    finally
        //    {
        //        // 记录请求日志
        //        stopwatch.Stop();
        //        //_apiLogHelper.LogApiRequest(log, responseBody, stopwatch, loggerName);
        //    }
        //}

        ///// <summary>
        ///// 获取指定账本中最早的一笔交易记录。
        ///// </summary>
        ///// <param name="ledgerId">账本ID。</param>
        ///// <returns>如果找到最早交易记录，则返回包含该记录的 ApiResponse；否则返回 NotFound。</returns>
        //[HttpGet]
        //[Route("earliest")]
        //public async Task<IActionResult> GetEarliest(Guid ledgerId)
        //{
        //    // 高精度计时
        //    var stopwatch = Stopwatch.StartNew();
        //    object? responseBody = null;

        //    try
        //    {
        //        var transaction = await _transactionService.GetEarliestByLedgerIdAsync(ledgerId);
        //        if (transaction == null)
        //        {
        //            responseBody = ApiResponse<TransactionDto>.NotFound();
        //            return NotFound(responseBody);
        //        }
        //        responseBody = ApiResponse<TransactionDto>.OperationSuccess(transaction);

        //        return Ok(responseBody);
        //    }
        //    finally
        //    {
        //        // 记录请求日志
        //        stopwatch.Stop();
        //        //_apiLogHelper.LogApiRequest(log, responseBody, stopwatch, loggerName);
        //    }
        //}

    }
}