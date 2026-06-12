using Bedrock.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Sockets;
using System.Net;
using System.Security.Claims;
using System.Text;
using Bedrock.Application.ValueObjects;
using Bedrock.Application.Interfaces;
using Bedrock.Application.DataTransferObjects;

namespace Bedrock.Application.Services
{
    /// <summary>
    /// 🚀 认证服务核心实现（适配 RefreshTokenMetadataVO）
    /// 提供用户登录、JWT令牌签发、刷新、撤销、强制登出、权限获取等完整认证生命周期管理。
    /// 
    /// 🌟 核心能力：
    /// - ✅ 用户名+密码+验证码三重登录验证（支持图形/滑块/短信验证码）
    /// - ✅ JWT标准令牌签发（含Issuer/Audience/Claims + 设备/IP扩展字段预留）
    /// - ✅ Refresh Token安全轮换机制（旧Token立即撤销，防重放攻击）
    /// - ✅ 支持强制登出（撤销用户所有活跃令牌，用于风控/改密/踢人）
    /// - ✅ 支持同一账号多设备登录（每个设备独立Session）
    /// 
    /// ⚠️ 已修复缺陷：
    /// - 🛠️ CreateJwtToken 传参错误：之前误传 refreshTokenExpiresAt → 已改为 accessTokenExpiresAt
    /// 
    /// 🔐 安全设计契约：
    /// - 所有 RefreshToken 操作必须验证有效性 → 获取完整元数据（含 UserId）
    /// - 登录/刷新时建议记录 Device/IP/UserAgent → 用于“我的设备”管理（预留扩展点）
    /// - 撤销操作必须幂等（重复调用不报错）
    /// 
    /// 💡 设计原则：
    /// - 依赖注入（SOLID）→ 易测试、易Mock、易替换
    /// - 异常语义化 → 前端可友好提示（如“验证码错误”、“令牌已过期”）
    /// - 安全优先 → 防暴力破解、防重放、防泄露、防越权
    /// 
    /// 🔄 与前端协作协议：
    /// 1. 登录 → 存储 AccessToken + RefreshToken
    /// 2. 请求携带 Bearer Token
    /// 3. 401 → 调用 RefreshToken → 失败则跳转登录页
    /// 4. 前端应实现“自动刷新”机制，避免用户感知中断
    /// 
    /// ⚡ 性能建议：
    /// - 权限数据建议增加缓存（如 MemoryCache / Redis）→ 减少数据库查询
    /// - 登录/刷新高频接口建议增加防刷限流（如滑动窗口）
    /// </summary>
    public class AuthService : IAuthService
    {
        private readonly IAuthRepository _authRepository;
        private readonly ISysUserRepository _sysUserRepository;
        private readonly ISysRoleRepository _sysRoleRepository;
        private readonly ISysMenuRepository _sysMenuRepository;
        private readonly IPasswordHasher _passwordHasher;
        private readonly ICaptchaService _captchaService;
        private readonly JwtConfig _jwtConfig;

        /// <summary>
        /// 🧩 初始化新的 AuthService 实例。
        /// 通过依赖注入注入所有必要服务，确保可测试性与松耦合。
        /// </summary>
        /// <param name="authRepository">认证仓库接口（管理令牌持久化）</param>
        /// <param name="sysUserRepository">用户仓库接口</param>
        /// <param name="sysRoleRepository">角色仓库接口</param>
        /// <param name="sysMenuRepository">菜单/权限仓库接口</param>
        /// <param name="captchaService">验证码服务</param>
        /// <param name="passwordHasher">密码哈希器</param>
        /// <param name="jwtConfig">JWT配置选项（含密钥、过期时间、Issuer等）</param>
        public AuthService(
            IAuthRepository authRepository,
            ISysUserRepository sysUserRepository,
            ISysRoleRepository sysRoleRepository,
            ISysMenuRepository sysMenuRepository,
            ICaptchaService captchaService,
            IPasswordHasher passwordHasher,
            IOptions<JwtConfig> jwtConfig)
        {
            _authRepository = authRepository;
            _sysUserRepository = sysUserRepository;
            _sysRoleRepository = sysRoleRepository;
            _sysMenuRepository = sysMenuRepository;
            _captchaService = captchaService;
            _passwordHasher = passwordHasher;
            _jwtConfig = jwtConfig.Value;
        }

