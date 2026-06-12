using Bedrock.Application.DataTransferObjects;
using Bedrock.Application.Interfaces;
using Bedrock.Application.ValueObjects;
using Bedrock.Core.Entities;
using Humanizer;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using SqlSugar;
using System.Data;

namespace Bedrock.Application.Services
{
    /// <summary>
    /// 系统菜单应用服务实现。
    /// <para>
    /// 负责菜单的创建、更新、删除、查询等业务逻辑，支持软删除语义。
    /// 所有查询默认过滤已删除记录。
    /// 业务规则：Name 保证全局唯一。
    /// </para>
    /// </summary>
    public class SysMenuService : ISysMenuService
    {
        private readonly ILogger<SysMenuService> _logger;
        private readonly ISqlSugarClient _db;
        private readonly ISysMenuRepository _sysMenuRepository;
        private readonly ISysRoleMenuRepository _sysRoleMenuRepository;
        private readonly ISysUserRepository _sysUserRepository;
        private readonly ISysRoleRepository _sysRoleRepository;

        /** 是否菜单外链（是） */
        public const string YES_FRAME = "1";

        /** 是否菜单外链（否） */
        public const string NO_FRAME = "0";

        /** 是否缓存（是） */
        public const string YES_CACHE = "1";

        /** 菜单类型（目录） */
        public const string TYPE_DIR = "M";

        /** 菜单类型（菜单） */
        public const string TYPE_MENU = "C";

        /** 菜单类型（按钮） */
        public const string TYPE_BUTTON = "F";

        /** Layout组件标识 */
        public const string LAYOUT = "Layout";

        /** ParentView组件标识 */
        public const string PARENT_VIEW = "ParentView";

        /** InnerLink组件标识 */
        public const string INNER_LINK = "InnerLink";

        /** 校验是否唯一的返回标识 */
        public const bool UNIQUE = true;
        public const bool NOT_UNIQUE = false;

        /// <summary>
        /// 初始化 <see cref="SysMenuService"/> 类的新实例。
        /// </summary>
        /// <param name="db">SqlSugar 客户端，用于事务控制。</param>
        /// <param name="sysMenuRepository">菜单仓储。</param>
        /// <param name="sysRoleMenuRepository">角色菜单仓储。</param>
        public SysMenuService(
            ILogger<SysMenuService> logger,
            ISqlSugarClient db,
            ISysMenuRepository sysMenuRepository,
            ISysRoleMenuRepository sysRoleMenuRepository,
            ISysUserRepository sysUserRepository,
            ISysRoleRepository sysRoleRepository
            )
        {
            _logger = logger;
            _db = db;
            _sysMenuRepository = sysMenuRepository;
            _sysRoleMenuRepository = sysRoleMenuRepository;
            _sysUserRepository = sysUserRepository;
            _sysRoleRepository = sysRoleRepository;
        }

        /// <summary>
        /// 创建新的菜单。
        /// </summary>
        /// <param name="createDto">创建数据传输对象，包含 Name 等字段。</param>
        /// <param name="operatorId">操作人用户 ID，用于审计。</param>
        /// <returns>返回新创建记录的主键 ID。</returns>
        /// <exception cref="ArgumentException">当 Name 已存在时抛出。</exception>
        /// <exception cref="ArgumentNullException">当 createDto 为 null 时抛出。</exception>
        public async Task<long> CreateAsync(CreateSysMenuDto createDto, long operatorId)
        {
            if (createDto == null)
                throw new ArgumentNullException(nameof(createDto));

            if (string.IsNullOrWhiteSpace(createDto.Name))
                throw new ArgumentException("菜单名称不能为空。", nameof(createDto.Name));

            // 检查外链格式是否正确
            if (YES_FRAME.Equals(createDto.IsFrame) && (!createDto.Path?.StartsWith("http://") == true && !createDto.Path?.StartsWith("https://") == true))
            {
                throw new ArgumentException("外链必须以http(s)://开头");
            }

            var trimmedName = createDto.Name.Trim();

            var existsByName = await _sysMenuRepository.GetByNameAsync(trimmedName);
            if (existsByName != null)
                throw new ArgumentException($"菜单名称 '{trimmedName}' 已存在。");

            var entity = new SysMenu
            {
                Name = trimmedName,
                ParentId = createDto.ParentId,
                RouteName = createDto.RouteName?.Trim(),
                Sort = createDto.Sort,
                Path = createDto.Path?.Trim(),
                Component = createDto.Component?.Trim(),
                Query = createDto.Query?.Trim(),
                IsFrame = createDto.IsFrame?.Trim() ?? "0",
                IsCache = createDto.IsCache?.Trim() ?? "0",
                MenuType = createDto.MenuType?.Trim() ?? "C",
                Visible = createDto.Visible?.Trim() ?? "0",
                Perms = createDto.Perms?.Trim(),
                Icon = createDto.Icon?.Trim(),
                Status = createDto.Status?.Trim() ?? "1",
                CreatedById = operatorId,
                CreatedAt = DateTime.UtcNow,
                UpdatedById = operatorId,
                UpdatedAt = DateTime.UtcNow
            };

            var newId = await _sysMenuRepository.CreateAsync(entity);

            _logger.LogInformation("用户 {UserId} 创建菜单 {MenuName} (ID: {MenuId}) 成功。", operatorId, createDto.Name, newId);

            return newId;
        }

