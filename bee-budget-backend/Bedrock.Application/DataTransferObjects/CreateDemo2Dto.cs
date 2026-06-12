namespace Bedrock.Application.DataTransferObjects
{
    /// <summary>
    /// 创建样例2信息时使用的数据传输对象。
    /// </summary>
    public class CreateDemo2Dto
    {
        /// <summary>
        /// 关联Demo1的Id。
        /// </summary>
        public long Demo1Id { get; set; }

        /// <summary>
        /// 名称。
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// 别名。
        /// </summary>
        public string? AliasName { get; set; }

        /// <summary>
        /// 编码。
        /// </summary>
        public int? Code { get; set; }

        /// <summary>
        /// 是否可见。true 表示可见，false 表示隐藏。
        /// </summary>
        public bool IsVisible { get; set; }

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