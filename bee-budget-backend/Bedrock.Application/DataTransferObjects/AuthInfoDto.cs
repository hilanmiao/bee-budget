namespace Bedrock.Application.DataTransferObjects
{
    public class AuthInfoDto
    {
        public SysUserDto User { get; set; } = new();
        public List<string?> Permissions { get; set; } = new();
        public List<string> Roles { get; set; } = new();
    }
}