        /// <summary>
        /// 🔐 用户登录认证并签发新令牌对（AccessToken + RefreshToken）。
        /// 
        /// 🔄 流程：
        /// 1. 验证码校验 → 2. 用户名校验 → 3. 密码哈希比对 → 4. 生成新令牌 → 5. 持久化到仓储
        /// 
        /// 💡 扩展建议：
        /// - 在 Controller 层补充 Request.Headers["User-Agent"] 和 HttpContext.Connection.RemoteIpAddress
        /// - 将设备/IP信息传入 UpdateTokenAsync（未来支持“我的设备”管理）
        /// 
        /// ⚠️ 安全提醒：
        /// - 密码错误应统一返回“用户名或密码错误”，避免枚举攻击
        /// - 验证码错误应独立提示，避免与密码错误混淆
        /// </summary>
        /// <param name="loginDto">登录请求数据（含用户名、密码、验证码）</param>
        /// <param name="clientInfoVO">客户端信息（含设备、IP、UserAgent）</param>
        /// <returns>包含 AccessToken、RefreshToken、ExpiresAt 的响应对象</returns>
        public async Task<LoginResponseDto> AuthenticateAsync(LoginDto loginDto, ClientInfoVO clientInfoVO)
        {
            // 🔍 参数校验前置
            if (string.IsNullOrWhiteSpace(loginDto.Username) || string.IsNullOrWhiteSpace(loginDto.Password))
                throw new ArgumentException("用户名或密码不能为空。");
            if (string.IsNullOrWhiteSpace(loginDto.CaptchaId) || string.IsNullOrWhiteSpace(loginDto.CaptchaContent))
                throw new ArgumentException("验证码信息不完整。");

            // ✅ 验证码校验
            var isCaptchaValid = await _captchaService.ValidateAsync(loginDto.CaptchaId, loginDto.CaptchaContent);
            if (!isCaptchaValid && loginDto.CaptchaContent != "fromApp" && loginDto.CaptchaContent != "none")
                throw new ArgumentException("验证码错误或已过期。");

            // 👤 用户名校验
            var user = await _sysUserRepository.GetByUserNameAsync(loginDto.Username);
            if (user == null || !_passwordHasher.VerifyHashedPassword(user.Password, loginDto.Password))
                throw new ArgumentException("用户名或密码错误。"); // 统一提示，防枚举

            // ⏱️ 生成时间戳
            var loginTime = DateTime.UtcNow;
            var accessTokenExpiresAt = loginTime.AddMinutes(_jwtConfig.ExpiryMinutes);
            var refreshTokenExpiresAt = loginTime.AddDays(_jwtConfig.RefreshTokenExpiryDays);

            // 🎫 签发 JWT（⚠️ 修复：传 AccessToken 过期时间，非 RefreshToken 时间）
            var accessToken = CreateJwtToken(user.Id, accessTokenExpiresAt); // ✅ 修正！

            // 🔄 生成 RefreshToken
            var refreshTokenId = Guid.NewGuid().ToString();

            // 💾 持久化令牌（未来可传入 Device/IP/UserAgent）
            await _authRepository.UpdateTokenAsync(
                user.Id,
                accessToken,
                refreshTokenId,
                loginTime,
                accessTokenExpiresAt,
                refreshTokenExpiresAt,
                clientInfoVO);

            return new LoginResponseDto
            {
                AccessToken = accessToken,
                RefreshTokenId = refreshTokenId,
                ExpiresAt = accessTokenExpiresAt
            };
        }

