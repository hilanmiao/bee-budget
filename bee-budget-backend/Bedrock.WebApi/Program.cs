using Bedrock.Application.Interfaces;
using Bedrock.Application.Services;
using Bedrock.Configuration;
using Bedrock.Infrastructure.Helpers;
using Bedrock.Infrastructure.Repositories;
using Bedrock.Infrastructure.Scheduling;
using Bedrock.Infrastructure.Security;
using Bedrock.WebApi.Filters;
using Bedrock.WebApi.Middleware;
using Hangfire;
using Hangfire.MySql;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.FileProviders;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;
using Serilog;
using Serilog.Exceptions;
using SqlSugar;
using StackExchange.Redis;
using System.Reflection;
using System.Security.Claims;
using System.Text;

#region 初始化日志系统
// 创建临时 Bootstrap Logger（用于记录启动过程）
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File("logs/app-.txt", rollingInterval: RollingInterval.Day)
    .CreateBootstrapLogger();

#endregion

var builder = WebApplication.CreateBuilder(args);

#region 配置 Web 服务器
var port = builder.Configuration.GetValue<int>("AppSettings:HttpPort", 9999); // 默认端口为 9999

// 配置 Kestrel
builder.WebHost.ConfigureKestrel(serverOptions =>
{
    serverOptions.ListenAnyIP(port); // 使用配置中的端口
    serverOptions.Limits.MaxRequestBodySize = 1024 * 1024 * 1024; // 设置最大请求体大小为 1024 MB
});
#endregion

#region 绑定数据库、Redis 等配置
// 绑定CORS配置
builder.Services.Configure<CorsConfig>(builder.Configuration.GetSection("Cors"));
var corsConfig = builder.Configuration.GetSection("Cors").Get<CorsConfig>();
if (corsConfig == null)
{
    throw new InvalidOperationException("CORS configuration is not configured.");
}

// 绑定数据库配置
builder.Services.Configure<DatabaseConfig>(builder.Configuration.GetSection("Database"));
var databaseConfig = builder.Configuration.GetSection("Database").Get<DatabaseConfig>();
if (string.IsNullOrWhiteSpace(databaseConfig?.DefaultConnection))
{
    throw new InvalidOperationException("Database connection string is not configured.");
}

// 绑定 Redis 配置
builder.Services.Configure<RedisConfig>(builder.Configuration.GetSection("Redis"));
var redisConfig = builder.Configuration.GetSection("Redis").Get<RedisConfig>();
if (string.IsNullOrWhiteSpace(redisConfig?.Configuration))
{
    throw new InvalidOperationException("Redis configuration is not configured.");
}

// 绑定 Hangfire 配置
builder.Services.Configure<HangfireConfig>(builder.Configuration.GetSection("Hangfire"));
var hangfireConfig = builder.Configuration.GetSection("Hangfire").Get<HangfireConfig>();
if (hangfireConfig == null)
{
    throw new InvalidOperationException("Hangfire configuration is not configured.");
}

// 绑定健康检查配置
builder.Services.Configure<HealthChecksConfig>(builder.Configuration.GetSection("HealthChecks"));
var healthChecksConfig = builder.Configuration.GetSection("HealthChecks").Get<HealthChecksConfig>();
if (healthChecksConfig == null)
{
    throw new InvalidOperationException("HealthChecks configuration is not configured.");
}


// 绑定 TokenClean 配置
builder.Services.Configure<TokenCleanupConfig>(builder.Configuration.GetSection("TokenClean"));

// 绑定 JWT 配置
builder.Services.Configure<JwtConfig>(builder.Configuration.GetSection("Jwt"));
var jwtConfig = builder.Configuration.GetSection("Jwt").Get<JwtConfig>();
if (string.IsNullOrWhiteSpace(jwtConfig?.Key) || Encoding.UTF8.GetBytes(jwtConfig.Key).Length * 8 < 256) // 确保密钥长度足够
{
    throw new InvalidOperationException("JWT Key is invalid or too short.");
}

#endregion

#region 注册依赖注入服务

