namespace Bedrock.Application.DataTransferObjects
{
    public class CreateAppDto
    {

        /// <summary>
        /// 应用的唯一标识符，通常用于区分不同的App。
        /// </summary>
        public string AppId { get; set; } = string.Empty;   

        /// <summary>
        /// 应用的名称。
        /// </summary>
        public string Name { get; set; } = string.Empty;

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