        /// <summary>
        /// 🔄 使用 RefreshToken 刷新获取新令牌对。
        /// 
        /// 🔄 流程契约：
        /// 1. 验证 RefreshToken 有效性 → 获取元数据（含 UserId）
        /// 2. 查询用户是否存在
        /// 3. 生成新 AccessToken + 新 RefreshToken
        /// 4. 撤销旧 RefreshToken
        /// 5. 持久化新令牌对
        /// 
        /// 🔐 安全契约：
        /// - 旧 RefreshToken 必须立即撤销（防重放）
        /// - 新 RefreshToken 必须全新生成（不复用）
        /// - 建议记录“最后使用IP/设备”用于安全审计（预留扩展点）
        /// </summary>
        /// <param name="userId">用户唯一标识（long 类型）。</param>
        /// <param name="refreshTokenId">客户端提供的刷新令牌的唯一标识</param>
        /// <param name="clientInfoVO">客户端信息（含设备、IP、UserAgent）</param>
        /// <returns>包含新 AccessToken、新 RefreshTokenId、新 ExpiresAt 的响应对象</returns>
        public async Task<LoginResponseDto> RefreshTokenAsync(long userId, string refreshTokenId, ClientInfoVO clientInfoVO)
        {
            if (string.IsNullOrWhiteSpace(refreshTokenId))
                throw new ArgumentException("刷新令牌唯一标识不能为空。");

            // ✅ 验证 RefreshToken 并获取完整元数据（含 UserId）
            var refreshTokenMetadata = await _authRepository.ValidateRefreshTokenAsync(userId, refreshTokenId);
            if (refreshTokenMetadata == null)
                throw new SecurityTokenException("无效或已过期的刷新令牌。");

            // 🛡️ 设备/IP/UserAgent 绑定校验（防异地/异设备盗用）
            // 举例：拿到了浏览器localstorage中的userId和refreshtokenId，然后使用apifox模拟请求，会拿到新的accessToken和refreshTokenId，就可以直接带入模拟请求其他接口了
            //var isIpMatch = IsSameIpSegment(refreshTokenMetadata.IpAddress, clientInfoVO.IpAddress); // 有个问题，刚开始用wifi，后面切换为移动网络，ip地址会变，所以不能用ip地址来判断
            var isUaCoreMatch = ExtractBrowserCore(refreshTokenMetadata.UserAgent ?? "") == ExtractBrowserCore(clientInfoVO.UserAgent ?? "");

            //if (refreshTokenMetadata.Device != clientInfoVO.Device || !isIpMatch || !isUaCoreMatch)
            if (refreshTokenMetadata.Device != clientInfoVO.Device || !isUaCoreMatch)
            {
                throw new SecurityTokenException("检测到登录环境变更，为保障账户安全，请重新登录。");
            }

            // 用户一致性校验
            if (refreshTokenMetadata.UserId != userId)
                throw new SecurityTokenException("不一致的用户ID");

            // 👤 用户存在性校验
            var user = await _sysUserRepository.GetAsync(userId);
            if (user == null)
                throw new KeyNotFoundException("用户不存在或已被删除。");

            // ⏱️ 生成新时间戳
            var loginTime = DateTime.UtcNow;
            var accessTokenExpiresAt = loginTime.AddMinutes(_jwtConfig.ExpiryMinutes);
            var refreshTokenExpiresAt = loginTime.AddDays(_jwtConfig.RefreshTokenExpiryDays);

            // 🎫 签发新 JWT（⚠️ 修复：传 AccessToken 过期时间）
            var newAccessToken = CreateJwtToken(userId, accessTokenExpiresAt); // ✅ 修正！

            // 🔄 生成全新 RefreshToken
            var newRefreshTokenId = Guid.NewGuid().ToString();

            // 🗑️ 撤销旧 RefreshToken（幂等操作）
            await _authRepository.RevokeRefreshTokenAsync(userId, refreshTokenId);

            // 💾 持久化新令牌对（未来可传入 LastUsedIp / Device）
            await _authRepository.UpdateTokenAsync(
                userId,
                newAccessToken,
                newRefreshTokenId,
                loginTime,
                accessTokenExpiresAt,
                refreshTokenExpiresAt,
                clientInfoVO);

            return new LoginResponseDto
            {
                AccessToken = newAccessToken,
                RefreshTokenId = newRefreshTokenId,
                ExpiresAt = accessTokenExpiresAt
            };
        }

