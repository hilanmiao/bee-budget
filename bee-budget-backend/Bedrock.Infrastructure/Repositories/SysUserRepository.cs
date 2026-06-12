using Bedrock.Application.DataTransferObjects;
using Bedrock.Application.Interfaces;
using Bedrock.Core.Entities;
using SqlSugar;

namespace Bedrock.Infrastructure.Repositories
{
    /// <summary>
    /// 系统用户仓储实现。
    /// <para>
    /// 支持软删除（通过 DeletedAt 字段标记），所有查询默认过滤已删除记录。
    /// 业务规则：UserName 保证全局唯一。
    /// </para>
    /// </summary>
    public class SysUserRepository : ISysUserRepository
    {
        private readonly ISqlSugarClient _db;

        /// <summary>
        /// 构造函数注入 SqlSugar 数据库上下文。
        /// </summary>
        /// <param name="db">数据库客户端实例。</param>
        public SysUserRepository(ISqlSugarClient db)
        {
            _db = db;
        }

        /// <summary>
        /// 创建一条新的用户记录。
        /// </summary>
        /// <param name="entity">待创建的实体。主键由数据库生成。</param>
        /// <returns>返回插入后生成的主键 ID。</returns>
        public async Task<long> CreateAsync(SysUser entity)
        {
            var id = await _db.Insertable(entity).ExecuteReturnBigIdentityAsync();
            return id;
        }

        /// <summary>
        /// 更新一条用户记录。
        /// </summary>
        /// <param name="entity">包含更新数据的实体，必须包含有效 Id。</param>
        /// <returns>返回受影响的记录数（通常为 1，若记录不存在则为 0）。</returns>
        public async Task<int> UpdateAsync(SysUser entity)
        {
            var rowsAffected = await _db.Updateable(entity).ExecuteCommandAsync();
            return rowsAffected;
        }

        /// <summary>
        /// 软删除指定的用户记录（标记 DeletedAt 和 DeletedById）。
        /// </summary>
        /// <param name="entity">要删除的实体，用于获取 Id 和审计信息。</param>
        /// <returns>返回受影响的记录数（1 或 0）。</returns>
        public async Task<int> DeleteAsync(SysUser entity)
        {
            var rowsAffected = await _db.Updateable(entity).ExecuteCommandAsync();
            return rowsAffected;
        }

        /// <summary>
        /// 批量软删除用户记录，并记录操作人。
        /// </summary>
        /// <param name="ids">要删除的记录 ID 列表。</param>
        /// <param name="operatorId">执行删除操作的用户 ID，用于审计。</param>
        /// <returns>返回成功标记为删除的记录数量；若 ID 列表为空，则返回 0。</returns>
        public async Task<int> DeleteBatchAsync(IEnumerable<long> ids, long operatorId)
        {
            var idList = ids as long[] ?? ids.ToArray();
            if (!idList.Any())
                return 0;

            var rowsAffected = await _db.Updateable<SysUser>()
                .SetColumns(e => new SysUser
                {
                    DeletedAt = DateTime.UtcNow,
                    DeletedById = operatorId
                })
                .Where(e => idList.Contains(e.Id))
                .ExecuteCommandAsync();

            return rowsAffected;
        }

        /// <summary>
        /// 根据主键获取单条未删除的用户记录。
        /// </summary>
        /// <param name="id">记录的唯一标识符。</param>
        /// <returns>匹配的实体；若未找到或已删除，则返回 null。</returns>
        public async Task<SysUser> GetAsync(long id)
        {
            var entity = await _db.Queryable<SysUser>()
                .Where(e => e.Id == id && e.DeletedAt == null)
                .FirstAsync();

            return entity;
        }

        /// <summary>
        /// 查询未删除的用户记录，支持按用户名称模糊搜索和状态精确筛选。
        /// </summary>
        /// <param name="userName">用户名称，用于模糊搜索（可选）。</param>
        /// <param name="status">用户状态，用于精确匹配（可选）。</param>
        /// <returns>匹配条件的未删除记录集合（可能为空）。</returns>
        public async Task<List<SysUser>> GetAllAsync(string? userName = null, string? status = null)
        {
            var query = _db.Queryable<SysUser>().Where(e => e.DeletedAt == null);

            if (!string.IsNullOrWhiteSpace(userName))
            {
                query = query.Where(e => e.UserName.Contains(userName));
            }

            if (!string.IsNullOrWhiteSpace(status))
            {
                query = query.Where(e => e.Status == status);
            }

            var entities = await query
                .OrderBy(e => e.CreatedAt, OrderByType.Desc)
                .ToListAsync();

            return entities;
        }

        /// <summary>
        /// 分页查询未删除的用户记录，支持名称、手机号码模糊搜索和状态精确筛选。
        /// </summary>
        /// <param name="pageNumber">页码，从 1 开始。</param>
        /// <param name="pageSize">每页记录数。</param>
        /// <param name="userName">用户名称，用于模糊搜索（可选）。</param>
        /// <param name="phoneNumber">用户手机号码，用于模糊搜索（可选）。</param>
        /// <param name="status">用户状态，用于精确匹配（可选）。</param>
        /// <param name="orderByField">排序字段（可选）。</param>
        /// <param name="orderByType">排序方式（可选）。</param>
        /// <returns>
        /// 元组包含分页结果：
        /// - Data：当前页的数据列表（可能为空）。
        /// - TotalCount：满足查询条件的总记录数。
        /// </returns>
        public async Task<(List<SysUser> Data, int TotalCount)> GetPagedAsync(
            int pageNumber,
            int pageSize,
            string? userName = null,
            string? phoneNumber = null,
            string? status = null,
            string? orderByField = null,
            string? orderByType = null)
        {
            // 排序字段映射：前端字段名 → 数据库字段名
            orderByField = (orderByField ?? "createdAt") switch
            {
                "id" => "id",
                "userName" => "user_name",
                "sort" => "sort",
                "createdAt" => "created_at",
                _ => "created_at"
            };
            orderByType = orderByType ?? "DESC";

            var query = _db.Queryable<SysUser>().Where(e => e.DeletedAt == null);

            if (!string.IsNullOrWhiteSpace(userName))
            {
                query = query.Where(e => e.UserName.Contains(userName));
            }

            if (!string.IsNullOrWhiteSpace(phoneNumber))
            {
                query = query.Where(e => (e.PhoneNumber ?? "").Contains(phoneNumber));
            }

            if (!string.IsNullOrWhiteSpace(status))
            {
                query = query.Where(e => e.Status == status);
            }

            var totalCount = await query.CountAsync();
            var pagedData = await query
                .OrderBy($"{orderByField} {orderByType}")
                .ToPageListAsync(pageNumber, pageSize);

            return (pagedData, totalCount);
        }

