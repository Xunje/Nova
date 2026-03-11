using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using SqlSugar;
using Volo.Abp.DependencyInjection;
using Volo.Abp.MultiTenancy;

namespace Nova.Shared.MultiTenancy;

/// <summary>
/// 租户数据库连接解析器
/// <para>根据当前租户上下文解析数据库连接字符串</para>
/// <para>支持：共享数据库（默认）和VIP租户独立数据库</para>
/// </summary>
public class TenantConnectionResolver : ITransientDependency
{
    private const string SharedDatabaseMarker = "__shared__";
    private readonly ICurrentTenant _currentTenant;
    private readonly IConfiguration _configuration;
    private readonly IDistributedCache _cache;

    public TenantConnectionResolver(
        ICurrentTenant currentTenant,
        IConfiguration configuration,
        IDistributedCache cache)
    {
        _currentTenant = currentTenant;
        _configuration = configuration;
        _cache = cache;
    }

    /// <summary>
    /// 解析当前租户的数据库连接字符串
    /// </summary>
    /// <returns>数据库连接字符串</returns>
    public async Task<string> ResolveAsync()
    {
        var defaultConnection = _configuration.GetConnectionString("Default")!;
        var tenantId = _currentTenant.Id;

        // 超级租户/无租户上下文时，使用默认连接
        if (tenantId == null)
            return defaultConnection;

        // 尝试从缓存获取租户专属连接字符串（VIP租户独立数据库）
        var cacheKey = $"tenant:conn:{tenantId}";
        var cached = await _cache.GetStringAsync(cacheKey);
        if (!string.IsNullOrEmpty(cached))
            return cached == SharedDatabaseMarker ? defaultConnection : cached;

        var tenantConnection = ResolveFromSharedDatabase(defaultConnection, tenantId.Value);
        var cacheValue = string.IsNullOrWhiteSpace(tenantConnection)
            ? SharedDatabaseMarker
            : tenantConnection;

        await _cache.SetStringAsync(cacheKey, cacheValue, new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10)
        });

        return string.IsNullOrWhiteSpace(tenantConnection)
            ? defaultConnection
            : tenantConnection;
    }

    private static string? ResolveFromSharedDatabase(string defaultConnection, Guid tenantId)
    {
        try
        {
            MySqlDatabaseCreator.EnsureCreated(defaultConnection);

            var db = new SqlSugarClient(new ConnectionConfig
            {
                DbType = DbType.MySql,
                ConnectionString = defaultConnection,
                IsAutoCloseConnection = true,
                InitKeyType = InitKeyType.Attribute
            });

            const string sql = """
                SELECT connection_string
                FROM sys_tenant
                WHERE id = @tenantId AND is_deleted = 0
                LIMIT 1
                """;

            return db.Ado.SqlQuerySingle<string?>(sql, new SugarParameter("@tenantId", tenantId));
        }
        catch
        {
            // 在共享库或租户表尚未初始化时，安全回退到默认共享连接。
            return null;
        }
    }
}