// 注册 IHttpContextAccessor，用于在非请求上下文中安全访问 HttpContext。
builder.Services.AddHttpContextAccessor();

// 注册 RabbitMQ Log Publisher Service
// builder.Services.AddSingleton<RabbitMqLogPublisher>();
// 注册 RabbitMQ Log Consumer Service
// builder.Services.AddHostedService<RabbitMqLogConsumerService>();

// 注册 SqlSugar 数据库上下文
// 将字符串转换为 SqlSugar.DbType 枚举
builder.Services.AddScoped<ISqlSugarClient>(s =>
{
    var dbType = databaseConfig.DefaultDbType.ToLowerInvariant() switch
    {
        "sqlite" => DbType.Sqlite,
        "sqlserver" => DbType.SqlServer,
        "mysql" => DbType.MySql,
        "postgresql" => DbType.PostgreSQL,
        "oracle" => DbType.Oracle,
        _ => throw new ArgumentException($"不支持的数据库类型，请检查配置。")
    };

    var db = new SqlSugarClient(new ConnectionConfig
    {
        ConnectionString = databaseConfig.DefaultConnection, // 数据库连接字符串
        DbType = dbType,  // 根据您的数据库类型进行调整
        IsAutoCloseConnection = true, // 是否在每次数据库操作完成后自动关闭底层的数据库连接，推荐开启以提高性能
        InitKeyType = InitKeyType.Attribute, // 如何识别实体类中的主键（Primary Key）和自增列（Identity Column），仅通过 C# 实体类上的特性（Attribute）来识别，提供启动性能
        ConfigureExternalServices = new ConfigureExternalServices // 驼峰转下划线
        {
            EntityService = (property, column) => //处理列名
            {
                // 排除DTO类，通过命名空间过滤
                if (property.DeclaringType?.Namespace?.Contains("Entities") == true)
                {
                    column.DbColumnName = UtilMethods.ToUnderLine(column.DbColumnName);//驼峰转下划线方法
                }
            },
            EntityNameService = (type, table) => //处理表名
            {
                // 排除DTO类，通过命名空间判断
                if (type.Namespace?.Contains("Entities") == true)
                {
                    table.DbTableName = UtilMethods.ToUnderLine(table.DbTableName);//驼峰转下划线方法
                }
            }
        }
    });

    db.Aop.OnLogExecuting = (sql, pars) =>
    {
        //Log.Information("SQL 执行中: {Sql}, 参数: {@Params}", sql, pars);

        // 输出sql,查看执行sql 性能无影响
        ConsoleHelper.Success($"{DateTime.UtcNow} SQL 执行中: {sql}");

        // 获取原生SQL推荐 5.1.4.63  性能OK
        //ConsoleHelper.Success(UtilMethods.GetNativeSql(sql, pars));

        // 获取无参数化SQL 对性能有影响，特别大的SQL参数多的，调试使用
        //ConsoleHelper.Success(UtilMethods.GetSqlString(DbType.MySql, sql, pars));
    };

    db.Aop.OnLogExecuted = (sql, pars) =>
    {
        //Log.Information("SQL 执行完毕: {Sql}, 参数: {@Params}", sql, pars);
        //ConsoleHelper.Success($"{DateTime.UtcNow} SQL 执行完毕: {sql}");
    };

    return db;
});

// 注册 Redis 连接管理器
builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
{
    var options = ConfigurationOptions.Parse(redisConfig.Configuration);
    options.ClientName = redisConfig.InstanceName + "App"; // 设置客户端名称，方便监控
    options.ReconnectRetryPolicy = new ExponentialRetry(5000); // 设置重连策略
    return ConnectionMultiplexer.Connect(options);
});

//// 注册分布式缓存服务（废弃，统一使用 Redis 连接管理器）
//builder.Services.AddStackExchangeRedisCache(options =>
//{
//    options.Configuration = redisConfig.Configuration;
//});