        /// <summary>
        /// 根据用户名称获取唯一未删除的记录。
        /// </summary>
        /// <param name="userName">用户的名称（如“admin”），用于精确匹配。</param>
        /// <returns>匹配的实体；若未找到或已删除，则返回 null。由于 UserName 唯一，最多返回一条。</returns>
        public async Task<SysUser?> GetByUserNameAsync(string userName)
        {
            if (string.IsNullOrWhiteSpace(userName))
                return null;

            var entity = await _db.Queryable<SysUser>()
                .Where(e => e.UserName == userName && e.DeletedAt == null)
                .FirstAsync();

            return entity;
        }

        /// <summary>
        /// 根据 ID 列表批量获取未删除的用户实体。
        /// </summary>
        /// <param name="ids">用户 ID 列表。</param>
        /// <returns>返回匹配的实体列表（可能为空）。</returns>
        public async Task<List<SysUser>> GetByIdsAsync(IEnumerable<long> ids)
        {
            if (ids == null)
                return new();

            var idList = ids as long[] ?? ids.ToArray();
            if (!idList.Any())
                return new();

            return await _db.Queryable<SysUser>()
                .Where(x => idList.Contains(x.Id))
                .Where(x => x.DeletedAt == null)
                .ToListAsync();
        }

        /// <summary>
        /// 获取用户摘要信息（包含用户基本信息和关联的角色列表）
        /// 使用“分步查询”模式：先查用户，再查角色
        /// 优势：
        ///   - 逻辑分离，易于调试、测试、扩展
        ///   - 无 JOIN 数据膨胀，无子查询依赖，兼容性最强
        ///   - 每一步均可独立加缓存、日志、权限控制
        ///   - 不依赖 SqlSugar 高级特性（如 Subqueryable），适合保守型项目
        /// </summary>
        /// <param name="id">用户ID</param>
        /// <returns>用户摘要信息，包含角色列表；若用户不存在，返回 null</returns>
        public async Task<SysUserSummary?> GetSummaryAsync(long id)
        {
            // 1. 【第一步：查询用户基本信息】
            //    单表查询，高效精准，无关联膨胀
            var userSummary = await _db.Queryable<SysUser>()
                .Where(u => u.Id == id && u.DeletedAt == null) // 主键 + 逻辑删除过滤
                .Select(u => new SysUserSummary
                {
                    Id = u.Id,
                    UserName = u.UserName,
                    NickName = u.NickName,
                    PhoneNumber = u.PhoneNumber,
                    Email = u.Email,
                    Sex = u.Sex,
                    Avatar = u.Avatar,
                    Status = u.Status,
                    Remark = u.Remark,
                    CreatedAt = u.CreatedAt,
                    Roles = new List<RoleSummary>() // 初始化空角色列表，避免 null
                })
                .FirstAsync();

            // 2. 【空值快速返回】用户不存在，直接返回 null
            if (userSummary == null)
                return null;

            // 3. 【第二步：查询该用户关联的角色列表】
            //    仅当用户存在时才执行，避免无效查询
            userSummary.Roles = await _db.Queryable<SysUserRole, SysRole>((ur, r) => new object[]
                {
            JoinType.Inner, ur.RoleId == r.Id // 角色必须存在
                })
                .Where((ur, r) => ur.UserId == id) // 关联当前用户
                .Select((ur, r) => new RoleSummary
                {
                    Id = r.Id,
                    Name = r.Name,
                    Key = r.Key
                })
                .ToListAsync(); // 返回角色列表（可能为空）

            // 4. 【最终返回】用户信息 + 角色列表（可能为空）
            return userSummary;
        }

        /// <summary>
        /// 获取用户摘要信息（包含用户基本信息和关联的角色列表）
        /// 使用 SqlFunc.Subqueryable 实现单次查询 + 嵌套集合
        /// 优势：
        ///   - 无数据重复（JOIN 膨胀问题）
        ///   - 单次数据库往返
        ///   - 支持直接 Select 投影 DTO
        ///   - 实体类无需 [Navigate] 特性
        /// </summary>
        /// <param name="id">用户ID</param>
        /// <returns>用户摘要信息，包含角色列表；若用户不存在，返回 null</returns>
        public async Task<SysUserSummary?> GetSummaryUseSubQueryAsync(long id)
        {
            // 1. 【主表查询】从 SysUser 开始，筛选目标用户
            var result = await _db.Queryable<SysUser>()
                .Where(u => u.Id == id && u.DeletedAt == null) // 主键 + 逻辑删除过滤
                .Select(u => new SysUserSummary
                {
                    // 2. 【主表字段映射】填充用户基本信息
                    Id = u.Id,
                    UserName = u.UserName,
                    NickName = u.NickName,
                    PhoneNumber = u.PhoneNumber,
                    Email = u.Email,
                    Sex = u.Sex,
                    Avatar = u.Avatar,
                    Status = u.Status,
                    Remark = u.Remark,
                    CreatedAt = u.CreatedAt,

                    // 3. 【子查询聚合】使用 SqlFunc.Subqueryable 查询关联角色
                    //    这是一个“关联子查询”，会为每一行用户执行一次
                    Roles = SqlFunc.Subqueryable<SysUserRole>()
                        // 3.1 【子查询内连接】关联 SysUserRole 和 SysRole
                        .InnerJoin<SysRole>((ur, r) => ur.RoleId == r.Id)
                        // 3.2 【关联外部查询】关键：ur.UserId == u.Id 中的 u 来自外部查询
                        .Where((ur, r) => ur.UserId == u.Id)
                        // 3.3 【投影到 DTO】直接构建 RoleSummary，避免加载完整实体
                        // 3.4 【聚合为列表】返回 List<RoleSummary>
                        //     框架会自动处理 FOR JSON / XML PATH 等聚合逻辑
                        .ToList((ur, r) => new RoleSummary
                        {
                            Id = r.Id,
                            Name = r.Name,
                            Key = r.Key
                        })
                })
                // 4. 【执行查询】获取结果
                //    因为主键查询，最多返回 1 条记录
                .FirstAsync(); // 如果无匹配，抛出异常？不，FirstAsync() 在无结果时返回 default(T)

            // 5. 【空值处理】FirstAsync() 在无结果时返回 null（对于 class 类型）
            return result; // 如果用户不存在，result 为 null
        }