        /// <summary>
        /// 更新指定的菜单。
        /// </summary>
        /// <param name="updateDto">更新数据传输对象，必须包含有效 ID。</param>
        /// <param name="operatorId">操作人用户 ID。</param>
        /// <returns>返回被更新的记录 ID。</returns>
        /// <exception cref="ArgumentException">当记录不存在、已删除或更新后违反唯一性约束时抛出。</exception>
        /// <exception cref="ArgumentNullException">当 updateDto 为 null 时抛出。</exception>
        /// <exception cref="InvalidOperationException">当数据库更新未影响任何行时抛出（并发冲突）。</exception>
        public async Task<long> UpdateAsync(UpdateSysMenuDto updateDto, long operatorId)
        {
            if (updateDto == null)
                throw new ArgumentNullException(nameof(updateDto));
            if (updateDto.Id <= 0)
                throw new ArgumentException("无效的菜单 ID。", nameof(updateDto.Id));

            if (string.IsNullOrWhiteSpace(updateDto.Name))
                throw new ArgumentException("菜单名称不能为空。", nameof(updateDto.Name));
            // 检查外链格式是否正确
            if (YES_FRAME.Equals(updateDto.IsFrame) && (!updateDto.Path?.StartsWith("http://") == true && !updateDto.Path?.StartsWith("https://") == true))
            {
                throw new ArgumentException("外链必须以http(s)://开头");
            }
            // 上级菜单不能是自身
            if (updateDto.ParentId == updateDto.Id)
            {
                throw new ArgumentException("上级菜单不能是自身");
            }

            var entity = await _sysMenuRepository.GetAsync(updateDto.Id);
            if (entity == null)
                throw new ArgumentException("指定的菜单不存在或已被删除。", nameof(updateDto.Id));

            var trimmedName = updateDto.Name.Trim();

            if (entity.Name != trimmedName)
            {
                var exists = await _sysMenuRepository.GetByNameAsync(trimmedName);
                if (exists != null && exists.Id != entity.Id)
                    throw new ArgumentException($"菜单名称 '{trimmedName}' 已被占用。");
            }

            entity.Name = trimmedName;
            entity.ParentId = updateDto.ParentId;
            entity.RouteName = updateDto.RouteName?.Trim();
            entity.Sort = updateDto.Sort;
            entity.Path = updateDto.Path;
            entity.Component = updateDto.Component?.Trim();
            entity.Query = updateDto.Query;
            entity.IsFrame = updateDto.IsFrame?.Trim() ?? entity.IsFrame;
            entity.IsCache = updateDto.IsCache?.Trim() ?? entity.IsCache;
            entity.MenuType = updateDto.MenuType?.Trim() ?? entity.MenuType;
            entity.Visible = updateDto.Visible?.Trim() ?? entity.Visible;
            entity.Perms = updateDto.Perms?.Trim() ?? entity.Perms;
            entity.Icon = updateDto.Icon?.Trim() ?? entity.Icon;
            entity.Status = updateDto.Status?.Trim() ?? entity.Status;
            entity.UpdatedById = operatorId;
            entity.UpdatedAt = DateTime.UtcNow;

            var rowsAffected = await _sysMenuRepository.UpdateAsync(entity);
            if (rowsAffected == 0)
            {
                _logger.LogWarning("用户 {UserId} 更新菜单 {MenuName} (ID: {MenuId}) 失败，未影响任何数据库记录。", operatorId, entity.Name, entity.Id);
                throw new InvalidOperationException($"用户 {operatorId} 更新菜单 {entity.Name} (ID: {entity.Id}) 失败，未影响任何数据库记录。");
            }

            _logger.LogInformation("用户 {UserId} 更新菜单 {MenuName} (ID: {MenuId}) 成功。", operatorId, entity.Name, entity.Id);

            return entity.Id;
        }