// ===============注册 Hangfire 服务===================
// 添加 Hangfire 服务
builder.Services.AddHangfire(config =>
{
    config.UseSimpleAssemblyNameTypeSerializer()
          .UseRecommendedSerializerSettings()
          .UseStorage(new MySqlStorage(
              hangfireConfig.DatabaseConnection,
              new MySqlStorageOptions
              {
                  // MySQL 特有配置
                  TransactionIsolationLevel = System.Transactions.IsolationLevel.ReadCommitted,
                  QueuePollInterval = TimeSpan.FromSeconds(15),
                  JobExpirationCheckInterval = TimeSpan.FromHours(1),
                  CountersAggregateInterval = TimeSpan.FromMinutes(5),
                  PrepareSchemaIfNecessary = true, // 首次自动建表
                  DashboardJobListLimit = 5000,
                  TablesPrefix = "hangfire_" // 表名前缀，如 Hangfire_Job, Hangfire_State
              }));
});
// 注册 Hangfire Server（负责执行后台任务）
builder.Services.AddHangfireServer(options =>
{
    options.WorkerCount = Environment.ProcessorCount; // 根据 CPU 核心数设置并发
    options.Queues = new[] { "critical", "default" }; // 可定义多个队列，critical：紧急任务，default：普通任务

    // 如果有长任务（> 15秒），加这一行
    options.ShutdownTimeout = TimeSpan.FromMinutes(30);

    // 如果有用到“每分钟/每10秒”这类定时任务，加这一行，如果都是“每天/每小时”级别，则不需要加。默认是 15 秒
    //options.SchedulePollingInterval = TimeSpan.FromSeconds(5);
});
// ==================================


// ========== 健康检查服务 ==========
builder.Services.AddHealthChecks()
    .AddCheck("self", () => HealthCheckResult.Healthy("API 正常运行"))
    .AddMySql(
        connectionString: healthChecksConfig.DatabaseConnection,
        name: "mysql",
        failureStatus: HealthStatus.Unhealthy, // 关键依赖故障
        tags: new[] { "db", "mysql" })
    .AddRedis(
        redisConnectionString: redisConfig.Configuration,
        name: "redis",
        failureStatus: HealthStatus.Degraded, // 部分功能降级
        tags: new[] { "cache", "redis" });

// ========== 健康检查 UI 服务 ==========
builder.Services.AddHealthChecksUI(setup =>
{
    setup.AddHealthCheckEndpoint("main-api", healthChecksConfig.HealthApiUrl); // 指向健康检查终结点
    setup.SetEvaluationTimeInSeconds(15);               // 每15秒检查一次
    setup.SetApiMaxActiveRequests(2);                   // UI API 最大并发请求数
})
.AddMySqlStorage(healthChecksConfig.DatabaseConnection); // 使用 MySQL 存储健康检查数据

// 注册仓储和服务
builder.Services.AddScoped<IDemo1Repository, Demo1Repository>();
builder.Services.AddScoped<IDemo1Service, Demo1Service>();
builder.Services.AddScoped<IDemo2Repository, Demo2Repository>();
builder.Services.AddScoped<IDemo2Service, Demo2Service>();

builder.Services.AddScoped<IFileService, FileService>();

builder.Services.AddScoped<ICaptchaService, CaptchaService>();

builder.Services.AddScoped<ISysMenuRepository, SysMenuRepository>();
builder.Services.AddScoped<ISysMenuService, SysMenuService>();

builder.Services.AddScoped<ISysRoleRepository, SysRoleRepository>();
builder.Services.AddScoped<ISysRoleService, SysRoleService>();
builder.Services.AddScoped<ISysRoleMenuRepository, SysRoleMenuRepository>();

builder.Services.AddScoped<ISysUserRepository, SysUserRepository>();
builder.Services.AddScoped<ISysUserService, SysUserService>();
builder.Services.AddScoped<ISysUserRoleRepository, SysUserRoleRepository>();

builder.Services.AddScoped<IAuthRepository, AuthRepository>();
builder.Services.AddScoped<IAuthService, AuthService>();

builder.Services.AddScoped<IPasswordHasher, PasswordHasher>();

