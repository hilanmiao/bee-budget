using Bedrock.WebApi.Responses;
using Bedrock.Application.DataTransferObjects;
using Bedrock.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Bedrock.WebApi.Controllers
{
    /// <summary>
    /// 控制器用于管理用户（SysUser）的增删改查及批量操作。
    /// </summary>
    [ApiController]
    [Authorize]
    [Route("api/sys-user")]
    public class SysUserController : ControllerBase
    {
        private readonly ISysUserService _sysUserService;

        /// <summary>
        /// 构造函数注入服务层依赖。
        /// </summary>
        /// <param name="sysUserService">用户业务逻辑服务。</param>
        public SysUserController(ISysUserService sysUserService)
        {
            _sysUserService = sysUserService;
        }

        /// <summary>
        /// 创建新的用户。
        /// </summary>
        /// <param name="createDto">创建用户的 DTO。</param>
        /// <returns>返回创建成功的用户信息。</returns>
        [HttpPost]
        public async Task<IActionResult> CreateAsync([FromBody] CreateSysUserDto createDto)
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

            var id = await _sysUserService.CreateAsync(createDto, currentUser.Id);
            var dto = await _sysUserService.GetAsync(id);
            var responseCreate = ApiResponse<SysUserDto>.OperationSuccess(dto);

            return Ok(responseCreate);
        }

        /// <summary>
        /// 更新指定 ID 的用户。
        /// </summary>
        /// <param name="id">用户 ID。</param>
        /// <param name="updateDto">更新 DTO。</param>
        /// <returns>返回被更新的用户 ID。</returns>
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateAsync(long id, [FromBody] UpdateSysUserDto updateDto)
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

            var updatedId = await _sysUserService.UpdateAsync(updateDto, currentUser.Id);
            var responseUpdate = ApiResponse<long>.OperationSuccess(updatedId);

            return Ok(responseUpdate);
        }

        /// <summary>
        /// 删除指定 ID 的用户（软删除）。
        /// </summary>
        /// <param name="id">用户 ID。</param>
        /// <returns>返回被删除的用户 ID。</returns>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAsync(long id)
        {
            var currentUser = GetCurrentUser();
            if (currentUser == null)
            {
                var response = ApiResponse<SysUserDto>.Unauthorized();
                return Unauthorized(response);
            }

            var deletedId = await _sysUserService.DeleteAsync(id, currentUser.Id);
            var responseDelete = ApiResponse<long>.OperationSuccess(deletedId);

            return Ok(responseDelete);
        }

        /// <summary>
        /// 批量删除用户（软删除）。
        /// </summary>
        /// <param name="batchRequestDto">包含 ID 列表的删除请求。</param>
        /// <returns>返回被删除的记录数量。</returns>
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

            var deletedCount = await _sysUserService.DeleteBatchAsync(batchRequestDto.Ids, currentUser.Id);
            var responseDeleteBatch = ApiResponse<int>.OperationSuccess(deletedCount);

            return Ok(responseDeleteBatch);
        }

        /// <summary>
        /// 根据 ID 获取用户详情。
        /// </summary>
        /// <param name="id">用户 ID。</param>
        /// <returns>返回用户详情。</returns>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetAsync(long id)
        {
            //var dto = await _sysUserService.GetAsync(id);
            var dto = await _sysUserService.GetSummaryAsync(id);

            if (dto == null)
            {
                //var response = ApiResponse<SysUserDto>.NotFound();
                var response = ApiResponse<SysUserSummary>.NotFound();
                return NotFound(response);
            }

            //var responseGet = ApiResponse<SysUserDto>.OperationSuccess(dto);
            var responseGet = ApiResponse<SysUserSummary>.OperationSuccess(dto);
            return Ok(responseGet);
        }

        /// <summary>
        /// 获取所有用户（支持按名称和状态过滤）。
        /// </summary>
        /// <param name="userName">用户名称（模糊匹配）。</param>
        /// <param name="status">状态（启用/禁用）。</param>
        /// <returns>返回所有匹配的用户列表。</returns>
        [HttpGet("all")]
        public async Task<IActionResult> GetAllAsync(
            [FromQuery] string? userName = null,
            [FromQuery] string? status = null)
        {
            //var dtos = await _sysUserService.GetAllAsync(userName, status);
            //var responseAll = ApiResponse<List<SysUserDto>>.OperationSuccess(dtos);
            var dtos = await _sysUserService.GetAllSummaryAsync(userName, status);
            var responseAll = ApiResponse<List<SysUserSummary>>.OperationSuccess(dtos);

            return Ok(responseAll);
        }

        /// <summary>
        /// 分页获取用户列表（支持多条件过滤）。
        /// </summary>
        /// <param name="pageNumber">页码，从 1 开始。</param>
        /// <param name="pageSize">每页数量。</param>
        /// <param name="userName">用户名称，模糊搜索（可选）。</param>
        /// <param name="phoneNumber">用户手机号码，模糊搜索（可选）。</param>
        /// <param name="status">用户状态，精确匹配（可选）。</param>
        /// <param name="orderByField">排序字段（可选）。</param>
        /// <param name="orderByType">排序方式（可选）。</param>
        /// <returns>返回分页结果对象，包含数据和分页元信息。</returns>
        [HttpGet("paged")]
        public async Task<IActionResult> GetPagedAsync(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? userName = null,
            [FromQuery] string? phoneNumber = null,
            [FromQuery] string? status = null,
            [FromQuery] string? orderByField = null,
            [FromQuery] string? orderByType = null)
        {
            //var pagedResult = await _sysUserService.GetPagedAsync(pageNumber, pageSize, userName, phoneNumber, status);
            //var responsePaged = ApiResponse<PaginationResult<SysUserDto>>.OperationSuccess(pagedResult);
            var pagedResult = await _sysUserService.GetPagedSummaryAsync(pageNumber, pageSize, userName, phoneNumber, status, orderByField, orderByType);
            var responsePaged = ApiResponse<PaginationResult<SysUserSummary>>.OperationSuccess(pagedResult);

            return Ok(responsePaged);
        }

        /// <summary>
        /// 根据用户名称获取单条记录。
        /// </summary>
        /// <param name="userName">用户名称（如 'admin'）。</param>
        /// <returns>返回匹配的用户。</returns>
        [HttpGet("by-user-name/{userName}")]
        public async Task<IActionResult> GetByUserNameAsync(string userName)
        {
            var dto = await _sysUserService.GetByUserNameAsync(userName);

            if (dto == null)
            {
                var response = ApiResponse<SysUserDto>.NotFound();
                return NotFound(response);
            }

            var responseGet = ApiResponse<SysUserDto>.OperationSuccess(dto);
            return Ok(responseGet);
        }

        /// <summary>
        /// 更新用户的状态。
        /// </summary>
        /// <param name="id">要更新的用户 ID。</param>
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

            var updatedId = await _sysUserService.ChangeStatusAsync(id, changeStatusDto.Status, currentUser.Id);
            var responseUpdate = ApiResponse<long>.OperationSuccess(updatedId);
            return Ok(responseUpdate);
        }

        /// <summary>
        /// 重置用户密码。
        /// </summary>
        /// <param name="id">要更新的用户 ID。</param>
        /// <param name="resetUserPasswordDto">数据传输对象</param>
        /// <returns>返回被更新的记录 ID。</returns>
        [HttpPut]
        [Route("reset-password/{id}")]
        public async Task<IActionResult> ResetPassword(long id, [FromBody] ResetUserPasswordDto resetUserPasswordDto)
        {
            if (resetUserPasswordDto == null)
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

            var updatedId = await _sysUserService.ResetPasswordAsync(id, resetUserPasswordDto, currentUser.Id);
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