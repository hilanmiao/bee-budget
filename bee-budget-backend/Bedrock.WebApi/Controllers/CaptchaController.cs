using Bedrock.WebApi.Responses;
using Bedrock.Application.DataTransferObjects;
using Bedrock.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Bedrock.WebApi.Controllers
{
    /// <summary>
    /// 验证码控制器，用于生成和验证图形验证码。
    /// </summary>
    [ApiController]
    [AllowAnonymous]
    [Route("api/captcha")]
    public class CaptchaController : ControllerBase
    {
        private readonly ICaptchaService _captchaService;

        /// <summary>
        /// 构造函数注入验证码服务。
        /// </summary>
        /// <param name="captchaService">验证码业务逻辑服务。</param>
        public CaptchaController(ICaptchaService captchaService)
        {
            _captchaService = captchaService;
        }

        /// <summary>
        /// 生成新的图形验证码。
        /// </summary>
        /// <returns>
        /// 返回包含验证码 ID 和 Base64 编码图像的响应。
        /// 前端可直接将 ImageBase64 赋值给 &lt;img src=""&gt; 显示。
        /// </returns>
        /// <response code="200">成功生成验证码。</response>
        [HttpGet]
        [Route("")]
        public async Task<IActionResult> Generate()
        {
            var captchaDto = await _captchaService.GenerateAsync();
            var response = ApiResponse<CaptchaDto>.OperationSuccess(captchaDto);
            return Ok(response);
        }

        /// <summary>
        /// 验证用户输入的验证码是否正确。
        /// </summary>
        /// <param name="dto">包含验证码 ID 和用户输入的 DTO。</param>
        /// <returns>
        /// 验证通过返回 true，否则返回 false。
        /// 验证成功后，该验证码将失效（防止重放攻击）。
        /// </returns>
        /// <response code="200">返回验证结果。</response>
        /// <response code="400">输入参数无效。</response>
        [HttpPost]
        [Route("validate")]
        public async Task<IActionResult> Validate([FromBody] ValidateCaptchaDto dto)
        {
            if (dto == null || string.IsNullOrWhiteSpace(dto.CaptchaId) || string.IsNullOrWhiteSpace(dto.CaptchaContent))
            {
                var errorResponse = ApiResponse<bool>.BadRequest("验证码 ID 或输入内容不能为空。");
                return BadRequest(errorResponse);
            }

            var isValid = await _captchaService.ValidateAsync(dto.CaptchaId, dto.CaptchaContent);
            if (isValid)
            {
                return Ok(ApiResponse<bool>.OperationSuccess(true, "验证通过"));
            }
            else
            {
                return Ok(ApiResponse<bool>.OperationFailure(false, "验证码错误或已过期"));
            }
        }
    }

}