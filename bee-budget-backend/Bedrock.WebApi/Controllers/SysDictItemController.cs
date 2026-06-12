using Bedrock.WebApi.Responses;
using Bedrock.Application.DataTransferObjects;
using Bedrock.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Bedrock.WebApi.Controllers
{
    /// <summary>
    /// 控制器用于管理字典项（SysDictItem）的增删改查及批量操作。
    /// </summary>
    [ApiController]
    [Authorize]
    [Route("api/sys-dict-item")]
    public class SysDictItemController : ControllerBase
    {
        private readonly ISysDictItemService _sysDictItemService;

        /// <summary>
        /// 构造函数注入服务层依赖。
        /// </summary>
        /// <param name="sysDictItemService">字典项业务逻辑服务。</param>
        public SysDictItemController(ISysDictItemService sysDictItemService)
        {
            _sysDictItemService = sysDictItemService;
        }

        /// <summary>
        /// 创建新的字典项。
        /// </summary>
        /// <param name="createDto">创建字典项的 DTO。</param>
        /// <returns>返回创建成功的字典项信息。</returns>
        [HttpPost]
        public async Task<IActionResult> CreateAsync([FromBody] CreateSysDictItemDto createDto)
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

            var id = await _sysDictItemService.CreateAsync(createDto, currentUser.Id);
            var dto = await _sysDictItemService.GetAsync(id);
            var responseCreate = ApiResponse<SysDictItemDto>.OperationSuccess(dto);

            return Ok(responseCreate);
        }

        /// <summary>
        /// 批量创建字典项。
        /// </summary>
        /// <param name="createDtos">字典项创建 DTO 列表。</param>
        /// <returns>返回成功创建的数量。</returns>
        [HttpPost("batch")]
        public async Task<IActionResult> CreateBatchAsync([FromBody] List<CreateSysDictItemDto> createDtos)
        {
            if (createDtos == null || !createDtos.Any())
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

            var createdCount = await _sysDictItemService.CreateBatchAsync(createDtos, currentUser.Id);
            var responseCreateBatch = ApiResponse<int>.OperationSuccess(createdCount);

            return Ok(responseCreateBatch);
        }

        /// <summary>
        /// 更新指定 ID 的字典项。
        /// </summary>
        /// <param name="id">字典项 ID。</param>
        /// <param name="updateDto">更新 DTO。</param>
        /// <returns>返回更新后的字典项 ID。</returns>
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateAsync(long id, [FromBody] UpdateSysDictItemDto updateDto)
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

            var updatedId = await _sysDictItemService.UpdateAsync(updateDto, currentUser.Id);
            var responseUpdate = ApiResponse<long>.OperationSuccess(updatedId);

            return Ok(responseUpdate);
        }

        /// <summary>
        /// 批量更新字典项。
        /// </summary>
        /// <param name="updateDtos">字典项更新 DTO 列表，包含 ID。</param>
        /// <returns>返回成功更新的数量。</returns>
        [HttpPut("batch")]
        public async Task<IActionResult> UpdateBatchAsync([FromBody] List<UpdateSysDictItemDto> updateDtos)
        {
            if (updateDtos == null || !updateDtos.Any())
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

            var updatedCount = await _sysDictItemService.UpdateBatchAsync(updateDtos, currentUser.Id);
            var responseUpdateBatch = ApiResponse<int>.OperationSuccess(updatedCount);

            return Ok(responseUpdateBatch);
        }

        /// <summary>
        /// 删除指定 ID 的字典项（软删除）。
        /// </summary>
        /// <param name="id">字典项 ID。</param>
        /// <returns>返回被删除的记录 ID。</returns>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAsync(long id)
        {
            var currentUser = GetCurrentUser();
            if (currentUser == null)
            {
                var response = ApiResponse<SysUserDto>.Unauthorized();
                return Unauthorized(response);
            }

            var deletedId = await _sysDictItemService.DeleteAsync(id, currentUser.Id);
            var responseDelete = ApiResponse<long>.OperationSuccess(deletedId);

            return Ok(responseDelete);
        }

        /// <summary>
        /// 批量删除字典项（软删除）。
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

            var deletedCount = await _sysDictItemService.DeleteBatchAsync(request.Ids, currentUser.Id);
            var responseDeleteBatch = ApiResponse<int>.OperationSuccess(deletedCount);

            return Ok(responseDeleteBatch);
        }

        /// <summary>
        /// 根据 ID 获取字典项详情。
        /// </summary>
        /// <param name="id">字典项 ID。</param>
        /// <returns>返回字典项详情。</returns>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetAsync(long id)
        {
            var dto = await _sysDictItemService.GetAsync(id);

            if (dto == null)
            {
                var response = ApiResponse<SysDictItemDto>.NotFound();
                return NotFound(response);
            }

            var responseGet = ApiResponse<SysDictItemDto>.OperationSuccess(dto);
            return Ok(responseGet);
        }

        /// <summary>
        /// 获取所有字典项（支持按标签和字典项状态过滤）。
        /// </summary>
        /// <param name="label">字典项标签（模糊匹配）。</param>
        /// <param name="status">字典项状态（启用/禁用）。</param>
        /// <returns>返回所有匹配的字典项列表。</returns>
        [HttpGet("all")]
        public async Task<IActionResult> GetAllAsync(
            [FromQuery] string? label = null,
            [FromQuery] string? status = null)
        {
            var dtos = await _sysDictItemService.GetAllAsync(label, status);
            var responseAll = ApiResponse<List<SysDictItemDto>>.OperationSuccess(dtos);

            return Ok(responseAll);
        }

        /// <summary>
        /// 分页获取字典项列表（支持多条件过滤）。
        /// </summary>
        /// <param name="pageNumber">页码，从 1 开始。</param>
        /// <param name="pageSize">每页数量。</param>
        /// <param name="label">字典项标签，模糊搜索（可选）。</param>
        /// <param name="value">字典项实际值，模糊搜索（可选）。</param>
        /// <param name="status">字典项状态，精确匹配（可选）。</param>
        /// <param name="categoryCode">字典分类编码，精确匹配（可选）。</param>
        /// <param name="orderByField">排序字段（可选）。</param>
        /// <param name="orderByType">排序方式（可选）。</param>
        /// <returns>返回分页结果。</returns>
        [HttpGet("paged")]
        public async Task<IActionResult> GetPagedAsync(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? label = null,
            [FromQuery] string? value = null,
            [FromQuery] string? status = null,
            [FromQuery] string? categoryCode = null,
            [FromQuery] string? orderByField = null,
            [FromQuery] string? orderByType = null)
        {
            var pagedResult = await _sysDictItemService.GetPagedAsync(pageNumber, pageSize, label, value, status, categoryCode, orderByField, orderByType);
            var responsePaged = ApiResponse<PaginationResult<SysDictItemDto>>.OperationSuccess(pagedResult);

            return Ok(responsePaged);
        }
        
        /// <summary>
        /// 根据字典分类编码查询字典项列表。
        /// </summary>
        /// <param name="categoryCode">字典分类编码。</param>
        /// <returns>返回所有匹配的字典项列表。</returns>
        [HttpGet("by-category-code/{categoryCode}")]
        public async Task<IActionResult> GetAllByCategoryCode(string categoryCode)
        {
            var dtos = await _sysDictItemService.GetAllByCategoryCodeAsync(categoryCode);
            var responseAll = ApiResponse<List<SysDictItemDto>>.OperationSuccess(dtos);

            return Ok(responseAll);
        }

        /// <summary>
        /// 更新字典项的字典项状态。
        /// </summary>
        /// <param name="id">要更新的字典项 ID。</param>
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

            var updatedId = await _sysDictItemService.ChangeStatusAsync(id, changeStatusDto.Status, currentUser.Id);
            var responseUpdate = ApiResponse<long>.OperationSuccess(updatedId);
            return Ok(responseUpdate);
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