builder.Services.AddScoped<ISysDictCategoryRepository, SysDictCategoryRepository>();
builder.Services.AddScoped<ISysDictCategoryService, SysDictCategoryService>();
builder.Services.AddScoped<ISysDictItemRepository, SysDictItemRepository>();
builder.Services.AddScoped<ISysDictItemService, SysDictItemService>();

builder.Services.AddScoped<ILedgerRepository, LedgerRepository>();
builder.Services.AddScoped<ILedgerService, LedgerService>();
builder.Services.AddScoped<ITransactionCategoryRepository, TransactionCategoryRepository>();
builder.Services.AddScoped<ITransactionCategoryService, TransactionCategoryService>();
builder.Services.AddScoped<ITransactionRepository, TransactionRepository>();
builder.Services.AddScoped<ITransactionService, TransactionService>();

builder.Services.AddScoped<IFileStorageCenterRepository, FileStorageCenterRepository>();
builder.Services.AddScoped<IFileStorageCenterService, FileStorageCenterService>();

builder.Services.AddScoped<IAppRepository, AppRepository>();
builder.Services.AddScoped<IAppService, AppService>();
builder.Services.AddScoped<IAppVersionRepository, AppVersionRepository>();
builder.Services.AddScoped<IAppVersionService, AppVersionService>();

// 注册测试任务服务
builder.Services.AddScoped<ITestTaskService, TestTaskService>();

// 注册TokenCleanup服务
builder.Services.AddScoped<ITokenCleanupService, TokenCleanupService>();
// 注册TokenCleanup后台服务
//builder.Services.AddHostedService<TokenCleanupBackgroundService>(); // 使用更高级的hangfire替代

// 注册Hangfire任务注册后台服务
builder.Services.AddHostedService<HangfireJobRegistrationBackgroundService>();

#endregion

#region 添加身份验证和授权
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtConfig.Issuer,
        ValidAudience = jwtConfig.Audience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtConfig.Key)),
        ClockSkew = TimeSpan.Zero
    };

    // 添加事件处理
    options.Events = new JwtBearerEvents
    {
        OnChallenge = async context =>
        {
            // 阻止默认的 WWW-Authenticate 响应
            context.HandleResponse();

            // 设置统一响应格式
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            context.Response.ContentType = "application/json";

            var response = new
            {
                code = 401,
                message = "令牌校验失败，请重新登录。",
                data = (object?)null
            };

            await context.Response.WriteAsJsonAsync(response);
        },

        OnAuthenticationFailed = context =>
        {
            Log.Warning(
                context.Exception,
                "JWT 认证失败 - 客户端IP: {ClientIp}, UserAgent: {UserAgent}, Error: {AuthError}",
                context.HttpContext.Connection.RemoteIpAddress?.ToString(),
                context.HttpContext.Request.Headers["User-Agent"].ToString(),
                context.Exception.Message
            );
            return Task.CompletedTask;
        },

        OnTokenValidated = async context =>
        {
            try
            {
                // 1️⃣ 从已验证的 Token 中提取 userId
                var userIdClaim = context.Principal?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrWhiteSpace(userIdClaim) || !long.TryParse(userIdClaim, out long userId))
                {
                    throw new SecurityTokenException("Token 中缺少有效的用户标识（NameIdentifier）声明。");
                }

                // 正确提取原始完整 Token
                var authHeader = context.HttpContext.Request.Headers["Authorization"].ToString();
                if (string.IsNullOrWhiteSpace(authHeader) || !authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                {
                    throw new SecurityTokenException("请求头中缺少有效的 Bearer Token。");
                }

                var originalToken = authHeader["Bearer ".Length..].Trim();

                // 3️⃣ 从 DI 获取服务（注意：不能直接注入，要从 context.HttpContext.RequestServices 获取）
                var authService = context.HttpContext.RequestServices.GetRequiredService<IAuthService>();
                var sysUserService = context.HttpContext.RequestServices.GetRequiredService<ISysUserService>();

                // 4️⃣ 验证 Token 是否被主动注销（黑名单/刷新机制）
                if (!await authService.ValidateAccessTokenAsync(userId, originalToken))
                {
                    throw new SecurityTokenException("无效或已过期的访问令牌。");
                }

                // TODO: 每次请求都会查询一次用户，如果有上万的注册用户，且日活比较高，可以考虑缓存用户信息。（管理系统没有必要，用户量很少）
                // 5️⃣ 验证用户是否存在、是否被禁用
                var user = await sysUserService.GetAsync(userId);
                if (user == null)
                {
                    throw new SecurityTokenException("用户不存在或已被删除。");
                }

                // 1. 把用户信息添加到 ClaimsPrincipal（标准认证身份）
                var identity = (ClaimsIdentity)(context.Principal?.Identity ?? throw new InvalidOperationException("认证上下文异常：缺失 ClaimsIdentity。"));
                identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()));
                identity.AddClaim(new Claim("UserName", user.UserName));
                // 可选：如果你想在 [Authorize] 中使用角色，可以在这里添加角色声明
                // 例如：如果 user.Roles = "Admin,User"，你可以：
                // var roles = user.Roles?.Split(',') ?? Array.Empty<string>();
                // var identity = (ClaimsIdentity)context.Principal.Identity;
                // foreach (var role in roles)
                // {
                //     identity.AddClaim(new Claim(ClaimTypes.Role, role.Trim()));
                // }
                // 2. （可选）同时存入 Items 供业务层快速访问
                context.HttpContext.Items["User"] = user;
            }
            catch (Exception ex)
            {
                // 任何业务校验失败，都标记为“验证失败”
                context.Fail(ex); // 关键！触发 OnChallenge
            }
        }
    };
});