        /// <summary>
        /// 使用 JOIN + 手动聚合方式，查询指定用户及其角色列表
        /// </summary>
        /// <param name="id">用户ID</param>
        /// <returns>用户详情与角色列表的聚合视图</returns>
        /// <remarks>
        /// 【设计思想】
        /// 采用单次多表 JOIN 查询 + 客户端 GroupBy 聚合，避免以下问题：
        /// 1. ❌ Subqueryable.ToList() 的 JSON 序列化/反序列化开销（数据库 & 客户端 CPU）
        /// 2. ❌ ThenMapper 的 N+1 查询问题（尤其在查询列表时）
        /// 3. ❌ 导航属性 ([Navigate]) 的实体侵入性和数据重复膨胀
        /// 
        /// 【性能优势】
        /// - ✅ 单次数据库查询，减少网络往返
        /// - ✅ 数据库执行简单 JOIN，索引友好，执行计划高效
        /// - ✅ 无 FOR JSON PATH 等聚合操作，降低数据库 CPU 负载
        /// - ✅ 传输扁平化原始字段，减少网络和内存开销
        /// - ✅ 客户端聚合 (GroupBy) 算法高效，适合大数据量
        /// 
        /// 【适用场景】
        /// 特别适合：用户列表、报表、高并发查询等性能敏感场景。
        /// 单用户查询同样适用，且为未来扩展（如分页列表）打下基础。
        /// </remarks>
        public async Task<SysUserSummary?> GetSummaryUseJoinAsync(long id)
        {
            // 1️⃣ 【JOIN 查询】一次性获取用户与角色的所有相关数据
            //     使用三表 INNER JOIN：
            //     - SysUser: 用户主表
            //     - SysUserRole: 用户-角色关联表
            //     - SysRole: 角色表
            //     条件：用户ID匹配 且 用户未删除
            var results = await _db.Queryable<SysUser, SysUserRole, SysRole>((u, ur, r) => new object[]
                {
                    JoinType.Left, ur.UserId == u.Id,     // 用户 ↔ 用户角色
                    JoinType.Left, ur.RoleId == r.Id      // 用户角色 ↔ 角色
                })
                .Where((u, ur, r) => u.Id == id && u.DeletedAt == null) // 查询条件
                .Select((u, ur, r) => new
                {
                    // 👇 用户基本信息（扁平化字段）
                    u.Id,
                    u.UserName,
                    u.NickName,
                    u.PhoneNumber,
                    u.Email,
                    u.Sex,
                    u.Avatar,
                    u.Status,
                    u.Remark,
                    u.CreatedAt,
                    // 👇 角色信息（扁平化字段）
                    RoleId = r == null ? null : (long?)r.Id, // 显式转为可空
                    Name = r == null ? null : r.Name,
                    Key = r == null ? null : r.Key
                    // 🔔 注意：此处不包含导航属性或复杂对象，仅为原始字段
                })
                .ToListAsync(); // 执行查询，返回 List<匿名类型>

            // 2️⃣ 【空值检查】如果查询结果为空，直接返回 null
            if (!results.Any())
            {
                return null;
            }

            // 3️⃣ 【手动聚合】将扁平化数据聚合成嵌套结构
            //     使用 GroupBy 按 UserId 分组，每组代表一个用户的全部角色
            var grouped = results.GroupBy(x => x.Id).First(); // 因为只查一个用户，取第一组

            // 4️⃣ 【构建最终 DTO】
            //     - 用户信息取自分组内的任意一条记录（所有记录用户信息相同）
            //     - 角色列表通过 Select 投影构建，并转换为 List
            return new SysUserSummary
            {
                Id = grouped.Key,                               // 分组键即 UserId
                UserName = grouped.First().UserName,
                NickName = grouped.First().NickName,
                PhoneNumber = grouped.First().PhoneNumber,
                Email = grouped.First().Email,
                Sex = grouped.First().Sex,
                Avatar = grouped.First().Avatar,
                Remark = grouped.First().Remark,
                Status = grouped.First().Status,
                CreatedAt = grouped.First().CreatedAt,
                Roles = grouped.Where(x => x.RoleId != null)  // 过滤掉因 LEFT JOIN 产生的 null 角色
                .Select(x => new RoleSummary     // 聚合该用户的所有角色
                {
                    Id = x.RoleId,
                    Name = x.Name,
                    Key = x.Key
                }).ToList()
            };
        }

