using Bedrock.WebApi.Responses;
using Bedrock.Application.DataTransferObjects;
using Bedrock.Application.Interfaces;
using Bedrock.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Bedrock.WebApi.Controllers
{
    /// <summary>
    /// App版本控制器。
    /// </summary>
    [ApiController]
    [Authorize]
    [Route("api/app-version")]
    public class AppVersionController : ControllerBase
    {
        private readonly IAppVersionService _appVersionService;

        /// <summary>
        /// 构造函数注入服务层依赖。
        /// </summary>
        /// <param name="appVersionService">App版本业务逻辑服务。</param>
        public AppVersionController(IAppVersionService appVersionService)
        {
            _appVersionService = appVersionService;
        }

        /// <summary>
        /// 创建新的App版本。
        /// </summary>
        /// <param name="createDto">创建数据传输对象，包含 AppId 等字段。</param>
        /// <returns>返回创建成功的App版本信息。</returns>
        /// <exception cref="ArgumentException">当 AppId 和 VersionCode 已存在时抛出。</exception>
        /// <exception cref="ArgumentNullException">当 createDto 为 null 时抛出。</exception>
        [HttpPost]
        public async Task<IActionResult> CreateAsync([FromBody] CreateAppVersionDto createDto)
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

            var id = await _appVersionService.CreateAsync(createDto, currentUser.Id);
            var dto = await _appVersionService.GetAsync(id);
            var responseCreate = ApiResponse<AppVersionDto>.OperationSuccess(dto);

            return Ok(responseCreate);
        }

        /// <summary>
        /// 更新指定的App版本。
        /// </summary>
        /// <param name="id">App版本 ID。</param>
        /// <param name="updateDto">更新 DTO，必须包含有效 ID。</param>
        /// <returns>返回被更新的App版本 ID。</returns>
        /// <exception cref="ArgumentException">当记录不存在、已删除或更新后违反唯一性约束时抛出。</exception>
        /// <exception cref="ArgumentNullException">当 updateDto 为 null 时抛出。</exception>
        /// <exception cref="InvalidOperationException">当数据库更新未影响任何行时抛出（并发冲突）。</exception>
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateAsync(long id, [FromBody] UpdateAppVersionDto updateDto)
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

            var updatedId = await _appVersionService.UpdateAsync(updateDto, currentUser.Id);
            var responseUpdate = ApiResponse<long>.OperationSuccess(updatedId);

            return Ok(responseUpdate);
        }

        /// <summary>
        /// 软删除指定的App版本。
        /// </summary>
        /// <param name="id">要删除的App版本 ID。</param>
        /// <returns>返回被删除的App版本 ID。</returns>
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

            var deletedId = await _appVersionService.DeleteAsync(id, currentUser.Id);
            var responseDelete = ApiResponse<long>.OperationSuccess(deletedId);

            return Ok(responseDelete);
        }

        /// <summary>
        /// 批量删除App版本（软删除）。
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

            var deletedCount = await _appVersionService.DeleteBatchAsync(request.Ids, currentUser.Id);
            var responseDeleteBatch = ApiResponse<int>.OperationSuccess(deletedCount);

            return Ok(responseDeleteBatch);
        }

        ///// <summary>
        ///// 根据 ID 获取单条未删除的App版本详情。
        ///// </summary>
        ///// <param name="id">App版本 ID。</param>
        ///// <returns>返回匹配的 DTO；若未找到或已删除，则返回 null。</returns>
        //[HttpGet("{id}")]
        //public async Task<IActionResult> GetAsync(long id)
        //{
        //    var dto = await _appVersionService.GetAsync(id);

        //    if (dto == null)
        //    {
        //        var response = ApiResponse<AppVersionDto>.NotFound();
        //        return NotFound(response);
        //    }

        //    var responseGet = ApiResponse<AppVersionDto>.OperationSuccess(dto);
        //    return Ok(responseGet);
        //}

        ///// <summary>
        ///// 查询未删除的App版本列表，支持按名称模糊搜索。
        ///// </summary>
        ///// <param name="name">App版本名称，用于模糊搜索（可选）。</param>
        ///// <returns>返回匹配条件的 DTO 列表（可能为空）。</returns>
        //[HttpGet("all")]
        //public async Task<IActionResult> GetAllAsync(
        //    [FromQuery] string? name = null)
        //{
        //    var dtos = await _appVersionService.GetAllAsync(name);
        //    var responseAll = ApiResponse<List<AppVersionDto>>.OperationSuccess(dtos);

        //    return Ok(responseAll);
        //}

        ///// <summary>
        ///// 分页查询未删除的App版本列表，支持名称模糊搜索。
        ///// </summary>
        ///// <param name="pageNumber">页码，从 1 开始。</param>
        ///// <param name="pageSize">每页数量。</param>
        ///// <param name="appId">AppId，用于精确搜索（可选）。</param>
        ///// <param name="orderByField">排序字段（可选）。</param>
        ///// <param name="orderByType">排序方式（可选）。</param>
        ///// <returns>返回分页结果对象，包含数据和分页元信息。</returns>
        //[HttpGet("paged")]
        //public async Task<IActionResult> GetPagedAsync(
        //    [FromQuery] int pageNumber = 1,
        //    [FromQuery] int pageSize = 10,
        //    [FromQuery] string? appId = null,
        //    [FromQuery] string? orderByField = null,
        //    [FromQuery] string? orderByType = null)
        //{
        //    var pagedResult = await _appVersionService.GetPagedAsync(pageNumber, pageSize, appId, orderByField, orderByType);
        //    var responsePaged = ApiResponse<PaginationResult<AppVersionDto>>.OperationSuccess(pagedResult);

        //    return Ok(responsePaged);
        //}

        /// <summary>
        /// 根据 ID 获取单条未删除的App版本详情。
        /// </summary>
        /// <param name="id">App版本 ID。</param>
        /// <returns>返回匹配的 DTO；若未找到或已删除，则返回 null。</returns>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetAsync(long id)
        {
            var dto = await _appVersionService.GetSummaryAsync(id);

            if (dto == null)
            {
                var response = ApiResponse<AppVersionSummary>.NotFound();
                return NotFound(response);
            }

            var responseGet = ApiResponse<AppVersionSummary>.OperationSuccess(dto);
            return Ok(responseGet);
        }

        /// <summary>
        /// 查询未删除的App版本列表，支持按名称模糊搜索。
        /// </summary>
        /// <param name="name">App版本名称，用于模糊搜索（可选）。</param>
        /// <returns>返回匹配条件的 DTO 列表（可能为空）。</returns>
        [HttpGet("all")]
        public async Task<IActionResult> GetAllAsync(
            [FromQuery] string? name = null)
        {
            var dtos = await _appVersionService.GetAllSummaryAsync(name);
            var responseAll = ApiResponse<List<AppVersionSummary>>.OperationSuccess(dtos);

            return Ok(responseAll);
        }

        /// <summary>
        /// 分页查询未删除的App版本列表，支持名称模糊搜索。
        /// </summary>
        /// <param name="pageNumber">页码，从 1 开始。</param>
        /// <param name="pageSize">每页数量。</param>
        /// <param name="appId">AppId，用于精确搜索（可选）。</param>
        /// <param name="orderByField">排序字段（可选）。</param>
        /// <param name="orderByType">排序方式（可选）。</param>
        /// <returns>返回分页结果对象，包含数据和分页元信息。</returns>
        [HttpGet("paged")]
        public async Task<IActionResult> GetPagedAsync(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? appId = null,
            [FromQuery] string? orderByField = null,
            [FromQuery] string? orderByType = null)
        {
            var pagedResult = await _appVersionService.GetPagedSummaryAsync(pageNumber, pageSize, appId, orderByField, orderByType);
            var responsePaged = ApiResponse<PaginationResult<AppVersionSummary>>.OperationSuccess(pagedResult);

            return Ok(responsePaged);
        }

        /// <summary>
        /// 根据 ID 获取已上线发行的最大版本信息。
        /// </summary>
        /// <param name="appId">记录的 AppId。</param>
        /// <returns>返回包含指定记录的 ApiResponse；如果记录不存在，则返回 NotFound。</returns>
        [HttpGet]
        [Route("max-version/{appId}")]
        public async Task<IActionResult> GetMaxVersion(string appId)
        {
            var dto = await _appVersionService.GetMaxVersionAsync(appId);
            //if (dto == null)
            //{
            //    var response = ApiResponse<AppVersionDto>.NotFound();
            //    return NotFound(response);
            //}

            var responseGet = ApiResponse<AppVersionDto>.OperationSuccess(dto);
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