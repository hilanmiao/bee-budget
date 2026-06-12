using Bedrock.WebApi.Responses;
using Bedrock.Application.DataTransferObjects;
using Bedrock.Application.Interfaces;
using Bedrock.Application.ValueObjects;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Bedrock.WebApi.Controllers
{
    /// <summary>
    /// 系统菜单控制器。
    /// </summary>
    [ApiController]
    [Authorize]
    [Route("api/sys-menu")]
    public class SysMenuController : ControllerBase
    {
        private readonly ISysMenuService _sysMenuService;

        /// <summary>
        /// 构造函数注入服务层依赖。
        /// </summary>
        /// <param name="sysMenuService">菜单业务逻辑服务。</param>
        public SysMenuController(ISysMenuService sysMenuService)
        {
            _sysMenuService = sysMenuService;
        }

        /// <summary>
        /// 创建新的菜单。
        /// </summary>
        /// <param name="createDto">创建数据传输对象，包含 Name 等字段。</param>
        /// <returns>返回创建成功的菜单信息。</returns>
        /// <exception cref="ArgumentException">当 Name 已存在时抛出。</exception>
        /// <exception cref="ArgumentNullException">当 createDto 为 null 时抛出。</exception>
        [HttpPost]
        public async Task<IActionResult> CreateAsync([FromBody] CreateSysMenuDto createDto)
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

            var id = await _sysMenuService.CreateAsync(createDto, currentUser.Id);
            var dto = await _sysMenuService.GetAsync(id);
            var responseCreate = ApiResponse<SysMenuDto>.OperationSuccess(dto);

            return Ok(responseCreate);
        }

        /// <summary>
        /// 更新指定的菜单。
        /// </summary>
        /// <param name="id">菜单 ID。</param>
        /// <param name="updateDto">更新 DTO，必须包含有效 ID。</param>
        /// <returns>返回被更新的菜单 ID。</returns>
        /// <exception cref="ArgumentException">当记录不存在、已删除或更新后违反唯一性约束时抛出。</exception>
        /// <exception cref="ArgumentNullException">当 updateDto 为 null 时抛出。</exception>
        /// <exception cref="InvalidOperationException">当数据库更新未影响任何行时抛出（并发冲突）。</exception>
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateAsync(long id, [FromBody] UpdateSysMenuDto updateDto)
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

            var updatedId = await _sysMenuService.UpdateAsync(updateDto, currentUser.Id);
            var responseUpdate = ApiResponse<long>.OperationSuccess(updatedId);

            return Ok(responseUpdate);
        }

        /// <summary>
        /// 软删除指定的菜单。
        /// </summary>
        /// <param name="id">要删除的菜单 ID。</param>
        /// <returns>返回被删除的菜单 ID。</returns>
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

            var deletedId = await _sysMenuService.DeleteAsync(id, currentUser.Id);
            var responseDelete = ApiResponse<long>.OperationSuccess(deletedId);

            return Ok(responseDelete);
        }

        /// <summary>
        /// 根据 ID 获取单条未删除的菜单详情。
        /// </summary>
        /// <param name="id">菜单 ID。</param>
        /// <returns>返回匹配的 DTO；若未找到或已删除，则返回 null。</returns>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetAsync(long id)
        {
            var dto = await _sysMenuService.GetAsync(id);

            if (dto == null)
            {
                var response = ApiResponse<SysMenuDto>.NotFound();
                return NotFound(response);
            }

            var responseGet = ApiResponse<SysMenuDto>.OperationSuccess(dto);
            return Ok(responseGet);
        }

        /// <summary>
        /// 查询未删除的菜单列表，支持按名称模糊搜索和状态精确筛选。
        /// </summary>
        /// <param name="name">菜单名称，用于模糊搜索（可选）。</param>
        /// <param name="status">菜单状态，用于精确匹配（可选）。</param>
        /// <returns>返回匹配条件的 DTO 列表（可能为空）。</returns>
        [HttpGet("all")]
        public async Task<IActionResult> GetAllAsync(
            [FromQuery] string? name = null,
            [FromQuery] string? status = null)
        {
            var dtos = await _sysMenuService.GetAllAsync(name, status);
            var responseAll = ApiResponse<List<SysMenuDto>>.OperationSuccess(dtos);

            return Ok(responseAll);
        }

        /// <summary>
        /// 根据菜单名称获取唯一未删除的菜单详情。
        /// </summary>
        /// <param name="name">菜单的唯一名称（如 用户管理），用于精确匹配。</param>
        /// <returns>返回匹配的 DTO；若未找到或已删除，则返回 null。</returns>
        [HttpGet("by-name/{name}")]
        public async Task<IActionResult> GetByNameAsync(string name)
        {
            var dto = await _sysMenuService.GetByNameAsync(name);

            if (dto == null)
            {
                var response = ApiResponse<SysMenuDto>.NotFound();
                return NotFound(response);
            }

            var responseByType = ApiResponse<SysMenuDto>.OperationSuccess(dto);
            return Ok(responseByType);
        }

        /// <summary>
        /// 根据用户 ID 获取树形选择控件数据。
        /// </summary>
        /// <param name="userId">用户 ID。</param>
        /// <returns>返回匹配条件的 DTO 列表（可能为空）。</returns>
        [HttpGet("tree-select-data/by-user-id/{userId}")]
        public async Task<IActionResult> GetTreeSelectDataByUserId(long userId)
        {
            var dtos = await _sysMenuService.GetTreeSelectDataByUserId(userId);
            var responseGet = ApiResponse<List<TreeSelectDto>>.OperationSuccess(dtos);

            return Ok(responseGet);
        }

        /// <summary>
        /// 根据角色 ID 获取树形选择控件数据。
        /// </summary>
        /// <param name="roleId">角色 ID。</param>
        /// <returns>返回匹配的 DTO；若未找到或已删除，则返回 null。</returns>
        [HttpGet("tree-select-data/by-role-id/{roleId}")]
        public async Task<IActionResult> GetTreeSelectDataByRoleId(long roleId)
        {
            var dto = await _sysMenuService.GetTreeSelectDataByRoleId(roleId);

            if (dto == null)
            {
                var response = ApiResponse<RoleMenuTreeSelectDto>.NotFound();
                return NotFound(response);
            }

            var responseGet = ApiResponse<RoleMenuTreeSelectDto>.OperationSuccess(dto);

            return Ok(responseGet);
        }

        /// <summary>
        /// 根据用户 ID 获取路由数据。
        /// </summary>
        /// <param name="userId">用户 ID。</param>
        /// <returns>返回匹配条件的 DTO 列表（可能为空）。</returns>
        [HttpGet("router-data/by-user-id/{userId}")]
        public async Task<IActionResult> GetRouterDataByUserId(long userId)
        {
            var currentUser = GetCurrentUser();
            if (currentUser == null)
            {
                var response = ApiResponse<SysUserDto>.Unauthorized();
                return Unauthorized(response);
            }

            var dtos = await _sysMenuService.GetRouterDataByUserId(currentUser.Id);
            var responseGet = ApiResponse<List<RouterVO>>.OperationSuccess(dtos);

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