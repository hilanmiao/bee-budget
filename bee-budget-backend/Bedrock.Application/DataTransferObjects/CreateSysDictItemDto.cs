namespace Bedrock.Application.DataTransferObjects
{
    /// <summary>
    /// 创建字典数据时使用的数据传输对象。
    /// </summary>
    public class CreateSysDictItemDto
    {
        /// <summary>
        /// 显示标签（前端展示文本）。
        /// 示例："启用"、"男"、"成功"
        /// </summary>
        public string Label { get; set; } = string.Empty;

        /// <summary>
        /// 实际值（提交或逻辑判断用的值）。
        /// 建议统一使用字符串类型，即使业务为数字。
        /// 示例："0"、"male"、"success"
        /// </summary>
        public string Value { get; set; } = string.Empty;

        /// <summary>
        /// 所属字典分类编码，关联 SysDictCategory.Code。
        /// 用于按分类查询字典项。
        /// </summary>
        public string CategoryCode { get; set; } = string.Empty;

        /// <summary>
        /// 排序
        /// </summary>
        public int? Sort { get; set; }

        /// <summary>
        /// 样式属性（其他样式扩展）
        /// </summary>
        public string? CssClass { get; set; }

        /// <summary>
        /// 表格回显样式 
        /// </summary>
        public string? ListClass { get; set; }

        /// <summary>
        /// 是否默认（Y是 N否）
        /// </summary>
        public string? IsDefault { get; set; }

        /// <summary>
        /// 状态（0正常 1停用）
        /// </summary>
        public string Status { get; set; } = string.Empty;

        /// <summary>
        /// 备注
        /// </summary>
        public string? Remark { get; set; }
    }

}