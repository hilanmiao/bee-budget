namespace Bedrock.Application.DataTransferObjects
{
    /// <summary>
    /// 返回系统角色简要信息时使用的数据传输对象。
    /// </summary>
    public class SysRoleMiniDto
    {
        /// <summary>
        /// 唯一标识符（主键）。
        /// </summary>
        public long Id { get; set; }
        /// <summary>
        /// 名称。
        /// </summary>
        public string Name { get; set; } = string.Empty;
        /// <summary>
        /// 角色标识（权限关键字）。
        /// </summary>
        public string? Key { get; set; }
    }
}