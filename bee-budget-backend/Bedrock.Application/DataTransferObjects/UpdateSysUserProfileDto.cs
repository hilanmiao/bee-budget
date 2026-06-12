namespace Bedrock.Application.DataTransferObjects
{
    /// <summary>
    /// 更新个人信息DTO
    /// </summary>
    public class UpdateSysUserProfileDto
    {
        public long Id { get; set; }
        public string? NickName { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Email { get; set; }
        public string? Sex { get; set; }
        public string? Avatar { get; set; }
    }
}