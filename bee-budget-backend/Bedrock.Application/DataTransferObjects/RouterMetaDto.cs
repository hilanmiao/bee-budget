using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bedrock.Application.DataTransferObjects
{
    /// <summary>
    /// 路由元信息数据传输对象，描述路由在侧边栏和面包屑中的展示方式。
    /// </summary>
    public class RouterMetaDto
    {
        /// <summary>
        /// 路由在侧边栏和面包屑中展示的名称。
        /// </summary>
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// 路由图标，对应路径 src/assets/icons/svg。
        /// </summary>
        public string? Icon { get; set; }

        /// <summary>
        /// 是否不使用 &lt;keep-alive&gt; 缓存；为 true 时该路由不会被缓存。
        /// </summary>
        public bool NoCache { get; set; }

        /// <summary>
        /// 内链地址，以 http(s):// 开头。
        /// </summary>
        public string? Link { get; set; }

        /// <summary>
        /// 使用标题和图标创建路由元信息。
        /// </summary>
        /// <param name="title">路由展示名称。</param>
        /// <param name="icon">图标路径。</param>
        public RouterMetaDto(string title, string? icon)
        {
            this.Title = title;
            this.Icon = icon;
        }

        /// <summary>
        /// 使用标题、图标和缓存设置创建路由元信息。
        /// </summary>
        /// <param name="title">路由展示名称。</param>
        /// <param name="icon">图标路径。</param>
        /// <param name="noCache">是否禁用 keep-alive 缓存。</param>
        public RouterMetaDto(string title, string? icon, bool noCache)
        {
            this.Title = title;
            this.Icon = icon;
            this.NoCache = noCache;
        }

        /// <summary>
        /// 使用标题、图标和内链地址创建路由元信息。
        /// </summary>
        /// <param name="title">路由展示名称。</param>
        /// <param name="icon">图标路径。</param>
        /// <param name="link">内链地址，仅当以 http 开头时生效。</param>
        public RouterMetaDto(string title, string? icon, string? link)
        {
            this.Title = title;
            this.Icon = icon;
            this.Link = link;
        }

        /// <summary>
        /// 使用标题、图标、缓存设置和内链地址创建路由元信息。
        /// </summary>
        /// <param name="title">路由展示名称。</param>
        /// <param name="icon">图标路径。</param>
        /// <param name="noCache">是否禁用 keep-alive 缓存。</param>
        /// <param name="link">内链地址，仅当以 http 开头时生效。</param>
        public RouterMetaDto(string title, string? icon, bool noCache, string? link)
        {
            this.Title = title;
            this.Icon = icon;
            this.NoCache = noCache;
            if (!string.IsNullOrWhiteSpace(link) && link.StartsWith("http", StringComparison.OrdinalIgnoreCase))
            {
                this.Link = link;
            }
        }
    }
}
