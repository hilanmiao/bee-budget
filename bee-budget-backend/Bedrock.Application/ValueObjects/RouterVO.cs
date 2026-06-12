using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bedrock.Application.ValueObjects
{
    public class RouterVO
    {
        /**
        * 路由名字
        */
        public string Name { get; set; } = string.Empty;

        /**
         * 路由地址
         */
        public string? Path { get; set; }

        /**
         * 是否隐藏路由，当设置 true 的时候该路由不会再侧边栏出现
         */
        public bool Hidden { get; set; }

        /**
         * 重定向地址，当设置 noRedirect 的时候该路由在面包屑导航中不可被点击
         */
        public string Redirect { get; set; } = string.Empty;

        /**
         * 组件地址
         */
        public string? Component { get; set; }
        /**
         * 路由参数：如 {"id": 1, "name": "ry"}
         */
        public string? Query { get; set; }

        /**
         * 当你一个路由下面的 children 声明的路由大于1个时，自动会变成嵌套的模式--如组件页面
         */
        public bool AlwaysShow { get; set; }

        /**
         * 其他元素
         */
        public MetaVO? Meta { get; set; }

        /**
         * 子路由
         */
        public List<RouterVO> Children { get; set; } = new();
    }
}