// 设置默认授权策略
builder.Services.AddAuthorization(options =>
{
    //对所有未显式配置授权策略的端点，强制要求用户认证。
    //options.FallbackPolicy = new AuthorizationPolicyBuilder()
    //    .RequireAuthenticatedUser()
    //    .Build();
    // 改为只在需要保护的 Controller 或 Action 上显示添加 [Authorize]
});
#endregion

#region 添加控制器和其他中间件支持

// 添加控制器支持
builder.Services.AddControllers(options =>
{
    options.Filters.Add(typeof(ApiExceptionFilter));
}).AddJsonOptions(options =>
{
    // 关键配置：告诉序列化器，遇到 DateTime 就当成 UTC 处理，并自动加上 'Z'
    options.JsonSerializerOptions.Converters.Add(new DateTimeUtcConverter());
});

// 添加 OpenAPI 支持
builder.Services.AddOpenApi();

// 将 Serilog 接入 ASP.NET Core 日志系统，用正式配置替换 Bootstrap Logger。默认启用全链路诊断日志（包括框架层）
builder.Host.UseSerilog((context, services, configuration) => configuration
    .ReadFrom.Configuration(context.Configuration)   // 从 appsettings.json 读取配置
    .ReadFrom.Services(services)                    // 从 DI 容器中读取配置
    .Enrich.FromLogContext()                     // 支持 LogContext.PushProperty
    .Enrich.WithExceptionDetails()               // 自动展开异常的公共属性为 JSON 字段
);
#endregion

#region 启动应用程序
var app = builder.Build(); // DI 容器已准备好

