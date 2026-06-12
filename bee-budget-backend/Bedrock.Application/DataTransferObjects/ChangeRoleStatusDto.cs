using System.ComponentModel.DataAnnotations;

namespace Bedrock.Application.DataTransferObjects
{
    /// <summary>
    /// 角色状态变更数据传输对象
    /// </summary>
    public class ChangeRoleStatusDto
    {
        /// <summary>
        /// 新状态（0：停用，1：正常）
        /// </summary>
        public string Status { get; set; } = string.Empty;
    }
} 