namespace Bedrock.Application.DataTransferObjects
{
    public class UpdateAppDto
    {
        /// <summary>
        /// 唯一标识符（主键）。
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// 应用的描述信息。
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// 应用图标的URL地址。
        /// </summary>
        public string? Icon { get; set; }

        /// <summary>
        /// 应用截图的URL地址。
        /// </summary>
        public string? Screenshot { get; set; }

        /// <summary>
        /// 应用对应的H5页面的URL地址。
        /// </summary>
        public string? H5Url { get; set; }

        /// <summary>
        /// 标识该应用是否启用。true 表示启用，false 表示禁用。
        /// </summary>
        public bool IsEnabled { get; set; }

    }

}