        /// <summary>
        /// 软删除指定的菜单。
        /// </summary>
        /// <param name="id">要删除的菜单 ID。</param>
        /// <param name="operatorId">操作人用户 ID。</param>
        /// <returns>返回被删除的记录 ID。</returns>
        /// <exception cref="ArgumentException">当记录不存在或已被删除时抛出。</exception>
        public async Task<long> DeleteAsync(long id, long operatorId)
        {
            if (id <= 0)
                throw new ArgumentException("无效的菜单 ID。", nameof(id));

            var entity = await _sysMenuRepository.GetAsync(id);
            if (entity == null)
                throw new ArgumentException("菜单不存在或已被删除。");

            // 使用 UseTranAsync<long> 显式指定返回类型
            DbResult<long> result = await _db.Ado.UseTranAsync<long>(
                async () =>
                {
                    // 多级菜单不适用
                    //// 删除关联数据
                    //await _sysMenuRepository.DeleteByParentIdAsync(entity.Id, operatorId); 

                    //// 执行软删除
                    //entity.DeletedAt = DateTime.UtcNow;
                    //entity.DeletedById = operatorId;
                    //var rowsAffected = await _sysMenuRepository.DeleteAsync(entity);

                    //if (rowsAffected == 0)
                    //    throw new ArgumentException("无法删除：菜单不存在或已被删除。");

                    //return id; // 事务成功，返回 ID

                    // 批量删除关联的菜单
                    var menuList = await GetAllAsync();
                    var idList = GetRecursiveChildren(menuList, MapToDto(entity)).Select(x => x.Id).ToList();
                    var deletedCount = await _sysMenuRepository.DeleteBatchAsync(idList, operatorId);

                    // 校验删除数量
                    if (deletedCount != idList.Count)
                    {
                        _logger.LogWarning("用户 {UserId} 批量删除菜单失败，期望删除 {Count} 条，但实际仅成功删除 {DeletedCount} 条。", operatorId, idList.Count, deletedCount);
                        throw new ArgumentException($"用户 {operatorId} 批量删除菜单失败，期望删除 {idList.Count} 条，但实际仅成功删除 {deletedCount} 条。");
                    }

                    // 事务成功，返回删除数量
                    return deletedCount;
                },
                // 可选：使用 errorCallBack 记录日志
                ex =>
                {
                    _logger.LogWarning("用户 {UserId} 删除菜单 {MenuName} (ID: {MenuId}) 失败，{ErrorMessage}。", operatorId, entity.Name, entity.Id, ex.Message);
                }
            );

            // 检查结果并处理错误
            if (!result.IsSuccess)
            {
                // 关键：直接抛出原始异常以保留完整的 StackTrace
                throw result.ErrorException;
            }

            _logger.LogInformation("用户 {UserId} 删除菜单 {MenuName} (ID: {MenuId}) 成功。", operatorId, entity.Name, entity.Id);

            // 返回成功结果
            return result.Data;
        }

        /// <summary>
        /// 根据主键获取单条未删除的菜单详情。
        /// </summary>
        /// <param name="id">菜单唯一标识。</param>
        /// <returns>返回匹配的 DTO；若未找到或已删除，则返回 null。</returns>
        public async Task<SysMenuDto?> GetAsync(long id)
        {
            var entity = await _sysMenuRepository.GetAsync(id);
            if (entity == null)
                return null;
            return MapToDto(entity);
        }