        /// <summary>
        /// 获取所有符合条件的用户及其角色信息（高性能两步查询 + 聚合）
        /// ⚠️【重要】本方法适用于小数据量场景（如 < 5000 条），大数据量请使用分页或子查询版本！
        /// </summary>
        /// <param name="userName">用户名关键词（模糊查询）</param>
        /// <param name="status">用户状态</param>
        /// <returns>用户摘要列表（含角色）</returns>
        /// <remarks>
        /// ✅【优势】
        ///   - 无数据冗余：用户信息与角色信息分开查询，避免 JOIN 膨胀
        ///   - 性能更优（小数据量）：避免大表 JOIN + 内存 GroupBy
        ///   - 易扩展：支持分页、缓存、权限控制（未来可轻松改造为分页接口）
        ///   - 稳定可靠：不依赖复杂 SQL 或 ORM 高级特性（如 Subqueryable）
        ///   - 逻辑清晰：调试、测试、加日志/缓存都非常方便
        /// 
        /// ⚠️【严重劣势与风险】❗❗❗
        ///   - ❗【Contains 爆炸风险】第二步使用 userIds.Contains(ur.UserId) 生成 IN 查询，
        ///       若用户数量 > 2100（SQL Server）或 > 5000（MySQL/PG），将导致：
        ///         → SQL Server：直接报错 "The incoming request has too many parameters"
        ///         → MySQL/PG：性能急剧下降，可能拖垮数据库
        ///   - ❗【内存压力】一次性加载全部用户 + 角色，可能导致 OOM（Out of Memory）
        ///   - ❗【两次 Round-trip】比子查询或 JOIN 多一次数据库往返
        ///   - ❗【不适合开放接口】前端或外部系统调用极易引发服务雪崩
        /// 
        /// 🚫【禁止使用场景】
        ///   - 前端页面直接调用（应使用 GetPagedSummaryAsync）
        ///   - 无数量限制的开放 API
        ///   - 高并发、低延迟核心路径（如登录、网关、首页）
        /// 
        /// ✅【安全使用建议】
        ///   - 仅限后台管理、数据导出、定时任务等“受控环境”调用
        ///   - 调用前必须加数量限制（见下方熔断代码）
        ///   - 可改造为“自动分批查询”版本（参考 GetPagedSummaryAsync 逻辑）
        ///   - 强烈建议替换为子查询版本（GetAllSummaryUseSubQueryAsync）或分页接口
        ///   - 确保 SysUserRole.UserId 有索引（否则第二步查询极慢！）
        /// 
        /// 🔄【替代方案对比】
        ///   - 【子查询版本】→ GetAllSummaryUseSubQueryAsync：无 Contains 风险，单次查询，推荐替换！
        ///   - 【分页版本】→ GetPagedSummaryAsync：生产环境首选，限制 pageSize，安全可控
        ///   - 【JOIN + GroupBy】→ 不推荐：数据冗余严重，排序性能差
        /// </remarks>
        public async Task<List<SysUserSummary>> GetAllSummaryAsync(
            string? userName = null,
            string? status = null)
        {
            // 🔥【熔断保护】防止数据量过大导致 IN 查询爆炸 + OOM
            const int MaxSafeUserCount = 5000; // 根据你的数据库和业务调整

            var totalCount = await _db.Queryable<SysUser>()
                .Where(u => u.DeletedAt == null)
                .WhereIF(!string.IsNullOrWhiteSpace(userName), u => u.UserName.Contains(userName ?? ""))
                .WhereIF(!string.IsNullOrWhiteSpace(status), u => u.Status == status)
                .CountAsync();

            if (totalCount > MaxSafeUserCount)
            {
                throw new InvalidOperationException(
                    $"查询数据量过大（{totalCount} > {MaxSafeUserCount}），请使用分页接口、子查询版本或联系管理员导出。");
            }

            // 1️⃣ 【第一步：查询符合条件的用户列表】
            var userQuery = _db.Queryable<SysUser>()
                .Where(u => u.DeletedAt == null); // 基础过滤：未删除

            // 动态条件
            if (!string.IsNullOrWhiteSpace(userName))
            {
                userQuery = userQuery.Where(u => u.UserName.Contains(userName));
            }

            if (!string.IsNullOrWhiteSpace(status))
            {
                userQuery = userQuery.Where(u => u.Status == status);
            }

            // 排序（仅对用户表，高效！）
            userQuery = userQuery.OrderBy(u => u.CreatedAt, OrderByType.Desc);

            // 投影到 SysUserSummary（角色先留空）
            var users = await userQuery
                .Select(u => new SysUserSummary
                {
                    Id = u.Id,
                    UserName = u.UserName,
                    NickName = u.NickName,
                    PhoneNumber = u.PhoneNumber,
                    Email = u.Email,
                    Sex = u.Sex,
                    Avatar = u.Avatar,
                    Status = u.Status,
                    Remark = u.Remark,
                    CreatedAt = u.CreatedAt,
                    Roles = new List<RoleSummary>() // 初始化空列表
                })
                .ToListAsync();

            // 2️⃣ 【快速短路】无用户，直接返回空列表
            if (!users.Any())
                return users;

            // 3️⃣ 【第二步：批量查询这些用户的所有角色】
            var userIds = users.Select(u => u.Id).ToList();

            var roleMap = await _db.Queryable<SysUserRole, SysRole>((ur, r) => new object[]
                {
            JoinType.Inner, ur.RoleId == r.Id // 只查存在的角色
                })
                .Where((ur, r) => userIds.Contains(ur.UserId)) // ⚠️ Contains 风险已被熔断保护！
                .Select((ur, r) => new
                {
                    ur.UserId,
                    Role = new RoleSummary
                    {
                        Id = r.Id,
                        Name = r.Name,
                        Key = r.Key
                    }
                })
                .ToListAsync();

            // 4️⃣ 【内存聚合：按 UserId 分组角色】
            var roleGroups = roleMap
                .GroupBy(x => x.UserId)
                .ToDictionary(g => g.Key, g => g.Select(x => x.Role).ToList());

            // 5️⃣ 【为每个用户填充角色列表】
            foreach (var user in users)
            {
                user.Roles = roleGroups.TryGetValue(user.Id, out var roles)
                    ? roles
                    : new List<RoleSummary>();
            }

            // 6️⃣ 【返回最终结果】
            return users;
        }

