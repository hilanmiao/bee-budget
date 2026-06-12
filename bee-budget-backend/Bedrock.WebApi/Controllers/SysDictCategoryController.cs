using Bedrock.WebApi.Responses;
using Bedrock.Application.DataTransferObjects;
using Bedrock.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Bedrock.WebApi.Controllers
{
    /// <summary>
    /// 系统字典分类控制器。
    /// </summary>
    [ApiController]
    [Authorize]
    [Route("api/sys-dict-category")]
    public class SysDictCategoryController : ControllerBase
    {
        private readonly ISysDictCategoryService _sysDictCategoryService;

        /// <summary>
        /// 构造函数注入服务层依赖。
        /// </summary>
        /// <param name="sysDictCategoryService">字典分类业务逻辑服务。</param>
        public SysDictCategoryController(ISysDictCategoryService sysDictCategoryService)
        {
            _sysDictCategoryService = sysDictCategoryService;
        }

        /// <summary>
        /// 创建新的字典分类。
        /// </summary>
        /// <param name="createDto">创建数据传输对象，包含 Code、Name 等字段。</param>
        /// <returns>返回创建成功的字典分类信息。</returns>
        /// <exception cref="ArgumentException">当 Code 或 Name 已存在时抛出。</exception>
        /// <exception cref="ArgumentNullException">当 createDto 为 null 时抛出。</exception>
        [HttpPost]
        public async Task<IActionResult> CreateAsync([FromBody] CreateSysDictCategoryDto createDto)
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

            var id = await _sysDictCategoryService.CreateAsync(createDto, currentUser.Id);
            var dto = await _sysDictCategoryService.GetAsync(id);
            var responseCreate = ApiResponse<SysDictCategoryDto>.OperationSuccess(dto);

            return Ok(responseCreate);
        }

        /// <summary>
        /// 批量创建字典分类。
        /// </summary>
        /// <param name="createDtos">创建 DTO 集合。</param>
        /// <returns>返回成功插入的记录数量；若集合为空，则返回 0。</returns>
        /// <exception cref="ArgumentException">当存在重复编码或名称时抛出。</exception>
        [HttpPost("batch")]
        public async Task<IActionResult> CreateBatchAsync([FromBody] List<CreateSysDictCategoryDto> createDtos)
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

            var createdCount = await _sysDictCategoryService.CreateBatchAsync(createDtos, currentUser.Id);
            var responseCreateBatch = ApiResponse<int>.OperationSuccess(createdCount);

            return Ok(responseCreateBatch);
        }

        /// <summary>
        /// 更新指定的字典分类。
        /// </summary>
        /// <param name="id">字典分类 ID。</param>
        /// <param name="updateDto">更新 DTO，必须包含有效 ID。</param>
        /// <returns>返回被更新的字典分类 ID。</returns>
        /// <exception cref="ArgumentException">当记录不存在、已删除或更新后违反唯一性约束时抛出。</exception>
        /// <exception cref="ArgumentNullException">当 updateDto 为 null 时抛出。</exception>
        /// <exception cref="InvalidOperationException">当数据库更新未影响任何行时抛出（并发冲突）。</exception>
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateAsync(long id, [FromBody] UpdateSysDictCategoryDto updateDto)
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

            var updatedId = await _sysDictCategoryService.UpdateAsync(updateDto, currentUser.Id);
            var responseUpdate = ApiResponse<long>.OperationSuccess(updatedId);

            return Ok(responseUpdate);
        }

        /// <summary>
        /// 批量更新字典分类。
        /// </summary>
        /// <param name="updateDtos">更新 DTO 集合，每个必须包含有效 ID。</param>
        /// <returns>返回成功更新的数量。</returns>
        /// <returns>返回所有更新操作受影响的总记录数；若集合为空，则返回 0。</returns>
        /// <exception cref="ArgumentException">当任一记录不存在或违反业务规则时抛出。</exception>
        [HttpPut("batch")]
        public async Task<IActionResult> UpdateBatchAsync([FromBody] List<UpdateSysDictCategoryDto> updateDtos)
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

            var updatedCount = await _sysDictCategoryService.UpdateBatchAsync(updateDtos, currentUser.Id);
            var responseUpdateBatch = ApiResponse<int>.OperationSuccess(updatedCount);

            return Ok(responseUpdateBatch);
        }

        /// <summary>
        /// 软删除指定的字典分类。
        /// </summary>
        /// <param name="id">要删除的字典分类 ID。</param>
        /// <returns>返回被删除的字典分类 ID。</returns>
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

            var deletedId = await _sysDictCategoryService.DeleteAsync(id, currentUser.Id);
            var responseDelete = ApiResponse<long>.OperationSuccess(deletedId);

            return Ok(responseDelete);
        }

        /// <summary>
        /// 批量软删除字典分类。
        /// </summary>
        /// <param name="batchRequestDto">包含 ID 列表的删除请求。</param>
        /// <returns>返回成功标记为删除的记录数量；若 ID 列表为空则返回 0。</returns>
        /// <exception cref="ArgumentException">当部分 ID 不存在或已被删除时可能抛出。</exception>

        [HttpDelete("batch")]
        public async Task<IActionResult> DeleteBatchAsync([FromBody] DeleteBatchRequest batchRequestDto)
        {
            if (batchRequestDto?.Ids == null || !batchRequestDto.Ids.Any())
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

            var deletedCount = await _sysDictCategoryService.DeleteBatchAsync(batchRequestDto.Ids, currentUser.Id);
            var responseDeleteBatch = ApiResponse<int>.OperationSuccess(deletedCount);

            return Ok(responseDeleteBatch);
        }

        /// <summary>
        /// 根据 ID 获取单条未删除的字典分类详情。
        /// </summary>
        /// <param name="id">字典分类 ID。</param>
        /// <returns>返回匹配的 DTO；若未找到或已删除，则返回 null。</returns>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetAsync(long id)
        {
            var dto = await _sysDictCategoryService.GetAsync(id);

            if (dto == null)
            {
                var response = ApiResponse<SysDictCategoryDto>.NotFound();
                return NotFound(response);
            }

            var responseGet = ApiResponse<SysDictCategoryDto>.OperationSuccess(dto);
            return Ok(responseGet);
        }

        /// <summary>
        /// 查询未删除的字典分类列表，支持按名称模糊搜索和状态精确筛选。
        /// </summary>
        /// <param name="name">字典分类名称，用于模糊搜索（可选）。</param>
        /// <param name="status">字典分类状态，用于精确匹配（可选）。</param>
        /// <returns>返回匹配条件的 DTO 列表（可能为空）。</returns>
        [HttpGet("all")]
        public async Task<IActionResult> GetAllAsync(
            [FromQuery] string? name = null,
            [FromQuery] string? status = null)
        {
            var dtos = await _sysDictCategoryService.GetAllAsync(name, status);
            var responseAll = ApiResponse<List<SysDictCategoryDto>>.OperationSuccess(dtos);

            return Ok(responseAll);
        }

        /// <summary>
        /// 分页查询未删除的字典分类列表，支持名称、类型模糊搜索和状态精确筛选。
        /// </summary>
        /// <param name="pageNumber">页码，从 1 开始。</param>
        /// <param name="pageSize">每页数量。</param>
        /// <param name="name">字典分类名称，用于模糊搜索（可选）。</param>
        /// <param name="code">字典分类编码，用于模糊搜索（可选）。</param>
        /// <param name="status">字典分类状态，用于精确匹配（可选）。</param>
        /// <param name="orderByField">排序字段（可选）。</param>
        /// <param name="orderByType">排序方式（可选）。</param>
        /// <returns>返回分页结果对象，包含数据和分页元信息。</returns>
        [HttpGet("paged")]
        public async Task<IActionResult> GetPagedAsync(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? name = null,
            [FromQuery] string? code = null,
            [FromQuery] string? status = null,
            [FromQuery] string? orderByField = null,
            [FromQuery] string? orderByType = null)
        {
            var pagedResult = await _sysDictCategoryService.GetPagedAsync(pageNumber, pageSize, name, code, status, orderByField, orderByType);
            var responsePaged = ApiResponse<PaginationResult<SysDictCategoryDto>>.OperationSuccess(pagedResult);

            return Ok(responsePaged);
        }

        /// <summary>
        /// 根据字典分类编码获取唯一未删除的字典分类详情。
        /// </summary>
        /// <param name="code">字典分类的唯一编码（如 gender），用于精确匹配。</param>
        /// <returns>返回匹配的 DTO；若未找到或已删除，则返回 null。</returns>
        [HttpGet("by-code/{code}")]
        public async Task<IActionResult> GetByCodeAsync(string code)
        {
            var dto = await _sysDictCategoryService.GetByCodeAsync(code);

            if (dto == null)
            {
                var response = ApiResponse<SysDictCategoryDto>.NotFound();
                return NotFound(response);
            }

            var responseByType = ApiResponse<SysDictCategoryDto>.OperationSuccess(dto);
            return Ok(responseByType);
        }

        /// <summary>
        /// 更新字典分类的状态。
        /// </summary>
        /// <param name="id">要更新的字典分类 ID。</param>
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

            var updatedId = await _sysDictCategoryService.ChangeStatusAsync(id, changeStatusDto.Status, currentUser.Id);
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