        /// <summary>
        /// 查询未删除的菜单列表，支持按名称模糊搜索和状态精确筛选。
        /// </summary>
        /// <param name="name">菜单名称，用于模糊搜索（可选）。</param>
        /// <param name="status">菜单状态，用于精确匹配（可选）。</param>
        /// <returns>返回匹配条件的 DTO 列表（可能为空）。</returns>
        public async Task<List<SysMenuDto>> GetAllAsync(string? name = null, string? status = null)
        {
            var entities = await _sysMenuRepository.GetAllAsync(name, status);
            return entities.Select(MapToDto).ToList();
        }

        /// <summary>
        /// 根据菜单名称获取唯一未删除的菜单详情。
        /// </summary>
        /// <param name="name">菜单的显示名称（如“性别”），用于精确匹配。</param>
        /// <returns>返回匹配的 DTO；若未找到或已删除，则返回 null。</returns>
        public async Task<SysMenuDto?> GetByNameAsync(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return null;

            var entity = await _sysMenuRepository.GetByNameAsync(name.Trim());
            if (entity == null)
                return null;
            return MapToDto(entity);
        }

        /// <summary>
        /// 查询未删除的菜单列表，支持按用户 ID 精确筛选。
        /// </summary>
        /// <param name="userId">用户 ID ，用于精确匹配（可选）。</param>
        /// <returns>返回匹配条件的 DTO 列表（可能为空）。</returns>
        public async Task<List<SysMenuDto>> GetAllByUserIdAsync(long userId)
        {
            var sysUser = await _sysUserRepository.GetAsync(userId);
            var sysRoles = await _sysRoleRepository.GetAllByUserIdAsync(userId);
            // 角色包含 admin 则表示是超级管理员
            var IsAdmin = sysRoles.Any(r => r.Key == "admin");

            var entities = new List<SysMenu>();
            if (IsAdmin)
            {
                entities = await _sysMenuRepository.GetAllAsync();
            }
            else
            {
                entities = await _sysMenuRepository.GetAllByUserIdAsync(userId);
            }

            return entities.Select(MapToDto).ToList();
        }

        /// <summary>
        /// 查询未删除的菜单列表，支持按角色 ID 精确筛选。
        /// </summary>
        /// <param name="roleId">角色 ID ，用于精确匹配（可选）。</param>
        /// <returns>返回匹配条件的 DTO 列表（可能为空）。</returns>
        public async Task<List<SysMenuDto>> GetAllByRoleIdAsync(long roleId)
        {
            var entities = await _sysMenuRepository.GetAllByRoleIdAsync(roleId);
            return entities.Select(MapToDto).ToList();
        }

        /// <summary>
        /// 将领域实体映射为应用层数据传输对象。
        /// </summary>
        /// <param name="entity">源实体对象。</param>
        /// <returns>映射后的 SysMenuDto 实例。</returns>
        private static SysMenuDto MapToDto(SysMenu entity)
        {
            return new SysMenuDto
            {
                Id = entity.Id,
                Name = entity.Name,
                ParentId = entity.ParentId,
                RouteName = entity.RouteName,
                Sort = entity.Sort,
                Path = entity.Path,
                Component = entity.Component,
                Query = entity.Query,
                IsFrame = entity.IsFrame,
                IsCache = entity.IsCache,
                MenuType = entity.MenuType,
                Visible = entity.Visible,
                Perms = entity.Perms,
                Icon = entity.Icon,
                Status = entity.Status,
                CreatedAt = entity.CreatedAt,
                UpdatedAt = entity.UpdatedAt
            };
        }

        #region 前端构建相关

        /// <summary>
        /// 递归获取所有子菜单（平行结构）
        /// </summary>
        /// <param name="menus"></param>
        /// <param name="currentMenu"></param>
        /// <returns></returns>
        public List<SysMenuDto> GetRecursiveChildren(List<SysMenuDto> menus, SysMenuDto currentMenu)
        {
            var result = new List<SysMenuDto>();
            if (currentMenu == null) return result;

            result.Add(currentMenu); // 处理当前节点

            var children = menus.Where(x => x.ParentId == currentMenu.Id).ToList();
            foreach (var child in children)
            {
                result.AddRange(GetRecursiveChildren(menus, child)); // 递归子节点
            }

            return result;
        }

