using Bedrock.Configuration;
using Bedrock.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text;

namespace Bedrock.WebApi.Middleware
{
    /// <summary>
    /// JWT 中间件，用于验证请求中的 JWT 访问令牌。
    /// 通过解析和验证 JWT，确保用户身份合法，并将用户信息附加到 HttpContext 中以供后续使用。
    /// </summary>
    public class JwtMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly JwtConfig _jwtConfig;

        /// <summary>
        /// 构造函数，注入下一个中间件委托和 JWT 配置。
        /// </summary>
        /// <param name="next">下一个中间件在管道中的委托。</param>
        /// <param name="jwtConfig">JWT 配置选项。</param>
        public JwtMiddleware(RequestDelegate next, IOptions<JwtConfig> jwtConfig)
        {
            _next = next;
            _jwtConfig = jwtConfig.Value;
        }

        /// <summary>
        /// 中间件处理方法，负责解析和验证请求中的 JWT 访问令牌。
        /// 如果验证成功，将用户信息附加到 HttpContext 的 Items 集合中；否则终止请求。
        /// </summary>
        /// <param name="context">当前 HTTP 上下文。</param>
        /// <param name="authService">认证服务接口，用于验证访问令牌的有效性。</param>
        /// <param name="sysUserService">用户服务接口，用于根据用户 ID 获取用户实体。</param>
        /// <returns>异步任务。</returns>
        public async Task Invoke(HttpContext context, IAuthService authService, ISysUserService sysUserService)
        {
            // 主动检查当前终结点是否允许匿名访问
            var endpoint = context.GetEndpoint();
            if (endpoint?.Metadata.GetMetadata<IAllowAnonymous>() != null)
            {
                await _next(context); // 跳过验证，直接放行
                return;
            }

            // 从请求头中尝试获取 Authorization 头部值
            var accessToken = context.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();

            if (accessToken != null)
            {
                var validationResult = await ValidateAccessTokenAsync(context, accessToken, authService, sysUserService);
                if (!validationResult)
                {
                    // 如果验证失败，则不调用下一个中间件
                    return;
                }
            }

            // 调用下一个中间件
            await _next(context);
        }

        /// <summary>
        /// 验证 JWT 访问令牌的有效性。
        /// 包括解析令牌、检查签名、验证过期时间以及确认令牌未被撤销。
        /// </summary>
        /// <param name="context">当前 HTTP 上下文。</param>
        /// <param name="accessToken">要验证的访问令牌。</param>
        /// <param name="authService">认证服务接口，用于验证访问令牌是否已被撤销。</param>
        /// <param name="sysUserService">用户服务接口，用于根据用户 ID 获取用户实体。</param>
        /// <returns>如果验证成功返回 true，否则返回 false。</returns>
        private async Task<bool> ValidateAccessTokenAsync(HttpContext context, string accessToken, IAuthService authService, ISysUserService sysUserService)
        {
            try
            {
                // 创建 JwtSecurityTokenHandler 实例用于解析和验证令牌
                var tokenHandler = new JwtSecurityTokenHandler();
                if (string.IsNullOrWhiteSpace(_jwtConfig.Key))
                {
                    throw new InvalidOperationException("未配置JWT的密钥。");
                }
                var key = Encoding.ASCII.GetBytes(_jwtConfig.Key); // 获取密钥

                // 配置 TokenValidationParameters 并验证令牌
                tokenHandler.ValidateToken(accessToken, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true, // 检查签名是否有效
                    IssuerSigningKey = new SymmetricSecurityKey(key), // 使用提供的密钥
                    ValidateIssuer = false, // 不检查发行者
                    ValidateAudience = false, // 不检查受众
                    ClockSkew = TimeSpan.Zero // 精确的时间检查
                }, out SecurityToken validatedToken);

                // 将验证后的令牌转换为 JwtSecurityToken 类型
                var jwtToken = (JwtSecurityToken)validatedToken;

                // 从令牌中提取 "unique_name" 声明作为用户 ID
                var userIdClaim = jwtToken.Claims.First(x => x.Type == "unique_name").Value;

                // 尝试将用户 ID 转换为 long 类型
                if (!long.TryParse(userIdClaim, out long userId))
                {
                    throw new UnauthorizedAccessException("用户ID无效。");
                }

                // 验证访问令牌是否已被撤销
                if (!await authService.ValidateAccessTokenAsync(userId, accessToken))
                {
                    throw new SecurityTokenException("无效或已过期的访问令牌。");
                }

                // 根据用户 ID 从服务中获取用户实体
                var user = await sysUserService.GetAsync(userId);
                if (user == null)
                {
                    throw new UnauthorizedAccessException("用户不存在或已被删除。");
                }

                // 将用户信息附加到 HttpContext 的 Items 集合中，供后续中间件或控制器使用
                context.Items["User"] = user;

                // 返回 true 表示验证成功
                return true;
            }
            catch (SecurityTokenExpiredException ex)
            {
                // 处理令牌过期的情况
                throw new SecurityTokenException("无效或已过期的刷新令牌。", ex);
            }
            catch (Exception ex)
            {
                // 处理其他异常情况
                //throw new SecurityTokenException($"Invalid token: {ex.Message}", ex);
                throw new SecurityTokenException(ex.Message, ex);
            }
        }
    }
}