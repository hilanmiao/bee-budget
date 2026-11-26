namespace Bedrock.Application.DataTransferObjects
{
    /// <summary>
    /// 创建账本信息时使用的数据传输对象。
    /// </summary>
    public class CreateLedgerDto
    {
        /// <summary>
        /// 账本名称。
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// 用户ID。
        /// </summary>
        public long UserId { get; set; }

        /// <summary>
        /// 备注。
        /// </summary>
        public string? Remark { get; set; }
    }
}