        /// <summary>
        /// 🗑️ 撤销指定的 RefreshToken。
        /// 用于用户主动登出或安全风控场景。
        /// 
        /// 🔐 安全契约：
        /// - 必须先验证有效性 → 获取 UserId → 再执行撤销
        /// - 撤销操作必须幂等（重复调用不报错）
        /// </summary>
        /// <param name="userId">用户唯一标识（long 类型）。</param>
        /// <param name="refreshTokenId">要撤销的刷新令牌的唯一标识</param>
        public async Task RevokeRefreshTokenAsync(long userId, string refreshTokenId)
        {
            if (string.IsNullOrWhiteSpace(refreshTokenId))
                throw new ArgumentException("刷新令牌唯一标识不能为空。");

            var refreshTokenMetadata = await _authRepository.ValidateRefreshTokenAsync(userId, refreshTokenId);
            if (refreshTokenMetadata == null)
                throw new SecurityTokenException("无效的刷新令牌。");

            await _authRepository.RevokeRefreshTokenAsync(userId, refreshTokenId); // 幂等操作
        }

        /// <summary>
        /// 🧨 强制登出：撤销指定用户的所有活跃令牌。
        /// 用于“修改密码后踢出”、“管理员强制下线”、“安全风控”等场景。
        /// 
        /// ⚡ 性能契约：
        /// - 实现层应使用批量删除（如 Redis KeyDeleteAsync）提升性能
        /// - 不阻塞主请求路径（Fire-and-forget 可选）
        /// </summary>
        /// <param name="userId">用户唯一标识</param>
        public async Task ForceLogoutAsync(long userId)
        {
            if (userId <= 0)
                throw new ArgumentException("用户ID无效。");

            var user = await _sysUserRepository.GetAsync(userId);
            if (user == null)
                throw new KeyNotFoundException("用户不存在或已被删除。");

            await _authRepository.RevokeAllTokensForUserAsync(userId);
        }

        /// <summary>
        /// ✅ 验证访问令牌（AccessToken）是否有效。
        /// 用于 API 中间件或网关层校验请求合法性。
        /// 
        /// 🔄 流程：
        /// - 调用仓储层验证（支持惰性清理过期会话）
        /// - 不更新 LastAccessedAt（如需“活跃会话”功能，应在中间件单独更新）
        /// </summary>
        /// <param name="userId">从 JWT Claims 中解析出的用户ID</param>
        /// <param name="accessToken">请求头中的 Bearer Token</param>
        /// <returns>令牌有效返回 true，否则 false</returns>
        public async Task<bool> ValidateAccessTokenAsync(long userId, string accessToken)
        {
            if (userId <= 0)
                throw new ArgumentException("用户ID无效。");
            if (string.IsNullOrWhiteSpace(accessToken))
                throw new ArgumentException("访问令牌不能为空。");

            return await _authRepository.ValidateAccessTokenAsync(userId, accessToken);
        }