        /// <summary>
        /// 获取所有符合条件的用户及其角色信息（使用 SqlFunc.Subqueryable 实现单次查询 + 嵌套集合）
        /// 适用于“查全部”场景，无 Contains 爆炸风险，无 JOIN 数据冗余。
        /// </summary>
        /// <param name="userName">用户名关键词（模糊查询）</param>
        /// <param name="status">用户状态</param>
        /// <returns>用户摘要列表（含角色）</returns>
        /// <remarks>
        /// ✅【优势】
        ///   - 无数据重复：避免 JOIN 导致的用户字段膨胀
        ///   - 无 Contains 风险：不依赖 IN (...)，适合大数据量查询（突破 2100 参数限制）
        ///   - 单次数据库往返：比“两步法”减少网络开销
        ///   - 支持直接 Select 投影 DTO：无需加载完整实体，性能更优
        ///   - 实体类无需 [Navigate] 特性：解耦性强，适合保守型项目
        /// 
        /// ⚠️【劣势与风险】❗❗❗
        ///   - ❗【子查询性能成本】每行用户都会触发一次子查询（数据库内部优化 ≠ 真 N+1，但仍有开销）
        ///       → 用户量 > 10 万时，建议改用分页或导出专用接口
        ///   - ❗【内存压力】一次性加载全部用户 + 角色，可能导致 OOM（尤其角色多、字段大时）
        ///       → 建议加熔断保护：if (totalCount > 5000) throw ...
        ///   - ❗【排序限制】ORDER BY 必须在主查询（用户表）完成，不能按角色字段排序
        ///       → 如需“按角色名排序”，此方案不适用
        ///   - ❗【执行计划复杂】超大数据量时，子查询可能不如 JOIN + 索引高效（需 DBA 调优）
        ///   - ❗【数据库兼容性】部分老旧数据库（如 MySQL 5.7）对子查询聚合支持有限
        ///       → SqlSugar 会自动降级处理，但可能影响性能
        /// 
        /// 🚫【禁止使用场景】
        ///   - 前端页面直接调用（应使用分页接口）
        ///   - 无数量限制的开放 API
        ///   - 高并发、低延迟核心路径（如登录、网关）
        /// 
        /// ✅【安全建议】
        ///   - 调用前加数量限制（强烈推荐！）：
        ///         var count = await _db.Queryable<...>.CountAsync();
        ///         if (count > MaxSafeCount) throw ...
        ///   - 确保 SysUserRole.UserId 有索引（否则子查询极慢！）
        ///   - 大数据量导出 → 改用分页 + 分批 + 异步队列
        ///   - 监控慢 SQL：关注执行时间 > 1s 的查询
        /// </remarks>
        public async Task<List<SysUserSummary>> GetAllSummaryUseSubQueryAsync(
            string? userName = null,
            string? status = null)
        {
            // 🔥【熔断保护】防止数据量过大拖垮系统（强烈建议保留！）
            const int MaxSafeUserCount = 5000; // 根据业务和数据库性能调整

            var totalCount = await _db.Queryable<SysUser>()
                .Where(u => u.DeletedAt == null)
                .WhereIF(!string.IsNullOrWhiteSpace(userName), u => u.UserName.Contains(userName ?? ""))
                .WhereIF(!string.IsNullOrWhiteSpace(status), u => u.Status == status)
                .CountAsync();

            if (totalCount > MaxSafeUserCount)
            {
                throw new InvalidOperationException(
                    $"查询数据量过大（{totalCount} > {MaxSafeUserCount}），请使用分页接口或联系管理员导出。");
            }

            // 1. 【主表查询】从 SysUser 开始，筛选符合条件的用户
            var query = _db.Queryable<SysUser>()
                .Where(u => u.DeletedAt == null);

            if (!string.IsNullOrWhiteSpace(userName))
                query = query.Where(u => u.UserName.Contains(userName));
            if (!string.IsNullOrWhiteSpace(status))
                query = query.Where(u => u.Status == status);

            // 2. 【投影 + 子查询】构建最终结果
            var result = await query
                .Select(u => new SysUserSummary
                {
                    // 3. 【主表字段映射】
                    Id = u.Id,
                    UserName = u.UserName,
                    NickName = u.NickName,
                    PhoneNumber = u.PhoneNumber,
                    Email = u.Email,
                    Sex = u.Sex,
                    Avatar = u.Avatar,
                    Status = u.Status,
                    Remark = u.Remark,
                    CreatedAt = u.CreatedAt,

                    // 4. 【子查询聚合角色】—— 无 Contains，无爆炸风险！
                    Roles = SqlFunc.Subqueryable<SysUserRole>()
                        .InnerJoin<SysRole>((ur, r) => ur.RoleId == r.Id)
                        .Where((ur, r) => ur.UserId == u.Id) // ✅ 关联当前用户，非 IN
                        .ToList((ur, r) => new RoleSummary
                        {
                            Id = r.Id,
                            Name = r.Name,
                            Key = r.Key
                        })
                })
                .ToListAsync(); // 5. 【执行查询】

            return result;
        }

        /// <summary>
        /// 获取所有符合条件的用户及其角色信息（高性能 JOIN + 聚合）
        /// </summary>
        /// <param name="userName">用户名关键词（模糊查询）</param>
        /// <param name="status">用户状态</param>
        /// <returns>用户摘要列表（含角色）</returns>
        /// <remarks>
        /// ⚠️ 注意：此方法会查询所有匹配数据，请确保调用场景合理（如：数据量不大或有强过滤条件）
        ///      若数据量大，建议改用分页查询。
        /// </remarks>
        public async Task<List<SysUserSummary>> GetAllSummaryUseJoinAsync(
            string? userName = null,
            string? status = null)
        {
            // 1️⃣ 【构建三表 JOIN 查询】使用 SqlSugar 多表语法
            var query = _db.Queryable<SysUser, SysUserRole, SysRole>((u, ur, r) => new object[]
            {
                JoinType.Left, ur.UserId == u.Id,     // 用户 ↔ 用户角色
                JoinType.Left, ur.RoleId == r.Id      // 用户角色 ↔ 角色
            })
            // 2️⃣ 【基础过滤】用户未删除
            .Where((u, ur, r) => u.DeletedAt == null);

            // 3️⃣ 【动态条件】仅在参数存在时添加
            if (!string.IsNullOrWhiteSpace(userName))
            {
                query = query.Where((u, ur, r) => u.UserName.Contains(userName));
            }

            if (!string.IsNullOrWhiteSpace(status))
            {
                query = query.Where((u, ur, r) => u.Status == status);
            }

            // 4️⃣ 【排序】按创建时间倒序
            query = query.OrderBy((u, ur, r) => u.CreatedAt, OrderByType.Desc);

            // 5️⃣ 【执行查询】获取扁平化数据（用户 + 角色）
            var results = await query
                .Select((u, ur, r) => new
                {
                    // 👇 用户字段
                    u.Id,
                    u.UserName,
                    u.NickName,
                    u.PhoneNumber,
                    u.Email,
                    u.Sex,
                    u.Avatar,
                    u.Status,
                    u.Remark,
                    u.CreatedAt,
                    // 👇 角色字段
                    RoleId = r == null ? null : (long?)r.Id, // 显式转为可空
                    Name = r == null ? null : r.Name,
                    Key = r == null ? null : r.Key
                })
                .ToListAsync(); // 获取所有匹配记录

            // 6️⃣ 【空值检查】避免 GroupBy 报错
            if (!results.Any())
            {
                return new(); // 返回空列表
            }

            // 7️⃣ 【手动聚合】按用户ID分组，聚合成嵌套结构
            return results
                .GroupBy(x => x.Id) // 按用户ID分组
                .Select(g => new SysUserSummary
                {
                    Id = g.Key,
                    UserName = g.First().UserName,
                    NickName = g.First().NickName,
                    PhoneNumber = g.First().PhoneNumber,
                    Email = g.First().Email,
                    Sex = g.First().Sex,
                    Avatar = g.First().Avatar,
                    Status = g.First().Status,
                    Remark = g.First().Remark,
                    CreatedAt = g.First().CreatedAt,
                    Roles = g.Where(x => x.RoleId != null)  // 过滤掉因 LEFT JOIN 产生的 null 角色
                    .Select(x => new RoleSummary
                    {
                        Id = x.RoleId,
                        Name = x.Name,
                        Key = x.Key
                    }).ToList()
                })
                .ToList(); // 转为 List
        }

