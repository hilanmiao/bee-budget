namespace Bedrock.Application.DataTransferObjects
{
    /// <summary>
    /// 更新产品时使用的数据传输对象。
    /// </summary>
    public class UpdateProductionDto
    {
        /// <summary>
        /// 唯一标识符（主键）。
        /// </summary>
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
        public string Status { get; set; } = string.Empty;

        /// <summary>
        /// 备注。
        /// </summary>
        public string? Remark { get; set; }
    }

}