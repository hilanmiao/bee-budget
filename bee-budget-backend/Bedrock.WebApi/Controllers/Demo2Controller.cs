using Bedrock.WebApi.Responses;
using Bedrock.Application.DataTransferObjects;
using Bedrock.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Bedrock.WebApi.Controllers
{
    /// <summary>
    /// 样例2控制器。
    /// </summary>
    [ApiController]
    [Authorize]
    [Route("api/demo2")]
    public class Demo2Controller : ControllerBase
    {
        private readonly IDemo2Service _demo2Service;

        /// <summary>
        /// 构造函数注入服务层依赖。
        /// </summary>
        /// <param name="demo2Service">样例2业务逻辑服务。</param>
        public Demo2Controller(IDemo2Service demo2Service)
        {
            _demo2Service = demo2Service;
        }

        /// <summary>
        /// 创建新的样例2。
        /// </summary>
        /// <param name="createDto">创建数据传输对象，包含 Demo1Id 等字段。</param>
        /// <returns>返回创建成功的样例2信息。</returns>
        /// <exception cref="ArgumentException">当 Demo1Id 和 Code 已存在时抛出。</exception>
        /// <exception cref="ArgumentNullException">当 createDto 为 null 时抛出。</exception>
        [HttpPost]
        public async Task<IActionResult> CreateAsync([FromBody] CreateDemo2Dto createDto)
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

            var id = await _demo2Service.CreateAsync(createDto, currentUser.Id);
            var dto = await _demo2Service.GetAsync(id);
            var responseCreate = ApiResponse<Demo2Dto>.OperationSuccess(dto);

            return Ok(responseCreate);
        }

        /// <summary>
        /// 更新指定的样例2。
        /// </summary>
        /// <param name="id">样例2 ID。</param>
        /// <param name="updateDto">更新 DTO，必须包含有效 ID。</param>
        /// <returns>返回被更新的样例2 ID。</returns>
        /// <exception cref="ArgumentException">当记录不存在、已删除或更新后违反唯一性约束时抛出。</exception>
        /// <exception cref="ArgumentNullException">当 updateDto 为 null 时抛出。</exception>
        /// <exception cref="InvalidOperationException">当数据库更新未影响任何行时抛出（并发冲突）。</exception>
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateAsync(long id, [FromBody] UpdateDemo2Dto updateDto)
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

            var updatedId = await _demo2Service.UpdateAsync(updateDto, currentUser.Id);
            var responseUpdate = ApiResponse<long>.OperationSuccess(updatedId);

            return Ok(responseUpdate);
        }

        /// <summary>
        /// 软删除指定的样例2。
        /// </summary>
        /// <param name="id">要删除的样例2 ID。</param>
        /// <returns>返回被删除的样例2 ID。</returns>
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

            var deletedId = await _demo2Service.DeleteAsync(id, currentUser.Id);
            var responseDelete = ApiResponse<long>.OperationSuccess(deletedId);

            return Ok(responseDelete);
        }

        /// <summary>
        /// 批量删除样例2（软删除）。
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

            var deletedCount = await _demo2Service.DeleteBatchAsync(request.Ids, currentUser.Id);
            var responseDeleteBatch = ApiResponse<int>.OperationSuccess(deletedCount);

            return Ok(responseDeleteBatch);
        }

        /// <summary>
        /// 更新样例2的状态。
        /// </summary>
        /// <param name="id">要更新的样例2 ID。</param>
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

            var updatedId = await _demo2Service.ChangeStatusAsync(id, changeStatusDto.Status, currentUser.Id);
            var responseUpdate = ApiResponse<long>.OperationSuccess(updatedId);
            return Ok(responseUpdate);
        }

        ///// <summary>
        ///// 根据 ID 获取单条未删除的样例2详情。
        ///// </summary>
        ///// <param name="id">样例2 ID。</param>
        ///// <returns>返回匹配的 DTO；若未找到或已删除，则返回 null。</returns>
        //[HttpGet("{id}")]
        //public async Task<IActionResult> GetAsync(long id)
        //{
        //    var dto = await _demo2Service.GetAsync(id);

        //    if (dto == null)
        //    {
        //        var response = ApiResponse<Demo2Dto>.NotFound();
        //        return NotFound(response);
        //    }

        //    var responseGet = ApiResponse<Demo2Dto>.OperationSuccess(dto);
        //    return Ok(responseGet);
        //}

        ///// <summary>
        ///// 查询未删除的样例2记录，支持按名称模糊搜索、状态和Demo1Id精确搜索。
        ///// </summary>
        ///// <param name="name"> 样例2名称，用于模糊搜索（可选）。</param>
        ///// <param name="status"> 样例2状态，用于模糊搜索（可选）。</param>
        ///// <param name="demo1Id"> 样例2Demo1Id，用于精确搜索（可选）。</param>
        ///// <param name="startDate">开始日期，用于日期范围搜索（可选）。</param>
        ///// <param name="endDate">结束日期，用于日期范围搜索（可选）。</param>
        ///// <returns>返回匹配条件的 DTO 列表（可能为空）。</returns>
        //[HttpGet("all")]
        //public async Task<IActionResult> GetAllAsync(
        //    [FromQuery] string? name = null,
        //    [FromQuery] string? status = null,
        //    [FromQuery] long? demo1Id = null,
        //    [FromQuery] DateTime? startDate = null,
        //    [FromQuery] DateTime? endDate = null)
        //{
        //    var dtos = await _demo2Service.GetAllAsync(name, status, demo1Id, startDate, endDate);
        //    var responseAll = ApiResponse<List<Demo2Dto>>.OperationSuccess(dtos);

        //    return Ok(responseAll);
        //}

        ///// <summary>
        ///// 分页查询未删除的样例2记录，支持名称模糊搜索、状态和Demo1Id精确搜索。
        ///// </summary>
        ///// <param name="pageNumber">页码，从 1 开始。</param>
        ///// <param name="pageSize">每页记录数。</param>
        ///// <param name="name">样例2名称，用于模糊搜索（可选）。</param>
        ///// <param name="status">样例2状态，用于模糊搜索（可选）。</param>
        ///// <param name="demo1Id">样例2Demo1Id，用于精确搜索（可选）。</param>
        ///// <param name="startDate">开始日期，用于日期范围搜索（可选）。</param>
        ///// <param name="endDate">结束日期，用于日期范围搜索（可选）。</param>
        ///// <param name="orderByField">排序字段（可选）。</param>
        ///// <param name="orderByType">排序方式（可选）。</param>
        ///// <returns>返回分页结果对象，包含数据和分页元信息。</returns>
        //[HttpGet("paged")]
        //public async Task<IActionResult> GetPagedAsync(
        //    [FromQuery] int pageNumber = 1,
        //    [FromQuery] int pageSize = 10,
        //    [FromQuery] string? name = null,
        //    [FromQuery] string? status = null,
        //    [FromQuery] long? demo1Id = null,
        //    [FromQuery] string? orderByField = null,
        //    [FromQuery] string? orderByType = null,
        //    [FromQuery] DateTime? startDate = null,
        //    [FromQuery] DateTime? endDate = null)
        //{
        //    var pagedResult = await _demo2Service.GetPagedAsync(pageNumber, pageSize, name, status, demo1Id, startDate, endDate, orderByField, orderByType);
        //    var responsePaged = ApiResponse<PaginationResult<Demo2Dto>>.OperationSuccess(pagedResult);

        //    return Ok(responsePaged);
        //}

        /// <summary>
        /// 根据 ID 获取单条未删除的样例2详情。
        /// </summary>
        /// <param name="id">样例2 ID。</param>
        /// <returns>返回匹配的 DTO；若未找到或已删除，则返回 null。</returns>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetAsync(long id)
        {
            var dto = await _demo2Service.GetSummaryAsync(id);

            if (dto == null)
            {
                var response = ApiResponse<Demo2Summary>.NotFound();
                return NotFound(response);
            }

            var responseGet = ApiResponse<Demo2Summary>.OperationSuccess(dto);
            return Ok(responseGet);
        }

        /// <summary>
        /// 查询未删除的样例2记录，支持按Demo1Id精确搜索、状态和Demo1Id精确搜索。
        /// </summary>
        /// <param name="name">样例2名称，用于模糊搜索（可选）。</param>
        /// <param name="status">样例2状态，用于模糊搜索（可选）。</param>
        /// <param name="demo1Id">样例2Demo1Id，用于精确搜索（可选）。</param>
        /// <param name="startDate">开始日期，用于日期范围搜索（可选）。</param>
        /// <param name="endDate">结束日期，用于日期范围搜索（可选）。</param>
        /// <returns>返回匹配条件的 DTO 列表（可能为空）。</returns>
        [HttpGet("all")]
        public async Task<IActionResult> GetAllAsync(
            [FromQuery] string? name = null,
            [FromQuery] string? status = null,
            [FromQuery] long? demo1Id = null,
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null)
        {
            var dtos = await _demo2Service.GetAllSummaryAsync(name, status, demo1Id, startDate, endDate);
            var responseAll = ApiResponse<List<Demo2Summary>>.OperationSuccess(dtos);

            return Ok(responseAll);
        }

        /// <summary>
        /// 分页查询未删除的样例2记录，支持名称模糊搜索、状态和Demo1Id精确搜索。
        /// </summary>
        /// <param name="pageNumber">页码，从 1 开始。</param>
        /// <param name="pageSize">每页记录数。</param>
        /// <param name="name">样例2名称，用于模糊搜索（可选）。</param>
        /// <param name="status">样例2状态，用于模糊搜索（可选）。</param>
        /// <param name="demo1Id">样例2Demo1Id，用于精确搜索（可选）。</param>
        /// <param name="startDate">开始日期，用于日期范围搜索（可选）。</param>
        /// <param name="endDate">结束日期，用于日期范围搜索（可选）。</param>
        /// <param name="orderByField">排序字段（可选）。</param>
        /// <param name="orderByType">排序方式（可选）。</param>
        /// <returns>返回分页结果对象，包含数据和分页元信息。</returns>
        [HttpGet("paged")]
        public async Task<IActionResult> GetPagedAsync(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? name = null,
            [FromQuery] string? status = null,
            [FromQuery] long? demo1Id = null,
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null,
            [FromQuery] string? orderByField = null,
            [FromQuery] string? orderByType = null)
        {
            var pagedResult = await _demo2Service.GetPagedSummaryAsync(pageNumber, pageSize, name, status, demo1Id, startDate, endDate, orderByField, orderByType);
            var responsePaged = ApiResponse<PaginationResult<Demo2Summary>>.OperationSuccess(pagedResult);

            return Ok(responsePaged);
        }

        /// <summary>
        /// 根据样例2Demo1Id查询未删除的样例2记录。
        /// </summary>
        /// <param name="demo1Id">样例2Demo1Id。</param>
        /// <returns>返回匹配条件的 DTO 列表（可能为空）。</returns>
        [HttpGet("by-demo1-id/{demo1Id}")]
        public async Task<IActionResult> GetAllByDemo1Id(long demo1Id)
        {
            var dtos = await _demo2Service.GetAllByDemo1IdAsync(demo1Id);
            var responseAll = ApiResponse<List<Demo2Dto>>.OperationSuccess(dtos);

            return Ok(responseAll);
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