        /// <summary>
        /// 分页查询未删除的用户记录，支持名称、手机号码模糊搜索和状态精确筛选。
        /// ✅ 生产环境推荐方案 —— 但需控制 pageSize，防止 IN 查询爆炸！
        /// </summary>
        /// <param name="pageNumber">页码，从 1 开始。</param>
        /// <param name="pageSize">每页记录数（建议 ≤ 100，最大 ≤ 2000）。</param>
        /// <param name="userName">用户名称，用于模糊搜索（可选）。</param>
        /// <param name="phoneNumber">用户手机号码，用于模糊搜索（可选）。</param>
        /// <param name="status">用户状态，用于精确匹配（可选）。</param>
        /// <param name="orderByField">排序字段（可选）。</param>
        /// <param name="orderByType">排序方式（可选）。</param>
        /// <returns>
        /// 元组包含分页结果：
        /// - Data：当前页的数据列表（可能为空）。
        /// - TotalCount：满足查询条件的总记录数。
        /// </returns>
        /// <remarks>
        /// ✅【优势】
        ///   - 性能稳定：分页限制数据量，避免全表加载
        ///   - 无数据冗余：用户信息与角色信息分开查询，避免 JOIN 膨胀
        ///   - 排序高效：在用户表完成，可利用索引（CreatedAt DESC）
        ///   - 内存友好：仅加载当前页用户 + 对应角色
        ///   - 易扩展：支持缓存、权限过滤、字段裁剪
        ///   - 接口兼容：调用方无感知，可平滑替换旧接口
        /// 
        /// ⚠️【严重劣势与风险】❗❗❗
        ///   - ❗【Contains 爆炸风险】第二步使用 userIds.Contains(ur.UserId) 生成 IN 查询，
        ///       若 pageSize > 2100（SQL Server）或 > 5000（MySQL/PG），将导致：
        ///         → SQL Server：直接报错 "The incoming request has too many parameters"
        ///         → MySQL/PG：性能急剧下降，可能拖垮数据库
        ///   - ❗【恶意 pageSize 攻击】前端传 pageSize=10000 → IN (...) 爆炸 + OOM
        ///   - ❗【两次 Round-trip】比子查询或 JOIN 多一次数据库往返
        ///   - ❗【角色数据倾斜】若某页用户角色特别多（如 100 个/人），内存压力陡增
        /// 
        /// 🚫【禁止使用场景】
        ///   - pageSize > 2000（必须限制！）
        ///   - 无前端限制的开放 API（应加网关限流 + 参数校验）
        ///   - 角色数量不可控的业务（如“超级管理员”有 500 个角色）
        /// 
        /// ✅【安全使用建议】🔥（强烈推荐实施！）
        ///   - 🔒【强制限制 pageSize】：
        ///         if (pageSize > MaxPageSize) throw ...
        ///   - 📏【推荐 pageSize】：10 / 20 / 50 / 100（前端默认值）
        ///   - 🧱【熔断保护】：即使分页，也限制单页最大用户数（见下方代码）
        ///   - 📊【监控告警】：记录执行时间 > 500ms 的请求，分析慢 SQL
        ///   - 🧩【索引保障】：确保 SysUserRole.UserId 有索引！
        /// 
        /// 🔄【替代方案对比】
        ///   - 【子查询版本】→ GetAllSummaryUseSubQueryAsync：无 Contains 风险，但不适合分页（排序限制）
        ///   - 【JOIN + GroupBy】→ 不推荐：数据冗余严重，分页排序性能差
        ///   - 【游标分页】→ 适合超大数据量 + 实时性要求高场景（如 feed 流）
        /// 
        /// 🧭【未来演进方向】
        ///   - 支持字段裁剪（Select 指定字段）
        ///   - 支持角色缓存（减少重复查询）
        ///   - 支持异步预加载下一页（提升体验）
        /// </remarks>
        public async Task<(List<SysUserSummary> Data, int TotalCount)> GetPagedSummaryAsync(
            int pageNumber,
            int pageSize,
            string? userName = null,
            string? phoneNumber = null,
            string? status = null,
            string? orderByField = null,
            string? orderByType = null)
        {
            // 排序字段映射：前端字段名 → 数据库字段名
            orderByField = (orderByField ?? "createdAt") switch
            {
                "id" => "id",
                "userName" => "user_name",
                "sort" => "sort",
                "createdAt" => "created_at",
                _ => "created_at"
            };
            orderByType = orderByType ?? "DESC";

            // 🔥【安全熔断 1：限制 pageSize】防止恶意请求
            const int MaxPageSize = 2000; // 根据数据库能力调整（SQL Server 建议 ≤ 2000）
            const int RecommendedPageSize = 100;

            if (pageSize > MaxPageSize)
            {
                throw new ArgumentException($"pageSize 不能超过 {MaxPageSize}，建议使用 {RecommendedPageSize}。");
            }

            if (pageSize <= 0)
            {
                pageSize = RecommendedPageSize;
            }

            if (pageNumber <= 0)
            {
                pageNumber = 1;
            }

            // 1️⃣ 【构建用户查询（用于分页和计数）】
            var userQuery = _db.Queryable<SysUser>()
                .Where(u => u.DeletedAt == null); // 未删除

            // 动态条件
            if (!string.IsNullOrWhiteSpace(userName))
            {
                userQuery = userQuery.Where(u => u.UserName.Contains(userName));
            }

            if (!string.IsNullOrWhiteSpace(phoneNumber))
            {
                userQuery = userQuery.Where(u => (u.PhoneNumber ?? "").Contains(phoneNumber));
            }

            if (!string.IsNullOrWhiteSpace(status))
            {
                userQuery = userQuery.Where(u => u.Status == status);
            }

            // 2️⃣ 【获取总记录数】（独立查询，高效）
            var totalCount = await userQuery.CountAsync();

            // 3️⃣ 【快速短路】无数据直接返回
            if (totalCount == 0)
            {
                return (new List<SysUserSummary>(), 0);
            }

            // 4️⃣ 【分页查询当前页用户】（排序在用户表完成，高效！）
            var pagedUsers = await userQuery
                .OrderBy($"{orderByField} {orderByType}")
                .Select(u => new SysUserSummary
                {
                    Id = u.Id,
                    UserName = u.UserName,
                    NickName = u.NickName,
                    PhoneNumber = u.PhoneNumber,
                    Email = u.Email,
                    Sex = u.Sex,
                    Avatar = u.Avatar,
                    Status = u.Status,
                    Remark = u.Remark,
                    CreatedAt = u.CreatedAt,
                    Roles = new List<RoleSummary>() // 初始化空列表
                })
                .ToPageListAsync(pageNumber, pageSize);

            // 5️⃣ 【快速短路】当前页无数据
            if (!pagedUsers.Any())
            {
                return (pagedUsers, totalCount);
            }

            // 🔥【安全熔断 2：二次校验当前页用户数】防止数据倾斜或 pageSize 被绕过
            if (pagedUsers.Count > MaxPageSize)
            {
                throw new InvalidOperationException(
                    $"当前页数据量异常（{pagedUsers.Count} > {MaxPageSize}），请检查分页参数或联系管理员。");
            }

            // 6️⃣ 【批量查询当前页用户的角色】
            var userIds = pagedUsers.Select(u => u.Id).ToList();

            // 🔥【安全熔断 3：再次校验 userIds 数量】—— 防御性编程！
            if (userIds.Count > MaxPageSize)
            {
                throw new InvalidOperationException(
                    $"角色查询用户数异常（{userIds.Count} > {MaxPageSize}），请检查分页逻辑。");
            }

            // 7 【先查询扁平数据】
            var flatList = await _db.Queryable<SysUserRole, SysRole>((ur, r) => new object[]
                {
                    JoinType.Inner, ur.RoleId == r.Id // 只查存在的角色
                })
                .Where((ur, r) => userIds.Contains(ur.UserId)) // ⚠️ Contains 风险已被 pageSize 限制！
                .Select((ur, r) => new
                {
                    ur.UserId,
                    RoleId = r.Id,
                    r.Name,
                    r.Key
                })
                .ToListAsync();

            // 8 【构建角色映射字典】再在内存中组装对象
            var roleMap = flatList
                .GroupBy(x => x.UserId)
                .ToDictionary(
                g => g.Key,
                g => g.Select(x => new RoleSummary
                {
                    Id = x.RoleId,
                    Name = x.Name,
                    Key = x.Key
                }).ToList()
                );

            // 9 【为每个用户填充角色列表】
            foreach (var user in pagedUsers)
            {
                user.Roles = roleMap.TryGetValue(user.Id, out var roles)
                    ? roles
                    : new List<RoleSummary>();
            }

            // 9️⃣ 【返回结果】保持接口兼容
            return (pagedUsers, totalCount);
        }