        /// <summary>
        /// 递归获取所有子菜单（栈模拟递归，无溢出问题）
        /// </summary>
        public List<SysMenuDto> GetRecursiveChildrenNonRecursive(List<SysMenuDto> menus, SysMenuDto currentMenu)
        {
            var result = new List<SysMenuDto>();
            var stack = new Stack<SysMenuDto>(); // 👈 手动创建一个“任务栈”

            // 1. 把起始节点压入栈
            if (currentMenu != null)
                stack.Push(currentMenu);

            // 2. 只要栈不为空，就继续处理
            while (stack.Count > 0)
            {
                // 3. 弹出栈顶元素（相当于“进入这个函数”）
                var menu = stack.Pop();

                // 4. 处理当前节点（相当于函数体中的逻辑）
                result.Add(menu);

                // 5. 找到它的所有直接子节点
                var children = menus.Where(x => x.ParentId == menu.Id).ToList();

                // 6. 把子节点“压入栈”，等待后续处理（相当于“准备递归调用”）
                // 为了保持“从左到右”访问顺序，必须“从右到左”压栈(是ABC，而不是ACB)
                //foreach (var child in children)
                //{
                //    stack.Push(child);
                //}
                for (int i = children.Count - 1; i >= 0; i--)
                {
                    stack.Push(children[i]);
                }
            }

            return result;
        }

        /// <summary>
        /// 递归获取所有子菜单（父子结构）
        /// </summary>
        /// <param name="menus">菜单列表</param>
        /// <param name="menu">当前菜单</param>
        private void RecursionFn(List<SysMenuDto> menus, SysMenuDto menu)
        {
            // 获取子菜单
            List<SysMenuDto> children = GetChildren(menus, menu);
            menu.Children = children;
            foreach (var item in children)
            {
                if (HasChild(menus, item))
                {
                    RecursionFn(menus, item);
                }
            }
        }

        /// <summary>
        /// 根据父节点的ID获取所有子节点
        /// </summary>
        /// <param name="list">分类表</param>
        /// <param name="parentId">传入的父节点ID</param>
        /// <returns></returns>
        public List<SysMenuDto> GetChildrenByParentId(List<SysMenuDto> menus, int parentId)
        {
            List<SysMenuDto> result = new List<SysMenuDto>();
            foreach (var item in menus)
            {
                // 一、根据传入的某个父节点ID,遍历该父节点的所有子节点
                if (item.ParentId == parentId)
                {
                    RecursionFn(menus, item);
                    result.Add(item);
                }
            }
            return result;
        }

        /// <summary>
        /// 获取子菜单
        /// </summary>
        /// <param name="menus">菜单列表</param>
        /// <param name="menu">当前菜单</param>
        /// <returns></returns>
        private List<SysMenuDto> GetChildren(List<SysMenuDto> menus, SysMenuDto menu)
        {
            List<SysMenuDto> result = new List<SysMenuDto>();
            foreach (var item in menus)
            {
                if (item.ParentId == menu.Id)
                {
                    result.Add(item);
                }
            }
            return result;
        }

        /// <summary>
        /// 判断菜单是否有子菜单
        /// </summary>
        /// <param name="menus"></param>
        /// <param name="menu"></param>
        /// <returns></returns>
        private bool HasChild(List<SysMenuDto> menus, SysMenuDto menu)
        {
            return GetChildren(menus, menu).Count > 0;
        }

        /// <summary>
        /// 是否为一级目录
        /// </summary>
        public bool IsLevel1Dir(SysMenuDto menu)
        {
            return menu.ParentId == 0 && TYPE_DIR.Equals(menu.MenuType) && NO_FRAME.Equals(menu.IsFrame);
        }

        /// <summary>
        /// 是否为一级菜单
        /// </summary>
        public bool IsLevel1Menu(SysMenuDto menu)
        {
            return menu.ParentId == 0 && TYPE_MENU.Equals(menu.MenuType) && NO_FRAME.Equals(menu.IsFrame) 
                && !(menu.Path?.StartsWith("http") == true || menu.Path?.StartsWith("https") == true);
        }

