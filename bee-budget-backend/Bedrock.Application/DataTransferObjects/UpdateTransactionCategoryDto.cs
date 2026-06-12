namespace Bedrock.Application.DataTransferObjects
{
    public class UpdateTransactionCategoryDto
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
        /// 是否为公共分类。
        /// </summary>
        public bool IsPublic { get; set; }

        /// <summary>
        /// 图标（可选）。
        /// </summary>
        public string? Icon { get; set; }

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