        /// <summary>
        /// 分页查询未删除的用户记录（使用 SqlFunc.Subqueryable 嵌套角色查询）
        /// ✅ 无 Contains 爆炸风险 ✅ 无内存聚合 ✅ 单次数据库往返 ✅ 生产环境安全首选
        /// </summary>
        /// <param name="pageNumber">页码，从 1 开始。</param>
        /// <param name="pageSize">每页记录数（建议 ≤ 100，最大 ≤ 2000）。</param>
        /// <param name="userName">用户名称，用于模糊搜索（可选）。</param>
        /// <param name="phoneNumber">用户手机号码，用于模糊搜索（可选）。</param>
        /// <param name="status">用户状态，用于精确匹配（可选）。</param>
        /// <returns>
        /// 元组包含分页结果：
        /// - Data：当前页的数据列表（含角色）。
        /// - TotalCount：满足查询条件的总记录数。
        /// </returns>
        /// <remarks>
        /// ✅【核心优势】
        ///   - 🚫 无 Contains 风险：不使用 userIds.Contains(...)，彻底规避 SQL Server 2100 参数限制
        ///   - ⚡ 单次查询：用户 + 角色一次往返，比两步法更少网络开销
        ///   - 🧠 无内存聚合：不 GroupBy、不 ToDictionary，避免内存压力
        ///   - 📈 排序安全：ORDER BY 在主表（用户表）完成，稳定高效
        ///   - 🧩 无需 [Navigate]：实体解耦，适合保守型项目
        ///   - 🧭 易分页：天然兼容 ToPageListAsync，无数据膨胀
        /// 
        /// ⚠️【劣势与风险】❗❗❗
        ///   - ❗【子查询性能成本】每行用户执行一次子查询（数据库内部优化 ≠ 真 N+1）
        ///       → 用户量 > 10 万时，建议加索引或改用游标分页
        ///   - ❗【排序限制】不能按角色字段排序（如“按角色名排序”），只能按用户字段
        ///   - ❗【执行计划复杂】超大数据量时，子查询可能不如 JOIN + 索引高效（需 DBA 调优）
        ///   - ❗【数据库兼容性】部分老旧数据库（如 MySQL 5.7）对子查询聚合支持有限
        ///       → SqlSugar 会自动降级，但可能影响性能
        /// 
        /// 🚫【禁止使用场景】
        ///   - 需要“按角色字段排序”（如 OrderBy Name）
        ///   - 超高并发 + 超低延迟核心路径（如登录、网关）—— 用缓存！
        ///   - 无索引的 SysUserRole.UserId 字段（会导致子查询极慢！）
        /// 
        /// ✅【安全使用建议】🔥
        ///   - 🔒【强制限制 pageSize】：if (pageSize > MaxPageSize) throw ...
        ///   - 📏【推荐 pageSize】：10 / 20 / 50 / 100（前端默认值）
        ///   - 🧱【索引保障】：SysUserRole.UserId 必须有索引！
        ///   - 📊【监控告警】：记录执行时间 > 500ms 的请求
        ///   - 🧩【字段裁剪】：如不需要角色，可提供轻量版接口
        /// 
        /// 🔄【替代方案对比】
        ///   - 【两步法】→ GetPagedSummaryAsync：适合小 pageSize + 需要角色缓存场景
        ///   - 【JOIN + GroupBy】→ 不推荐：数据冗余严重，分页排序性能差
        ///   - 【游标分页】→ 适合超大数据量 + 实时性要求高场景（如 feed 流）
        /// 
        /// 🧭【未来演进方向】
        ///   - 支持字段裁剪（Select 指定字段）
        ///   - 支持角色缓存（减少子查询开销）
        ///   - 支持异步预加载下一页（提升体验）
        /// </remarks>
        public async Task<(List<SysUserSummary> Data, int TotalCount)> GetPagedSummaryUseSubQueryAsync(
            int pageNumber,
            int pageSize,
            string? userName = null,
            string? phoneNumber = null,
            string? status = null)
        {
            // 🔥【安全熔断：限制 pageSize】防止恶意请求
            const int MaxPageSize = 2000;
            const int RecommendedPageSize = 100;

            if (pageSize > MaxPageSize)
            {
                throw new ArgumentException($"pageSize 不能超过 {MaxPageSize}，建议使用 {RecommendedPageSize}。");
            }

            if (pageSize <= 0) pageSize = RecommendedPageSize;
            if (pageNumber <= 0) pageNumber = 1;

            // 1️⃣ 【构建基础查询】
            var userQuery = _db.Queryable<SysUser>()
                .Where(u => u.DeletedAt == null);

            // 动态条件
            if (!string.IsNullOrWhiteSpace(userName))
                userQuery = userQuery.Where(u => u.UserName.Contains(userName));
            if (!string.IsNullOrWhiteSpace(phoneNumber))
                userQuery = userQuery.Where(u => (u.PhoneNumber ?? "").Contains(phoneNumber));
            if (!string.IsNullOrWhiteSpace(status))
                userQuery = userQuery.Where(u => u.Status == status);

            // 2️⃣ 【获取总记录数】
            var totalCount = await userQuery.CountAsync();

            // 3️⃣ 【快速短路】
            if (totalCount == 0)
                return (new List<SysUserSummary>(), 0);

            // 4️⃣ 【分页 + 投影 + 子查询角色】—— 核心！
            var pagedUsers = await userQuery
                .OrderBy(u => u.CreatedAt, OrderByType.Desc) // 排序在用户表，安全高效
                .Select(u => new SysUserSummary
                {
                    Id = u.Id,
                    UserName = u.UserName,
                    NickName = u.NickName,
                    PhoneNumber = u.PhoneNumber,
                    Email = u.Email,
                    Sex = u.Sex,
                    Avatar = u.Avatar,
                    Status = u.Status,
                    Remark = u.Remark,
                    CreatedAt = u.CreatedAt,

                    // ✅【子查询嵌套角色】—— 无 Contains！无内存聚合！
                    Roles = SqlFunc.Subqueryable<SysUserRole>()
                        .InnerJoin<SysRole>((ur, r) => ur.RoleId == r.Id)
                        .Where((ur, r) => ur.UserId == u.Id) // 关联当前用户
                        .ToList((ur, r) => new RoleSummary
                        {
                            Id = r.Id,
                            Name = r.Name,
                            Key = r.Key
                        })
                })
                .ToPageListAsync(pageNumber, pageSize); // 分页执行

            // 5️⃣ 【返回结果】
            return (pagedUsers, totalCount);
        }

