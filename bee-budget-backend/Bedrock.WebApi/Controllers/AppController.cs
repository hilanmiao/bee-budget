using Bedrock.WebApi.Responses;
using Bedrock.Application.DataTransferObjects;
using Bedrock.Application.Interfaces;
using Bedrock.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Bedrock.WebApi.Controllers
{
    /// <summary>
    /// App应用控制器。
    /// </summary>
    [ApiController]
    [Authorize]
    [Route("api/app")]
    public class AppController : ControllerBase
    {
        private readonly IAppService _appService;

        /// <summary>
        /// 构造函数注入服务层依赖。
        /// </summary>
        /// <param name="appService">App应用业务逻辑服务。</param>
        public AppController(IAppService appService)
        {
            _appService = appService;
        }

        /// <summary>
        /// 创建新的App应用。
        /// </summary>
        /// <param name="createDto">创建数据传输对象，包含 AppId 等字段。</param>
        /// <returns>返回创建成功的App应用信息。</returns>
        /// <exception cref="ArgumentException">当 AppId 已存在时抛出。</exception>
        /// <exception cref="ArgumentNullException">当 createDto 为 null 时抛出。</exception>
        [HttpPost]
        public async Task<IActionResult> CreateAsync([FromBody] CreateAppDto createDto)
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

            var id = await _appService.CreateAsync(createDto, currentUser.Id);
            var dto = await _appService.GetAsync(id);
            var responseCreate = ApiResponse<AppDto>.OperationSuccess(dto);

            return Ok(responseCreate);
        }

        /// <summary>
        /// 更新指定的App应用。
        /// </summary>
        /// <param name="id">App应用 ID。</param>
        /// <param name="updateDto">更新 DTO，必须包含有效 ID。</param>
        /// <returns>返回被更新的App应用 ID。</returns>
        /// <exception cref="ArgumentException">当记录不存在、已删除或更新后违反唯一性约束时抛出。</exception>
        /// <exception cref="ArgumentNullException">当 updateDto 为 null 时抛出。</exception>
        /// <exception cref="InvalidOperationException">当数据库更新未影响任何行时抛出（并发冲突）。</exception>
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateAsync(long id, [FromBody] UpdateAppDto updateDto)
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

            var updatedId = await _appService.UpdateAsync(updateDto, currentUser.Id);
            var responseUpdate = ApiResponse<long>.OperationSuccess(updatedId);

            return Ok(responseUpdate);
        }

        /// <summary>
        /// 软删除指定的App应用。
        /// </summary>
        /// <param name="id">要删除的App应用 ID。</param>
        /// <returns>返回被删除的App应用 ID。</returns>
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

            var deletedId = await _appService.DeleteAsync(id, currentUser.Id);
            var responseDelete = ApiResponse<long>.OperationSuccess(deletedId);

            return Ok(responseDelete);
        }

        /// <summary>
        /// 批量删除App应用（软删除）。
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

            var deletedCount = await _appService.DeleteBatchAsync(request.Ids, currentUser.Id);
            var responseDeleteBatch = ApiResponse<int>.OperationSuccess(deletedCount);

            return Ok(responseDeleteBatch);
        }

        /// <summary>
        /// 根据 ID 获取单条未删除的App应用详情。
        /// </summary>
        /// <param name="id">App应用 ID。</param>
        /// <returns>返回匹配的 DTO；若未找到或已删除，则返回 null。</returns>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetAsync(long id)
        {
            var dto = await _appService.GetAsync(id);

            if (dto == null)
            {
                var response = ApiResponse<AppDto>.NotFound();
                return NotFound(response);
            }

            var responseGet = ApiResponse<AppDto>.OperationSuccess(dto);
            return Ok(responseGet);
        }

        /// <summary>
        /// 查询未删除的App应用列表，支持按名称模糊搜索。
        /// </summary>
        /// <param name="name">App应用名称，用于模糊搜索（可选）。</param>
        /// <returns>返回匹配条件的 DTO 列表（可能为空）。</returns>
        [HttpGet("all")]
        public async Task<IActionResult> GetAllAsync(
            [FromQuery] string? name = null)
        {
            var dtos = await _appService.GetAllAsync(name);
            var responseAll = ApiResponse<List<AppDto>>.OperationSuccess(dtos);

            return Ok(responseAll);
        }

        /// <summary>
        /// 分页查询未删除的App应用列表，支持名称模糊搜索。
        /// </summary>
        /// <param name="pageNumber">页码，从 1 开始。</param>
        /// <param name="pageSize">每页数量。</param>
        /// <param name="name">App应用名称，用于模糊搜索（可选）。</param>
        /// <param name="orderByField">排序字段（可选）。</param>
        /// <param name="orderByType">排序方式（可选）。</param>
        /// <returns>返回分页结果对象，包含数据和分页元信息。</returns>
        [HttpGet("paged")]
        public async Task<IActionResult> GetPagedAsync(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? name = null,
            [FromQuery] string? orderByField = null,
            [FromQuery] string? orderByType = null)
        {
            var pagedResult = await _appService.GetPagedAsync(pageNumber, pageSize, name, orderByField, orderByType);
            var responsePaged = ApiResponse<PaginationResult<AppDto>>.OperationSuccess(pagedResult);

            return Ok(responsePaged);
        }

        /// <summary>
        /// 根据AppId获取唯一未删除的App应用详情。
        /// </summary>
        /// <param name="appId">AppId（如“__UNI__9B0E754”），用于精确匹配。</param>
        /// <returns>返回匹配的 DTO；若未找到或已删除，则返回 null。</returns>
        [HttpGet("by-app-id/{appId}")]
        public async Task<IActionResult> GetByAppIdAsync(string appId)
        {
            var dto = await _appService.GetByAppIdAsync(appId);

            if (dto == null)
            {
                var response = ApiResponse<AppDto>.NotFound();
                return NotFound(response);
            }

            var responseGet = ApiResponse<AppDto>.OperationSuccess(dto);
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