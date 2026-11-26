using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bedrock.Application.DataTransferObjects
{
    /// <summary>
    /// 前端路由数据传输对象，描述一个可展示的路由节点。
    /// </summary>
    public class RouterDto
    {
        /// <summary>
        /// 路由名称。
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// 路由地址。
        /// </summary>
        public string? Path { get; set; }

        /// <summary>
        /// 是否隐藏路由；为 true 时该路由不会出现在侧边栏。
        /// </summary>
        public bool Hidden { get; set; }

        /// <summary>
        /// 重定向地址；设置 noRedirect 时该路由在面包屑导航中不可被点击。
        /// </summary>
        public string Redirect { get; set; } = string.Empty;

        /// <summary>
        /// 路由对应的组件地址。
        /// </summary>
        public string? Component { get; set; }
        /// <summary>
        /// 路由参数，如 {"id": 1, "name": "ry"}。
        /// </summary>
        public string? Query { get; set; }

        /// <summary>
        /// 是否始终显示；当一个路由下的 children 超过 1 个时会自动变成嵌套模式。
        /// </summary>
        public bool AlwaysShow { get; set; }

        /// <summary>
        /// 路由的元信息。
        /// </summary>
        public RouterMetaDto? Meta { get; set; }

        /// <summary>
        /// 子路由列表。
        /// </summary>
        public List<RouterDto> Children { get; set; } = new();
    }
}