        /// <summary>
        /// 分页查询未删除的用户记录，支持名称、手机号码模糊搜索和状态精确筛选。
        /// </summary>
        /// <param name="pageNumber">页码，从 1 开始。</param>
        /// <param name="pageSize">每页记录数。</param>
        /// <param name="userName">用户名称，用于模糊搜索（可选）。</param>
        /// <param name="phoneNumber">用户手机号码，用于模糊搜索（可选）。</param>
        /// <param name="status">用户状态，用于精确匹配（可选）。</param>
        /// <returns>
        /// 元组包含分页结果：
        /// - Data：当前页的数据列表（可能为空）。
        /// - TotalCount：满足查询条件的总记录数。
        /// </returns>
        /// <remarks>
        /// 🔁 内部已升级为 JOIN + 聚合方式，返回 SysUserSummary（含角色信息）
        ///     但保持接口返回元组，兼容调用方。
        /// </remarks>
        public async Task<(List<SysUserSummary> Data, int TotalCount)> GetPagedSummaryUseJoinAsync(
            int pageNumber,
            int pageSize,
            string? userName = null,
            string? phoneNumber = null,
            string? status = null)
        {
            // 1️⃣ 【构建三表 JOIN 查询】
            var query = _db.Queryable<SysUser, SysUserRole, SysRole>((u, ur, r) => new object[]
            {
                JoinType.Left, ur.UserId == u.Id,
                JoinType.Left, ur.RoleId == r.Id
            })
            .Where((u, ur, r) => u.DeletedAt == null); // 未删除

            // 2️⃣ 【动态条件】
            if (!string.IsNullOrWhiteSpace(userName))
            {
                query = query.Where((u, ur, r) => u.UserName.Contains(userName));
            }

            if (!string.IsNullOrWhiteSpace(phoneNumber))
            {
                query = query.Where((u, ur, r) => (u.PhoneNumber ?? "").Contains(phoneNumber));
            }

            if (!string.IsNullOrWhiteSpace(status))
            {
                query = query.Where((u, ur, r) => u.Status == status);
            }

            // 3️⃣ 【排序】
            query = query.OrderBy((u, ur, r) => u.CreatedAt, OrderByType.Desc);

            // 4️⃣ 【分页数据】获取当前页的扁平化数据
            var pagedResults = await query
                .Select((u, ur, r) => new
                {
                    // 👇 用户字段
                    u.Id,
                    u.UserName,
                    u.NickName,
                    u.PhoneNumber,
                    u.Email,
                    u.Sex,
                    u.Avatar,
                    u.Status,
                    u.Remark,
                    u.CreatedAt,
                    // 👇 角色字段
                    RoleId = r == null ? null : (long?)r.Id, // 显式转为可空
                    Name = r == null ? null : r.Name,
                    Key = r == null ? null : r.Key
                })
                .ToPageListAsync(pageNumber, pageSize);

            // 5️⃣ 【总记录数】
            var totalCount = await query.CountAsync();

            // 6️⃣ 【空值检查】
            if (!pagedResults.Any())
            {
                return (new List<SysUserSummary>(), totalCount);
            }

            var grouped = pagedResults
                .GroupBy(x => x.Id).ToList();

            // 7️⃣ 【手动聚合】按用户ID分组，构建 SysUserSummary 列表
            var userSummaries = pagedResults
                .GroupBy(x => x.Id)
                .Select(g => new SysUserSummary
                {
                    Id = g.Key,
                    UserName = g.First().UserName,
                    NickName = g.First().NickName,
                    PhoneNumber = g.First().PhoneNumber,
                    Email = g.First().Email,
                    Sex = g.First().Sex,
                    Avatar = g.First().Avatar,
                    Status = g.First().Status,
                    Remark = g.First().Remark,
                    CreatedAt = g.First().CreatedAt,
                    Roles = g.Where(x => x.RoleId != null)  // 过滤掉因 LEFT JOIN 产生的 null 角色
                    .Select(x => new RoleSummary
                    {
                        Id = x.RoleId,
                        Name = x.Name,
                        Key = x.Key
                    }).ToList()
                })
                .ToList();

            // 8️⃣ 【返回元组】保持接口兼容
            return (userSummaries, totalCount);
        }

    }
}