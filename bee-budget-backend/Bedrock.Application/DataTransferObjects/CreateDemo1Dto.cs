namespace Bedrock.Application.DataTransferObjects
{
    /// <summary>
    /// 创建样例1信息时使用的数据传输对象。
    /// </summary>
    public class CreateDemo1Dto
    {
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