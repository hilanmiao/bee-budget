namespace Bedrock.Application.DataTransferObjects
{
    /// <summary>
    /// 状态变更数据传输对象
    /// </summary>
    public class ChangeStatusDto
    {
        /// <summary>
        /// 新状态（0：，1：，2：...）
        /// </summary>
        public string Status { get; set; } = string.Empty;
    }
} 