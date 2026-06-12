namespace Bedrock.Application.DataTransferObjects
{
    public class SysRoleMiniDto
    {
        public long Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Key { get; set; }
    }
}