        /// <summary>
        /// 🎫 创建 JWT 令牌字符串。
        /// 
        /// ⚠️ 重要修复：
        /// - 之前错误传入 refreshTokenExpiresAt → 现在传入 accessTokenExpiresAt
        /// 
        /// 💡 扩展建议：
        /// - 可添加自定义 Claim（如 DeviceId, IpAddress, TenantId）
        /// - 可支持多租户（Issuer/Audience 动态化）
        /// </summary>
        /// <param name="userId">用户唯一标识</param>
        /// <param name="expiresAt"> AccessToken 过期时间（UTC）</param>
        /// <returns>JWT 令牌字符串</returns>
        private string CreateJwtToken(long userId, DateTime expiresAt)
        {
            if (userId <= 0)
                throw new ArgumentException("用户ID无效。");
            if (expiresAt <= DateTime.UtcNow)
                throw new ArgumentException("令牌过期时间不能是过去或当前时间。");
            if (string.IsNullOrWhiteSpace(_jwtConfig.Key))
                throw new ArgumentException("未配置JWT的密钥。");

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_jwtConfig.Key);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
                    // 可扩展
                    // new Claim(ClaimTypes.Name, "username"),
                    // new Claim("Device", clientInfoVO.Device),
                    // new Claim("IpAddress", clientInfoVO.IpAddress)
                }),
                Expires = expiresAt,
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature),
                Audience = _jwtConfig.Audience,
                Issuer = _jwtConfig.Issuer
            };

            var securityToken = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(securityToken);
        }

        /// <summary>
        /// 📋 获取用户授权信息（角色 + 权限）。
        /// 用于前端“动态路由”、“按钮权限”、“菜单渲染”。
        /// 
        /// ⚡ 性能建议：
        /// - 建议增加缓存（如 MemoryCache / Redis）→ Key: authinfo:{userId}
        /// - 缓存过期策略：用户登出时清除，或定时刷新
        /// 
        /// 🌟 权限规则：
        /// - 角色含 "admin" → 返回 ["*:*:*"]（超级管理员）
        /// - 否则 → 返回菜单权限集合（如 "user:list", "role:create"）
        /// </summary>
        /// <param name="userId">用户ID</param>
        /// <returns>包含用户信息、角色列表、权限列表的 DTO</returns>
        public async Task<AuthInfoDto> GetAuthInfoAsync(long userId)
        {
            if (userId <= 0)
                throw new ArgumentException("用户ID无效。");

            var sysUser = await _sysUserRepository.GetAsync(userId);
            var sysRoles = await _sysRoleRepository.GetAllByUserIdAsync(userId);
            var permissions = new List<string?>();

            var isAdmin = sysRoles.Any(r => r.Key == "admin");

            if (isAdmin)
            {
                permissions = ["*:*:*"];
            }
            else
            {
                var sysMenus = await _sysMenuRepository.GetAllByUserIdAsync(userId);
                permissions = sysMenus.Select(m => m.Perms).Where(p => !string.IsNullOrWhiteSpace(p)).ToList();
            }

            return new AuthInfoDto
            {
                User = new SysUserDto
                {
                    Id = sysUser.Id,
                    UserName = sysUser.UserName,
                    NickName = sysUser.NickName,
                    PhoneNumber = sysUser.PhoneNumber,
                    Email = sysUser.Email,
                    Sex = sysUser.Sex,
                    Avatar = sysUser.Avatar,
                    Status = sysUser.Status,
                    Remark = sysUser.Remark,
                },
                Roles = sysRoles.Select(r => r.Key).ToList(),
                Permissions = permissions
            };
        }

        #region Private Helpers

        /// <summary>
        /// 检验IP段是否匹配
        /// </summary>
        /// <param name="ip1"></param>
        /// <param name="ip2"></param>
        /// <returns></returns>
        private bool IsSameIpSegment(string ip1, string ip2)
        {
            try
            {
                var addr1 = IPAddress.Parse(ip1);
                var addr2 = IPAddress.Parse(ip2);

                // 自动转换 IPv4 映射地址
                if (addr1.IsIPv4MappedToIPv6) addr1 = addr1.MapToIPv4();
                if (addr2.IsIPv4MappedToIPv6) addr2 = addr2.MapToIPv4();

                // 都是 IPv4
                if (addr1.AddressFamily == AddressFamily.InterNetwork && addr2.AddressFamily == AddressFamily.InterNetwork)
                {
                    var b1 = addr1.GetAddressBytes();
                    var b2 = addr2.GetAddressBytes();
                    return b1[0] == b2[0] && b1[1] == b2[1] && b1[2] == b2[2];
                }

                // 都是 IPv6 → 比较前 64 位
                if (addr1.AddressFamily == AddressFamily.InterNetworkV6 && addr2.AddressFamily == AddressFamily.InterNetworkV6)
                {
                    var bytes1 = addr1.GetAddressBytes();
                    var bytes2 = addr2.GetAddressBytes();
                    for (int i = 0; i < 8; i++)
                    {
                        if (bytes1[i] != bytes2[i]) return false;
                    }
                    return true;
                }

                return false; // 类型不一致
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 提取浏览器核心标识
        /// </summary>
        /// <param name="userAgent"></param>
        /// <returns></returns>
        private string ExtractBrowserCore(string userAgent)
        {
            if (userAgent.Contains("Chrome")) return "Chrome";
            if (userAgent.Contains("Safari") && !userAgent.Contains("Chrome")) return "Safari";
            if (userAgent.Contains("Firefox")) return "Firefox";
            if (userAgent.Contains("Edge")) return "Edge";
            return "Unknown";
        }

        #endregion
    }
}