try
{
    ConsoleHelper.Success("正在启动应用...");
    Log.Information("正在启动应用...");

    // 注册全局异常处理中间件（中间件的执行顺序遵循“先进后出”（First In, Last Out）的栈式模型。想象成洋葱圈，需要放最前面才能捕获全局异常！）
    app.UseMiddleware<GlobalExceptionHandlingMiddleware>();
    // 使用自定义的 JWT 中间件（！！！完全绕过了框架设计，导致 [AllowAnonymous]、[Authorize(Roles="")]、策略授权等全部失效）
    //app.UseMiddleware<JwtMiddleware>();

    // 启用静态文件服务
    app.UseStaticFiles(new StaticFileOptions
    {
        FileProvider = new PhysicalFileProvider(Path.Combine(app.Environment.WebRootPath, "uploads")),
        RequestPath = "/uploads",
        ContentTypeProvider = new FileExtensionContentTypeProvider
        {
            Mappings =
            {
                { ".gltf", "model/gltf+json" },
                { ".glb", "model/gltf-binary" },
                { ".dds", "image/vnd.ms-dds" },
                { ".apk", "application/vnd.android.package-archive" },
                { ".wgt", "application/widget" }
            }
        }
    });

    // 使用 CORS 策略
    app.UseCors(policy =>
    {
        // 判断是否启用了“允许所有来源”
        if (corsConfig.AllowAllOrigins)
        {
            // 允许任何源（注意：不能与 AllowCredentials 同时使用）
            policy.AllowAnyOrigin();
        }
        // 如果未允许所有来源，但配置了具体的允许列表
        else if (corsConfig.AllowedOrigins?.Length > 0)
        {
            // 仅允许配置文件中列出的来源
            policy.WithOrigins(corsConfig.AllowedOrigins);
        }
        else
        {
            // 安全兜底：如果既没开 AllowAllOrigins，又没配 AllowedOrigins
            // 此处可以选择拒绝所有 CORS 请求（不调用任何 Allow* 方法）
            // 但为了调试方便，这里默认允许所有（你可根据需要修改）
            policy.AllowAnyOrigin();
        }

        // 允许所有 HTTP 方法（GET, POST, PUT, DELETE 等）
        policy.AllowAnyMethod();

        // 允许所有请求头（如 Authorization, Content-Type 等）
        policy.AllowAnyHeader();

        // ⚠️ 注意：如果你需要支持携带凭据（如 cookies、HTTP 认证 或 使用 fetch 的 credentials: 'include'），
        // 请不要使用 AllowAnyOrigin()，而应指定具体域名，并调用：
        // policy.AllowCredentials();
        // 否则浏览器会因安全策略拒绝请求。
        //
        // 注意："Authorization" 请求头（如 Bearer Token）若由前端手动设置，
        // 并不属于 CORS 凭据，无需 AllowCredentials()。
    });

    // 先进行身份验证
    app.UseAuthentication();

    // 再进行授权决策
    app.UseAuthorization();

    // 开发环境中启用 Scalar UI
    if (app.Environment.IsDevelopment())
    {
        // 映射 OpenAPI 文档（默认地址：https://localhost:xxxx/openapi.json）
        app.MapOpenApi();
        // 映射 Scalar UI 默认访问地址: https://localhost:xxxx/scalar/v1
        app.MapScalarApiReference();
    }

    // 映射 Hangfire Dashboard（默认地址：https://localhost:xxxx/hangfire）
    app.UseHangfireDashboard("/hangfire", new DashboardOptions
    {
        StatsPollingInterval = 5000, // 每 5 秒刷新一次统计数据
        Authorization = new[] { new Hangfire.Dashboard.LocalRequestsOnlyAuthorizationFilter() } // 仅允许本地访问
    });

    // 健康检查终结点（返回结构化 JSON，供 UI 使用）
    app.MapHealthChecks("/health", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
    {
        ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse, // 关键：支持 UI 格式
        ResultStatusCodes =
    {
        [HealthStatus.Healthy] = StatusCodes.Status200OK,
        [HealthStatus.Degraded] = StatusCodes.Status200OK,
        [HealthStatus.Unhealthy] = StatusCodes.Status503ServiceUnavailable
    }
    });

    // 健康检查可视化 UI 页面
    app.MapHealthChecksUI(options =>
    {
        options.UIPath = "/healthchecks-ui";     // 访问路径  http://localhost:xxxx/healthchecks-ui
        options.ApiPath = "/healthchecks-ui-api";      // UI 后端 API 路径
    })
    .RequireHost("localhost", "127.0.0.1"); // 访问控制（仅本地）

    // 映射控制器路由
    app.MapControllers();

    ConsoleHelper.Success($"应用启动成功，监听端口 {port}");
    Log.Information("应用启动成功，监听端口 {Port}", port);

    app.Run();
}
catch (Exception ex)
{
    ConsoleHelper.Error($"启动应用时发生致命错误: {ex.Message}");
    Log.Fatal(ex, "启动应用时发生致命错误");
    throw;
}
#endregion