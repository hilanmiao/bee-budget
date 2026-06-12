using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bedrock.Configuration
{
    /// <summary>
    /// CORS 配置选项，用于从 appsettings.json 绑定
    /// </summary>
    public class CorsConfig
    {
        /// <summary>
        /// 是否允许所有来源（等效于 AllowAnyOrigin）
        /// 建议仅在开发环境启用，生产环境应关闭以提高安全性
        /// </summary>
        public bool AllowAllOrigins { get; set; } = false;

        /// <summary>
        /// 允许的来源列表（仅在 AllowAllOrigins 为 false 时生效）
        /// 必须是完整的协议+域名+端口，例如 "https://localhost:5001"
        /// </summary>
        public string[] AllowedOrigins { get; set; } = Array.Empty<string>();
    }
}
