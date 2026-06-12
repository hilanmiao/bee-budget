namespace Bedrock.Core.Entities
{
    /// <summary>
    /// 系统字典项实体，表示字典分类下的具体实际值对选项。
    /// 用于前端下拉框、状态标签、表单默认值等场景。
    /// 索引：IX_SysDictItem_CategoryCode
    /// </summary>
    public class SysDictItem
    {
        /// <summary>
        /// 唯一标识符（主键）。
        /// </summary>
        [SqlSugar.SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
        public long Id { get; set; }

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
        public string? Status { get; set; }

        /// <summary>
        /// 排序
        /// </summary>
        public int? Sort { get; set; }

        /// <summary>
        /// 备注
        /// </summary>
        public string? Remark { get; set; }

        /// <summary>
        /// 创建时间（UTC）。
        /// </summary>
        public DateTime? CreatedAt { get; set; }

        /// <summary>
        /// 创建者ID，指向创建该记录的用户。
        /// </summary>
        public long? CreatedById { get; set; }

        /// <summary>
        /// 更新时间（UTC）。
        /// </summary>
        public DateTime? UpdatedAt { get; set; }

        /// <summary>
        /// 更新者ID，指向最后一次更新该记录的用户。
        /// </summary>
        public long? UpdatedById { get; set; }

        /// <summary>
        /// 删除时间（UTC）。
        /// </summary>
        public DateTime? DeletedAt { get; set; }

        /// <summary>
        /// 删除者ID，指向删除该记录的用户。
        /// </summary>
        public long? DeletedById { get; set; }
    }
}