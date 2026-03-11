using Microsoft.Extensions.Caching.Memory;
using SqlSugar;
using System.Data.Common;

namespace Nova.Gateway.Middlewares;

/// <summary>
/// 租户解析中间件
/// <para>在网关层解析租户标识，并注入到请求头中传递给下游服务</para>
/// <para>支持两种解析方式：</para>
/// <para>1. 子域名解析：tenant1.yourapp.com → 查询租户表获取TenantId</para>
/// <para>2. Header解析：X-Tenant-Id请求头</para>
/// </summary>
public class TenantResolverMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IConfiguration _configuration;
    private readonly IMemoryCache _memoryCache;

    public TenantResolverMiddleware(
        RequestDelegate next,
        IConfiguration configuration,
        IMemoryCache memoryCache)
    {
        _next = next;
        _configuration = configuration;
        _memoryCache = memoryCache;
    }

    /// <summary>
    /// 中间件执行方法
    /// <para>解析租户ID并注入到__tenant请求头，ABP多租户中间件会自动读取</para>
    /// </summary>
    public async Task InvokeAsync(HttpContext context)
    {
        var tenantId = ResolveTenantId(context);
        if (tenantId.HasValue)
        {
            // 注入租户ID到请求头，下游ABP服务会自动识别
            context.Request.Headers["__tenant"] = tenantId.ToString();
        }
        await _next(context);
    }

    /// <summary>
    /// 解析租户ID
    /// <para>优先级：子域名 > Header</para>
    /// </summary>
    private Guid? ResolveTenantId(HttpContext context)
    {
        var tenantCode = ResolveTenantCode(context);
        if (!string.IsNullOrWhiteSpace(tenantCode))
            return FindTenantIdByCode(tenantCode);

        // 从请求头获取租户ID（适用于移动端/API调用）
        if (context.Request.Headers.TryGetValue("X-Tenant-Id", out var tenantIdStr))
        {
            if (Guid.TryParse(tenantIdStr, out var tid)) return tid;
        }

        return null;
    }

    private static string? ResolveTenantCode(HttpContext context)
    {
        if (context.Request.Headers.TryGetValue("X-Tenant-Code", out var tenantCode) &&
            !string.IsNullOrWhiteSpace(tenantCode))
            return tenantCode.ToString().Trim();

        var host = context.Request.Host.Host;
        var parts = host.Split('.', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length < 2)
            return null;

        var subdomain = parts[0];
        if (subdomain.Equals("www", StringComparison.OrdinalIgnoreCase) ||
            subdomain.Equals("api", StringComparison.OrdinalIgnoreCase) ||
            subdomain.Equals("localhost", StringComparison.OrdinalIgnoreCase))
            return null;

        return subdomain;
    }

    private Guid? FindTenantIdByCode(string tenantCode)
    {
        return _memoryCache.GetOrCreate($"tenant:id:{tenantCode.ToLowerInvariant()}", entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10);

            var connectionString = _configuration.GetConnectionString("Default");
            if (string.IsNullOrWhiteSpace(connectionString))
                return null;

            EnsureMySqlDatabaseCreated(connectionString);

            var db = new SqlSugarClient(new ConnectionConfig
            {
                DbType = DbType.MySql,
                ConnectionString = connectionString,
                IsAutoCloseConnection = true
            });

            const string sql = """
                SELECT id
                FROM sys_tenant
                WHERE code = @code AND is_deleted = 0
                LIMIT 1
                """;

            return db.Ado.SqlQuerySingle<Guid?>(sql, new SugarParameter("@code", tenantCode));
        });
    }

    private static void EnsureMySqlDatabaseCreated(string connectionString)
    {
        var builder = new DbConnectionStringBuilder
        {
            ConnectionString = connectionString
        };

        if (!builder.TryGetValue("Database", out var databaseObj) &&
            !builder.TryGetValue("Initial Catalog", out databaseObj))
            return;

        var databaseName = databaseObj?.ToString();
        if (string.IsNullOrWhiteSpace(databaseName))
            return;

        builder.Remove("Database");
        builder.Remove("Initial Catalog");

        var adminDb = new SqlSugarClient(new ConnectionConfig
        {
            DbType = DbType.MySql,
            ConnectionString = builder.ConnectionString,
            IsAutoCloseConnection = true
        });

        var escapedDatabaseName = databaseName.Replace("`", "``", StringComparison.Ordinal);
        adminDb.Ado.ExecuteCommand(
            $"CREATE DATABASE IF NOT EXISTS `{escapedDatabaseName}` CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;");
    }
}

/// <summary>
/// 租户解析中间件扩展方法
/// </summary>
public static class TenantResolverMiddlewareExtensions
{
    /// <summary>
    /// 使用租户解析中间件
    /// </summary>
    public static IApplicationBuilder UseTenantResolver(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<TenantResolverMiddleware>();
    }
}
