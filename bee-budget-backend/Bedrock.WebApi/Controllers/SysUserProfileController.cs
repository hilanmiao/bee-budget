using Bedrock.Application.DataTransferObjects;
using Bedrock.Application.Interfaces;
using Bedrock.WebApi.Responses;
using Microsoft.AspNetCore.Mvc;

namespace Bedrock.WebApi.Controllers
{
    /// <summary>
    /// 个人信息接口
    /// </summary>
    [ApiController]
    [Route("api/sys-user/profile")]
    public class SysUserProfileController : ControllerBase
    {
        private readonly ISysUserService _sysUserService;
        private readonly IFileService _fileService;

        /// <summary>
        /// 构造函数注入服务层依赖。
        /// </summary>
        /// <param name="sysUserService">系统用户服务接口</param>
        /// <param name="fileService">文件服务接口，提供文件上传和处理功能。</param>
        public SysUserProfileController(ISysUserService sysUserService, IFileService fileService)
        {
            _sysUserService = sysUserService;
            _fileService = fileService;
        }

        /// <summary>
        /// 根据ID获取单个用户记录。
        /// </summary>
        [HttpGet]
        [Route("")]
        public async Task<IActionResult> Get()
        {
            var currentUser = GetCurrentUser();
            if (currentUser == null)
            {
                var response = ApiResponse<SysUserDto>.Unauthorized();
                return Unauthorized(response);
            }

            var dto = await _sysUserService.GetAsync(currentUser.Id);
            if (dto == null)
            {
                var response = ApiResponse<SysUserDto>.NotFound();
                return NotFound(response);
            }
            var successResponse = ApiResponse<SysUserDto>.OperationSuccess(dto);
            return Ok(successResponse);
        }

        /// <summary>
        /// 更新个人信息
        /// </summary>
        [HttpPut]
        [Route("")]
        public async Task<IActionResult> Update([FromBody] UpdateSysUserProfileDto updateDto)
        {
            var currentUser = GetCurrentUser();
            if (currentUser == null)
            {
                var response = ApiResponse<SysUserDto>.Unauthorized();
                return Unauthorized(response);
            }

            updateDto.Id = currentUser.Id;
            await _sysUserService.UpdateProfileAsync(updateDto, currentUser.Id);
            var responseUpdate = ApiResponse<long>.OperationSuccess(updateDto.Id);

            return Ok(responseUpdate);
        }

        /// <summary>
        /// 更新个人密码
        /// </summary>
        /// <param name="changePasswordDto"></param>
        /// <returns></returns>
        [HttpPut]
        [Route("password")]
        public async Task<IActionResult> UpdatePassword([FromBody] ChangeUserProfilePasswordDto changePasswordDto)
        {
            var currentUser = GetCurrentUser();
            if (currentUser == null)
            {
                var response = ApiResponse<SysUserDto>.Unauthorized();
                return Unauthorized(response);
            }

            var updatedId = await _sysUserService.UpdateProfilePasswordAsync(currentUser.Id, changePasswordDto, currentUser.Id);
            var responseUpdate = ApiResponse<long>.OperationSuccess(updatedId);

            return Ok(responseUpdate);
        }

        /// <summary>
        /// 上传头像。
        /// </summary>
        /// <param name="avatarFile">要上传的头像文件。（参数名也要是avatarFile和前端一致才能自动映射）</param>
        /// <returns>返回包含文件路径的 ApiResponse。</returns>
        [HttpPost]
        [Route("avatar")]
        public async Task<IActionResult> UploadAvatar(IFormFile avatarFile)
        {
            var currentUser = GetCurrentUser();
            if (currentUser == null)
            {
                var response = ApiResponse<SysUserDto>.Unauthorized();
                return Unauthorized(response);
            }

            var url = await _fileService.UploadImageAsync(avatarFile);
            var updatedId = await _sysUserService.UpdateProfileAvatarAsync(currentUser.Id, url, currentUser.Id);
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