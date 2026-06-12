using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bedrock.Application.ValueObjects
{
    public class MetaVO
    {
        /**
          * 设置该路由在侧边栏和面包屑中展示的名字
          */
        public string Title { get; set; } = string.Empty;

        /**
         * 设置该路由的图标，对应路径src/assets/icons/svg
         */
        public string? Icon { get; set; }

        /**
         * 设置为true，则不会被 <keep-alive>缓存
         */
        public bool NoCache { get; set; }

        /**
         * 内链地址（http(s)://开头）
         */
        public string? Link { get; set; }

        public MetaVO(string title, string? icon)
        {
            this.Title = title;
            this.Icon = icon;
        }

        public MetaVO(string title, string? icon, bool noCache)
        {
            this.Title = title;
            this.Icon = icon;
            this.NoCache = noCache;
        }

        public MetaVO(string title, string? icon, string? link)
        {
            this.Title = title;
            this.Icon = icon;
            this.Link = link;
        }

        public MetaVO(string title, string? icon, bool noCache, string? link)
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
