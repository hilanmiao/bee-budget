namespace Bedrock.Application.DataTransferObjects
{
    /// <summary>
    /// 返回用户认证信息时使用的数据传输对象。
    /// </summary>
    public class AuthInfoDto
    {
        /// <summary>
        /// 当前登录用户的基本信息。
        /// </summary>
        public SysUserDto User { get; set; } = new();
        /// <summary>
        /// 当前用户拥有的权限标识列表。
        /// </summary>
        public List<string?> Permissions { get; set; } = new();
        /// <summary>
        /// 当前用户拥有的角色标识列表。
        /// </summary>
        public List<string> Roles { get; set; } = new();
    }
}