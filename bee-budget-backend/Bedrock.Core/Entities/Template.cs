using SqlSugar;

namespace Bedrock.Core.Entities
{
    /// <summary>
    /// 模板实体类
    /// </summary>
    public class Template
    {
        /// <summary>
        /// 主键ID
        /// </summary>
        [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
        public long Id { get; set; }

        /// <summary>
        /// 模板编码
        /// </summary>
        [SugarColumn(Length = 50)]
        public string Code { get; set; } = string.Empty;

        /// <summary>
        /// 模板名称
        /// </summary>
        [SugarColumn(Length = 100)]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// 模板描述
        /// </summary>
        [SugarColumn(Length = 500)]
        public string? Description { get; set; }

        /// <summary>
        /// 模板状态
        /// </summary>
        [SugarColumn(Length = 1)]
        public string Status { get; set; } = "1";

        /// <summary>
        /// 排序号
        /// </summary>
        public int Sort { get; set; } = 0;

        /// <summary>
        /// 模板类型
        /// </summary>
        [SugarColumn(Length = 50)]
        public string? Type { get; set; }

        /// <summary>
        /// 模板标签
        /// </summary>
        [SugarColumn(Length = 200)]
        public string? Tags { get; set; }

        /// <summary>
        /// 备注
        /// </summary>
        [SugarColumn(Length = 1000)]
        public string? Remark { get; set; }

        /// <summary>
        /// 创建者ID
        /// </summary>
        public Guid? CreatedById { get; set; }

        /// <summary>
        /// 创建时间（UTC）
        /// </summary>
        public DateTime? CreatedAt { get; set; }

        /// <summary>
        /// 更新者ID
        /// </summary>
        public Guid? UpdatedById { get; set; }

        /// <summary>
        /// 更新时间（UTC）
        /// </summary>
        public DateTime? UpdatedAt { get; set; }

        /// <summary>
        /// 删除者ID
        /// </summary>
        public Guid? DeletedById { get; set; }

        /// <summary>
        /// 删除时间（UTC）
        /// </summary>
        public DateTime? DeletedAt { get; set; }
    }
} 