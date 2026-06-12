using Bedrock.WebApi.Responses;
using Bedrock.Application.DataTransferObjects;
using Bedrock.Application.Interfaces;
using Bedrock.Application.ValueObjects;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Bedrock.WebApi.Controllers
{
    /// <summary>
    /// 控制器用于处理与认证（Auth）资源相关的请求。
    /// 提供登录、刷新令牌、撤销刷新令牌和强制登出等功能。
    /// </summary>
    [ApiController]
    [Authorize]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        /// <summary>
        /// 构造函数注入 AuthService。
        /// </summary>
        /// <param name="authService">服务层接口，提供认证相关的业务逻辑。</param>
        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        /// <summary>
        /// 用户登录接口。
        /// 验证用户名和密码，并返回认证结果（包括访问令牌和刷新令牌）。
        /// </summary>
        /// <param name="loginDto">包含用户名和密码的登录数据传输对象。</param>
        /// <returns>返回登录成功的响应数据，或错误信息。</returns>
        [HttpPost]
        [Route("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
        {
            // 提取客户端信息
            var clientInfoVO = GetCurrentClientInfo();

            var loginResponseDto = await _authService.AuthenticateAsync(loginDto, clientInfoVO);
            var response = ApiResponse<LoginResponseDto>.OperationSuccess(loginResponseDto);

            return Ok(response);
        }

        /// <summary>
        /// 刷新访问令牌接口（匿名）。
        /// 使用有效的刷新令牌获取新的访问令牌。
        /// </summary>
        /// <param name="refreshTokenDto">包含刷新令牌的数据传输对象。</param>
        /// <returns>返回刷新成功后的新的访问令牌和刷新令牌。</returns>
        [HttpPost]
        [Route("refresh-token")]
        [AllowAnonymous]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenDto refreshTokenDto)
        {
            // 提取客户端信息
            var clientInfoVO = GetCurrentClientInfo();

            var loginResponseDto = await _authService.RefreshTokenAsync(refreshTokenDto.UserId, refreshTokenDto.RefreshTokenId, clientInfoVO);
            var responseRefresh = ApiResponse<LoginResponseDto>.OperationSuccess(loginResponseDto);

            return Ok(responseRefresh);
        }

        /// <summary>
        /// 撤销刷新令牌接口。
        /// 使指定的刷新令牌失效，防止其再次使用。
        /// </summary>
        /// <param name="refreshTokenDto">包含刷新令牌的数据传输对象。</param>
        /// <returns>返回操作成功的响应。</returns>
        [HttpPost]
        [Route("revoke-refresh-token")]
        [AllowAnonymous]
        public async Task<IActionResult> RevokeRefreshToken([FromBody] RefreshTokenDto refreshTokenDto)
        {
            await _authService.RevokeRefreshTokenAsync(refreshTokenDto.UserId, refreshTokenDto.RefreshTokenId);
            var responseRevoke = ApiResponse<RefreshTokenDto>.OperationSuccess(refreshTokenDto);
            return Ok(responseRevoke);
        }

        /// <summary>
        /// 强制登出用户接口。
        /// 通过用户ID强制撤销用户的会话，使其所有刷新令牌失效。
        /// </summary>
        /// <param name="forceLogoutDto">包含用户ID的数据传输对象。</param>
        /// <returns>返回操作成功的响应。</returns>
        [HttpPost]
        [Route("force-logout")]
        public async Task<IActionResult> ForceLogout([FromBody] ForceLogoutDto forceLogoutDto)
        {
            await _authService.ForceLogoutAsync(forceLogoutDto.UserId);
            var responseFourceLogout = ApiResponse<ForceLogoutDto>.OperationSuccess(forceLogoutDto);

            return Ok(responseFourceLogout);
        }

        /// <summary>
        /// 获取用户认证信息接口。
        /// 返回当前用户的认证信息。
        /// </summary>
        /// <returns>返回用户认证信息的响应数据。</returns>
        [HttpGet]
        [Route("info")]
        public async Task<IActionResult> GetAuthInfo()
        {
            // 从 Claims 获取 UserID
            var userId = long.Parse(
                HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                ?? throw new UnauthorizedAccessException("用户未登录")
            );

            var authInfoDto = await _authService.GetAuthInfoAsync(userId);
            var responseGet = ApiResponse<AuthInfoDto>.OperationSuccess(authInfoDto);
            return Ok(responseGet);
        }

        /// <summary>
        /// 用户主动登出接口。
        /// 撤销当前用户的所有令牌。
        /// </summary>
        /// <returns>返回操作成功的响应。</returns>
        [HttpPost]
        [Route("logout")]
        public async Task<IActionResult> Logout()
        {
            // 从 Claims 获取 UserID
            var userId = long.Parse(
                HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                ?? throw new UnauthorizedAccessException("用户未登录")
            );

            await _authService.ForceLogoutAsync(userId);

            var responseLogout = ApiResponse<object>.OperationSuccess(null, "Logout successful.");
            return Ok(responseLogout);
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

        /// <summary>
        /// 从当前 HTTP 请求上下文中提取客户端信息，包括 User-Agent、IP 地址和设备类型。
        /// </summary>
        /// <returns>包含客户端 User-Agent、IP 地址和设备类型的 <see cref="ClientInfoVO"/> 对象。</returns>
        /// <remarks>
        /// 此方法应在 Controller 层调用，避免在 Service 层直接依赖 HttpContext。
        /// 提取的信息可用于会话管理、安全审计或“我的设备”功能。
        /// </remarks>
        private ClientInfoVO GetCurrentClientInfo()
        {
            var userAgent = HttpContext.Request.Headers["User-Agent"].ToString();
            var ipAddress = GetClientIpAddress();
            var device = ParseDeviceFromUserAgent(userAgent);

            return new ClientInfoVO
            {
                UserAgent = userAgent,
                IpAddress = ipAddress,
                Device = device
            };
        }

        /// <summary>
        /// 获取客户端真实 IP 地址，支持通过反向代理（如 Nginx）传递的 X-Forwarded-For 头。
        /// </summary>
        /// <returns>客户端 IP 地址字符串，若无法获取则返回 null。</returns>
        /// <remarks>
        /// 优先从 <c>X-Forwarded-For</c> 请求头获取 IP（适用于部署在反向代理后的情况），
        /// 若不存在，则回退到直接连接的远程 IP 地址。
        /// </remarks>
        private string? GetClientIpAddress()
        {
            var forwardedFor = HttpContext.Request.Headers["X-Forwarded-For"].FirstOrDefault();
            if (!string.IsNullOrWhiteSpace(forwardedFor))
            {
                return forwardedFor.Split(',').FirstOrDefault()?.Trim();
            }

            return HttpContext.Connection.RemoteIpAddress?.ToString();
        }

        /// <summary>
        /// 根据 User-Agent 字符串解析客户端设备类型。
        /// </summary>
        /// <param name="userAgent">客户端 User-Agent 字符串。</param>
        /// <returns>识别出的设备类型名称（如 "iPhone", "Android", "Windows" 等），若无法识别则返回 "Unknown"。</returns>
        /// <remarks>
        /// 本方法通过关键字匹配简单识别设备类型，适用于“我的设备”等展示场景。
        /// 不保证 100% 准确，如需精确识别建议使用专用库（如 UAParser）。
        /// </remarks>
        private string? ParseDeviceFromUserAgent(string? userAgent)
        {
            if (string.IsNullOrWhiteSpace(userAgent)) return "Unknown";

            userAgent = userAgent.ToLowerInvariant();

            if (userAgent.Contains("iphone")) return "iPhone";
            if (userAgent.Contains("ipad")) return "iPad";
            if (userAgent.Contains("android")) return "Android";
            if (userAgent.Contains("windows")) return "Windows";
            if (userAgent.Contains("macintosh") || userAgent.Contains("mac os")) return "Mac";
            if (userAgent.Contains("linux")) return "Linux";

            return "Web Browser";
        }

        #endregion
    }
}