        /// <summary>
        /// 是否为内链方式打开外网链接
        /// </summary>
        public bool IsInnerLink(SysMenuDto menu)
        {
            return menu.IsFrame == NO_FRAME && (menu.Path?.StartsWith("http") == true || menu.Path?.StartsWith("https") == true);
        }

        /// <summary>
        /// 是否为parent_view组件
        /// </summary>
        public bool IsParentView(SysMenuDto menu)
        {
            return menu.ParentId != 0 && TYPE_DIR.Equals(menu.MenuType);
        }

        /// <summary>
        /// 获取路由名称
        /// </summary>
        public string GetRouterName(SysMenuDto menu)
        {
            string routerName = !string.IsNullOrWhiteSpace(menu.RouteName) ? menu.RouteName! : menu.Path!;
            // 转换成首字母大写，前端 keepAlive 大小写敏感
            routerName = routerName.Pascalize();

            if (IsLevel1Menu(menu))
            {
                routerName = string.Empty;
            }

            return routerName;
        }

        /// <summary>
        /// 获取路由地址
        /// </summary>
        public string GetRouterPath(SysMenuDto menu)
        {
            string routerPath = menu.Path!;
            // 内链方式打开外网链接
            if (menu.ParentId != 0 && IsInnerLink(menu))
            {
                routerPath = InnerLinkReplaceEach(routerPath);
            }
            // 一级目录
            else if (IsLevel1Dir(menu))
            {
                routerPath = "/" + menu.Path;
            }
            // 一级菜单
            else if (IsLevel1Menu(menu))
            {
                routerPath = "/";
            }
            return routerPath;
        }

        /// <summary>
        /// 获取组件信息
        /// </summary>
        public string GetComponent(SysMenuDto menu)
        {
            string component = LAYOUT;
            if (!string.IsNullOrWhiteSpace(menu.Component) && !IsLevel1Menu(menu))
            {
                component = menu.Component;
            }
            else if (string.IsNullOrWhiteSpace(menu.Component) && menu.ParentId != 0 && IsInnerLink(menu))
            {
                component = INNER_LINK;
            }
            else if (string.IsNullOrWhiteSpace(menu.Component) && IsParentView(menu))
            {
                component = PARENT_VIEW;
            }
            return component;
        }

        /// <summary>
        /// 将内链地址中的特定字符串替换为斜杠
        /// 在内链打开外网链接时路由path和name优化显示，如https://www.example.com 会变为 /example/com
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public string InnerLinkReplaceEach(string path)
        {
            var searchList = new string[] { "http://", "https://", "www.", "." };
            var replacementList = new string[] { "", "", "", "/" };

            for (var i = 0; i < searchList.Length; i++)
            {
               path = path.Replace(searchList[i], replacementList[i]);
            }

            return path;
        }

        /// <summary>
        /// 构建树数据
        /// </summary>
        /// <param name="menus">菜单列表</param>
        /// <returns></returns>
        public List<SysMenuDto> BuildTreeData(List<SysMenuDto> menus)
        {
            List<SysMenuDto> result = new List<SysMenuDto>();
            List<long> tempList = menus.Select(d => d.Id).ToList();
            foreach (var item in menus)
            {
                // 如果是当前菜单列表里的顶级菜单, 才开始遍历该父菜单的所有子菜单
                if (!tempList.Contains(item.ParentId))
                {
                    RecursionFn(menus, item);
                    result.Add(item);
                }
            }
            if (result.IsNullOrEmpty())
            {
                result = menus;
            }
            return result;
        }

