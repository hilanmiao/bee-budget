using Bedrock.WebApi.Responses;
using Bedrock.Application.DataTransferObjects;
using Bedrock.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Bedrock.WebApi.Controllers
{
    /// <summary>
    /// 交易控制器。
    /// </summary>
    [ApiController]
    [Authorize]
    [Route("api/transaction")]
    public class TransactionController : ControllerBase
    {
        private readonly ITransactionService _transactionService;

        /// <summary>
        /// 构造函数注入服务层依赖。
        /// </summary>
        /// <param name="transactionService">交易业务逻辑服务。</param>
        public TransactionController(ITransactionService transactionService)
        {
            _transactionService = transactionService;
        }

        /// <summary>
        /// 创建新的交易。
        /// </summary>
        /// <param name="createDto">创建数据传输对象，包含 Amount、Date 等字段。</param>
        /// <returns>返回创建成功的交易信息。</returns>
        /// <exception cref="ArgumentException">当账本、交易分类已删除时抛出。</exception>
        /// <exception cref="ArgumentNullException">当 createDto 为 null 时抛出。</exception>
        [HttpPost]
        public async Task<IActionResult> CreateAsync([FromBody] CreateTransactionDto createDto)
        {
            if (createDto == null)
            {
                var response = ApiResponse<object>.BadRequest("请求数据不能为空。");
                return BadRequest(response);
            }

            var currentUser = GetCurrentUser();
            if (currentUser == null)
            {
                var response = ApiResponse<SysUserDto>.Unauthorized();
                return Unauthorized(response);
            }

            var id = await _transactionService.CreateAsync(createDto, currentUser.Id);
            var dto = await _transactionService.GetSummaryAsync(id);
            var responseCreate = ApiResponse<TransactionSummary>.OperationSuccess(dto);

            return Ok(responseCreate);
        }

        /// <summary>
        /// 更新指定的交易。
        /// </summary>
        /// <param name="id">交易 ID。</param>
        /// <param name="updateDto">更新 DTO，必须包含有效 ID。</param>
        /// <returns>返回被更新的交易 ID。</returns>
        /// <exception cref="ArgumentException">当记录不存在、已删除或更新后违反唯一性约束时抛出。</exception>
        /// <exception cref="ArgumentNullException">当 updateDto 为 null 时抛出。</exception>
        /// <exception cref="InvalidOperationException">当数据库更新未影响任何行时抛出（并发冲突）。</exception>
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateAsync(long id, [FromBody] UpdateTransactionDto updateDto)
        {
            if (updateDto == null)
            {
                var response = ApiResponse<object>.BadRequest("请求数据不能为空。");
                return BadRequest(response);
            }

            var currentUser = GetCurrentUser();
            if (currentUser == null)
            {
                var response = ApiResponse<SysUserDto>.Unauthorized();
                return Unauthorized(response);
            }

            var updatedId = await _transactionService.UpdateAsync(updateDto, currentUser.Id);
            var responseUpdate = ApiResponse<long>.OperationSuccess(updatedId);

            return Ok(responseUpdate);
        }

        /// <summary>
        /// 软删除指定的交易。
        /// </summary>
        /// <param name="id">要删除的交易 ID。</param>
        /// <returns>返回被删除的交易 ID。</returns>
        /// <exception cref="ArgumentException">当记录不存在或已被删除时抛出。</exception>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAsync(long id)
        {
            var currentUser = GetCurrentUser();
            if (currentUser == null)
            {
                var response = ApiResponse<SysUserDto>.Unauthorized();
                return Unauthorized(response);
            }

            var deletedId = await _transactionService.DeleteAsync(id, currentUser.Id);
            var responseDelete = ApiResponse<long>.OperationSuccess(deletedId);

            return Ok(responseDelete);
        }

        /// <summary>
        /// 批量删除交易（软删除）。
        /// </summary>
        /// <param name="request">包含 ID 列表的删除请求。</param>
        /// <returns>返回被删除的记录数量。</returns>
        [HttpDelete("batch")]
        public async Task<IActionResult> DeleteBatchAsync([FromBody] DeleteBatchRequest request)
        {
            if (request?.Ids == null || !request.Ids.Any())
            {
                var response = ApiResponse<object>.BadRequest("未提供任何 ID。");
                return BadRequest(response);
            }

            var currentUser = GetCurrentUser();
            if (currentUser == null)
            {
                var response = ApiResponse<SysUserDto>.Unauthorized();
                return Unauthorized(response);
            }

            var deletedCount = await _transactionService.DeleteBatchAsync(request.Ids, currentUser.Id);
            var responseDeleteBatch = ApiResponse<int>.OperationSuccess(deletedCount);

            return Ok(responseDeleteBatch);
        }

        /// <summary>
        /// 更新交易的状态。
        /// </summary>
        /// <param name="id">要更新的交易 ID。</param>
        /// <param name="changeStatusDto">数据传输对象</param>
        /// <returns>返回被更新的记录 ID。</returns>
        [HttpPut]
        [Route("change-status/{id}")]
        public async Task<IActionResult> ChangeStatus(long id, [FromBody] ChangeStatusDto changeStatusDto)
        {
            if (changeStatusDto == null)
            {
                var response = ApiResponse<object>.BadRequest("请求数据不能为空。");
                return BadRequest(response);
            }

            var currentUser = GetCurrentUser();
            if (currentUser == null)
            {
                var response = ApiResponse<SysUserDto>.Unauthorized();
                return Unauthorized(response);
            }

            var updatedId = await _transactionService.ChangeStatusAsync(id, changeStatusDto.Status, currentUser.Id);
            var responseUpdate = ApiResponse<long>.OperationSuccess(updatedId);
            return Ok(responseUpdate);
        }

        /// <summary>
        /// 根据 ID 获取单条未删除的交易详情。
        /// </summary>
        /// <param name="id">交易 ID。</param>
        /// <returns>返回匹配的 DTO；若未找到或已删除，则返回 null。</returns>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetAsync(long id)
        {
            var dto = await _transactionService.GetSummaryAsync(id);

            if (dto == null)
            {
                var response = ApiResponse<TransactionSummary>.NotFound();
                return NotFound(response);
            }

            var responseGet = ApiResponse<TransactionSummary>.OperationSuccess(dto);
            return Ok(responseGet);
        }

        /// <summary>
        /// 查询未删除的交易记录，支持按名称和描述模糊搜索，状态、交易分类Id精确搜索，日期范围搜索。
        /// </summary>
        /// <param name="ledgerId">交易账本Id。</param>
        /// <param name="transactionCategoryId">交易分类Id，用于精确搜索（可选）。</param>
        /// <param name="status">交易状态，用于模糊搜索（可选）。</param>
        /// <param name="startDate">交易开始日期，用于日期范围搜索（可选）。</param>
        /// <param name="endDate">交易结束日期，用于日期范围搜索（可选）。</param>
        /// <param name="description">交易描述，用于模糊搜索（可选）。</param>
        /// <returns>返回匹配条件的 DTO 列表（可能为空）。</returns>
        [HttpGet("all")]
        public async Task<IActionResult> GetAllAsync(
            [FromQuery] long ledgerId,
            [FromQuery] long? transactionCategoryId = null,
            [FromQuery] string? status = null,
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null,
            [FromQuery] string? description = null)
        {
            var currentUser = GetCurrentUser();
            if (currentUser == null)
            {
                var response = ApiResponse<SysUserDto>.Unauthorized();
                return Unauthorized(response);
            }

            var dtos = await _transactionService.GetAllSummaryAsync(currentUser.Id, ledgerId, transactionCategoryId, status, startDate, endDate, description);
            var responseAll = ApiResponse<List<TransactionSummary>>.OperationSuccess(dtos);

            return Ok(responseAll);
        }

        /// <summary>
        /// 分页查询未删除的交易记录，支持按名称和描述模糊搜索，状态、交易分类Id精确搜索，日期范围搜索。
        /// </summary>
        /// <param name="pageNumber">页码，从 1 开始。</param>
        /// <param name="pageSize">每页记录数。</param>
        /// <param name="ledgerId">交易账本Id，用于精确搜索（可选）。</param>
        /// <param name="transactionCategoryId">交易分类Id，用于精确搜索（可选）。</param>
        /// <param name="status">交易状态，用于模糊搜索（可选）。</param>
        /// <param name="startDate">交易开始日期，用于日期范围搜索（可选）。</param>
        /// <param name="endDate">交易结束日期，用于日期范围搜索（可选）。</param>
        /// <param name="description">交易描述，用于模糊搜索（可选）。</param>
        /// <param name="orderByField">排序字段（可选）。</param>
        /// <param name="orderByType">排序方式（可选）。</param>
        /// <returns>返回分页结果对象，包含数据和分页元信息。</returns>
        [HttpGet("paged")]
        public async Task<IActionResult> GetPagedAsync(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] long? ledgerId = null,
            [FromQuery] long? transactionCategoryId = null,
            [FromQuery] string? status = null,
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null,
            [FromQuery] string? description = null,
            [FromQuery] string? orderByField = null,
            [FromQuery] string? orderByType = null)
        {
            var currentUser = GetCurrentUser();
            if (currentUser == null)
            {
                var response = ApiResponse<SysUserDto>.Unauthorized();
                return Unauthorized(response);
            }

            var pagedResult = await _transactionService.GetPagedSummaryAsync(pageNumber, pageSize, currentUser.Id, ledgerId, transactionCategoryId, status, startDate, endDate, description, orderByField, orderByType);
            var responsePaged = ApiResponse<PaginationResult<TransactionSummary>>.OperationSuccess(pagedResult);

            return Ok(responsePaged);
        }

        /// <summary>
        /// 获取指定账本的实时数据快照（包含日/周/月/年收支统计）。
        /// </summary>
        /// <param name="ledgerId">要查询的账本Id</param>
        /// <returns>返回账本实时数据快照 Dto，即使未查询到数据也不会返回 null</returns>
        [HttpGet("ledger-snapshot")]
        public async Task<IActionResult> GetLedgerSnapshotAsync(long ledgerId)
        {
            var dto = await _transactionService.GetLedgerSnapshotAsync(ledgerId);

            var responseGet = ApiResponse<LedgerSnapshotDto>.OperationSuccess(dto);
            return Ok(responseGet);
        }

        /// <summary>
        /// 获取本月支出分类排名 TopN
        /// </summary>
        /// <param name="ledgerId">账本Id</param>
        /// <param name="top">排名前 N 的数量</param>
        /// <returns>按金额降序排列的前 N 笔支出交易的分类 Dto 列表。若无数据返回空列表。</returns>
        [HttpGet("monthly-expense-transaction-category-top-n")]
        public async Task<IActionResult> GetMonthlyExpenseTransactionCategoryTopNAsync(long ledgerId, int top)
        {
            var dto = await _transactionService.GetMonthlyExpenseTransactionCategoryTopNAsync(ledgerId, top);

            var responseGet = ApiResponse<List<TransactionCategoryStatDto>>.OperationSuccess(dto);
            return Ok(responseGet);
        }

        /// <summary>
        /// 获取本月收入分类排名 TopN
        /// </summary>
        /// <param name="ledgerId">账本Id</param>
        /// <param name="top">排名前 N 的数量</param>
        /// <returns>按金额降序排列的前 N 笔收入交易的 Dto 列表。若无数据返回空列表。</returns>
        [HttpGet("monthly-income-transaction-category-top-n")]
        public async Task<IActionResult> GetMonthlyIncomeTransactionCategoryTopNAsync(long ledgerId, int top)
        {
            var dto = await _transactionService.GetMonthlyIncomeTransactionCategoryTopNAsync(ledgerId, top);

            var responseGet = ApiResponse<List<TransactionCategoryStatDto>>.OperationSuccess(dto);
            return Ok(responseGet);
        }

        /// <summary>
        /// 获取本月支出分类构成
        /// </summary>
        /// <param name="ledgerId">账本Id</param>
        /// <returns>按金额降序排列的本月所有交易的分类 Dto 列表。若无数据返回空列表。</returns>
        [HttpGet("monthly-expense-transaction-category")]
        public async Task<IActionResult> GetMonthlyExpenseTransactionCategoryAsync(long ledgerId)
        {
            var dto = await _transactionService.GetMonthlyExpenseTransactionCategoryAsync(ledgerId);

            var responseGet = ApiResponse<List<TransactionCategoryStatDto>>.OperationSuccess(dto);
            return Ok(responseGet);
        }

        /// <summary>
        /// 获取本月单笔金额最大的前 N 笔支出交易记录。
        /// </summary>
        /// <param name="ledgerId">账本Id</param>
        /// <param name="top">排名前 N 的数量</param>
        /// <returns>按金额降序排列的前 N 笔交易 Dto 列表。若无数据返回空列表。</returns>
        [HttpGet("monthly-expense-transaction-top-n")]
        public async Task<IActionResult> GetMonthlyExpenseTransactionTopNAsync(long ledgerId, int top)
        {
            var dto = await _transactionService.GetMonthlyExpenseTransactionTopNAsync(ledgerId, top);

            var responseGet = ApiResponse<List<TransactionSummary>>.OperationSuccess(dto);
            return Ok(responseGet);
        }

        /// <summary>
        /// 统计指定账本本年度每月的收入与支出金额。
        /// </summary>
        /// <param name="ledgerId">账本 ID</param>
        /// <returns>
        /// 本年 1-12 月的收支统计 Dto 列表。
        /// 注意：未来未开始的月份返回 null，已过去但无交易的月份返回 0。
        /// </returns>
        [HttpGet("yearly-stats")]
        public async Task<IActionResult> GetYearlyStatsAsync(long ledgerId)
        {
            var dto = await _transactionService.GetYearlyStatsAsync(ledgerId);

            var responseGet = ApiResponse<List<TransactionMonthlyStatDto>>.OperationSuccess(dto);
            return Ok(responseGet);
        }

        /// <summary>
        /// 统计指定时间范围内的收入和支出总金额。
        /// </summary>
        /// <param name="ledgerId">账本 ID</param>
        /// <param name="startDate">开始日期</param>
        /// <param name="endDate">结束日期</param>
        /// <returns>包含总收入和总支出的统计对象</returns>
        [HttpGet("range-stats")]
        public async Task<IActionResult> GetRangeStatsAsync(long ledgerId, DateTime startDate, DateTime endDate)
        {
            var dto = await _transactionService.GetRangeStatsAsync(ledgerId, startDate, endDate);

            var responseGet = ApiResponse<TransactionTotalStatDto>.OperationSuccess(dto);
            return Ok(responseGet);
        }

        /// <summary>
        /// 获取指定账本最早的一笔交易。
        /// </summary>
        /// <param name="ledgerId">账本 ID</param>
        /// <returns>返回匹配的 DTO；若未找到或已删除，则返回 null。</returns>
        [HttpGet("earliest")]
        public async Task<IActionResult> GetEarliestAsync(long ledgerId)
        {
            var dto = await _transactionService.GetEarliestAsync(ledgerId);

            if (dto == null)
            {
                var response = ApiResponse<TransactionDto>.NotFound();
                return NotFound(response);
            }

            var responseGet = ApiResponse<TransactionDto>.OperationSuccess(dto);
            return Ok(responseGet);
        }

        #region Private Helpers

        /// <summary>
        /// 从 HttpContext 中获取当前用户信息。
        /// </summary>
        /// <returns>当前用户 DTO，若未登录则返回 null。</returns>
        private SysUserDto? GetCurrentUser()
        {
            return HttpContext.Items["User"] as SysUserDto;
        }

        #endregion
    }
}