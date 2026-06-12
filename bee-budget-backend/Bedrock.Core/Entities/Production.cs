using SqlSugar;

namespace Bedrock.Core.Entities
{
    /// <summary>
    /// 产品实体类，用于表示产品的信息。
    /// </summary>
    public class Production
    {
        /// <summary>
        /// 唯一标识符（主键）。
        /// </summary>
        [SugarColumn(IsPrimaryKey = true, IsIdentity = false)]
        public long Id { get; set; }

        /// <summary>
        /// 产品名称。
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// 产品系列。
        /// </summary>
        public string Series { get; set; } = string.Empty;

        /// <summary>
        /// 产品型号。
        /// </summary>
        public string Model { get; set; } = string.Empty;

        /// <summary>
        /// 产品特性。
        /// </summary>
        public string Characteristic { get; set; } = string.Empty;

        /// <summary>
        /// 封面图片。
        /// </summary>
        public string Cover { get; set; } = string.Empty;

        /// <summary>
        /// 产品三维模型（可选）。
        /// </summary>
        public string? ModelThreeDimensional { get; set; }

        /// <summary>
        /// 产品相册（可选）。
        /// </summary>
        public string? Album { get; set; }

        /// <summary>
        /// 产品相关文件（可选）。
        /// </summary>
        public string? Files { get; set; }

        /// <summary>
        /// 产品描述。
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// 性能参数。
        /// </summary>
        public string? Params { get; set; }

        /// <summary>
        /// 外形尺寸。
        /// </summary>
        public string? Size { get; set; }

        /// <summary>
        /// 安装事项。
        /// </summary>
        public string? Install { get; set; }

        /// <summary>
        /// 产品选择信息（可选）。
        /// </summary>
        public string? Choose { get; set; }

        /// <summary>
        /// 产品价格。
        /// </summary>
        public decimal Price { get; set; }


        /// <summary>
        /// 状态（0正常 1停用）
        /// </summary>
        public string? Status { get; set; }

        /// <summary>
        /// 备注。
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