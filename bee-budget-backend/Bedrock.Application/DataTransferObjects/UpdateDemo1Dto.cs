namespace Bedrock.Application.DataTransferObjects
{
    /// <summary>
    /// 更新样例1信息时使用的数据传输对象。
    /// </summary>
    public class UpdateDemo1Dto
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
        /// 显示顺序。
        /// </summary>
        public int? Sort { get; set; }

        /// <summary>
        /// 状态。0 表示正常, 1 表示停用。
        /// </summary>
        public string? Status { get; set; }

        /// <summary>
        /// 备注。
        /// </summary>
        public string? Remark { get; set; }

    }
}