        /// <summary>
        /// 构建前端需要的路由数据
        /// </summary>
        /// <param name="menus"></param>
        /// <returns></returns>
        public List<RouterVO> BuildRouterData(List<SysMenuDto> menus)
        {
            List<RouterVO> routers = new List<RouterVO>();
            foreach (var menu in menus)
            {
                RouterVO router = new RouterVO();
                router.Hidden = menu.Visible == "1";
                router.Name = GetRouterName(menu);
                router.Path = GetRouterPath(menu);
                router.Component = GetComponent(menu);
                router.Query = menu.Query;
                router.Meta = new MetaVO(menu.Name, menu.Icon, YES_CACHE.Equals(menu.IsCache), menu.Path);
                List<SysMenuDto>? cMenus = menu.Children;

                // 目录
                if (cMenus != null && TYPE_DIR.Equals(menu.MenuType))
                {
                    router.AlwaysShow = true;
                    router.Redirect = "noRedirect";
                    router.Children = BuildRouterData(cMenus);
                }
                // 一级菜单
                else if (IsLevel1Menu(menu))
                {
                    router.Meta = null;
                    List<RouterVO> childrenList = new List<RouterVO>();
                    RouterVO children = new RouterVO();
                    children.Path = menu.Path;
                    children.Component = menu.Component;
                    children.Name = (!string.IsNullOrWhiteSpace(menu.RouteName) ? menu.RouteName! : menu.Path!).Pascalize();
                    children.Meta = new MetaVO(menu.Name, menu.Icon, YES_CACHE.Equals(menu.IsCache), menu.Path);
                    children.Query = menu.Query;
                    childrenList.Add(children);
                    router.Children = childrenList;
                }
                // 内链方式打开外网链接（特殊一级菜单直接打开外网链接，和目录下的内链打开外链区分开）
                else if (menu.ParentId == 0 && IsInnerLink(menu))
                {
                    router.Meta = new MetaVO(menu.Name, menu.Icon);
                    router.Path = "/";
                    List<RouterVO> childrenList = new List<RouterVO>();
                    RouterVO children = new RouterVO();
                    string routerPath = InnerLinkReplaceEach(menu.Path!);
                    children.Path = routerPath;
                    children.Component = INNER_LINK;
                    children.Name = routerPath;
                    children.Meta = new MetaVO(menu.Name, menu.Icon, menu.Path);
                    childrenList.Add(children);
                    router.Children = childrenList;
                }

                routers.Add(router);
            }
            return routers;
        }

        /// <summary>
        /// 构建前端需要的树形选择控件数据
        /// </summary>
        /// <param name="menus">菜单列表</param>
        /// <returns></returns>
        public List<TreeSelectDto> BuildTreeSelectData(List<SysMenuDto> menus)
        {
            List<SysMenuDto> menuTree = BuildTreeData(menus);
            return menuTree.Select(m => new TreeSelectDto(m)).ToList();
        }

        /// <summary>
        /// 根据用户 ID 获取路由数据。
        /// </summary>
        /// <param name="userId">用户 ID</param>
        /// <returns></returns>
        public async Task<List<RouterVO>> GetRouterDataByUserId(long userId)
        {
            var menuList = await GetAllByUserIdAsync(userId);
            // 路由只需要目录和菜单
            menuList = menuList.Where(m => m.MenuType == "M" || m.MenuType == "C").ToList();
            // 转换为父子节点结构
            menuList = GetChildrenByParentId(menuList, 0);
            var routerList = BuildRouterData(menuList);

            return routerList;
        }

        /// <summary>
        /// 根据角色 ID 获取树形选择控件数据。
        /// </summary>
        /// <param name="roleId">角色 ID。</param>
        /// <returns></returns>
        public async Task<RoleMenuTreeSelectDto> GetTreeSelectDataByRoleId(long roleId)
        {
            // 获取所有菜单
            var menuList = await GetAllAsync();

            // 获取角色已关联的菜单列表
            var roleMenus = await GetAllByRoleIdAsync(roleId);

            // 构建菜单树
            var menuTree = BuildTreeSelectData(menuList);

            var result = new RoleMenuTreeSelectDto
            {
                CheckedKeys = roleMenus.Select(x => x.Id).ToList(),
                Menus = menuTree.ToList()
            };

            return result;
        }

        /// <summary>
        /// 根据用户 ID 获取树形选择控件数据。
        /// </summary>
        /// <param name="userId">用户 ID。</param>
        /// <returns></returns>
        public async Task<List<TreeSelectDto>> GetTreeSelectDataByUserId(long userId)
        {
            var menuList = await GetAllByUserIdAsync(userId);
            var treeSelectDataList = BuildTreeSelectData(menuList);

            return treeSelectDataList;
        }
        #endregion
    }
}