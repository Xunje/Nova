using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Nova.Shared.MultiTenancy;
using SqlSugar;
using System.Collections.Concurrent;
using Volo.Abp.MultiTenancy;

namespace Nova.Shared.Hosting.Extensions;

/// <summary>
/// SqlSugar 注册扩展
/// <para>统一微服务模式下的连接解析、过滤器和 SQL 日志配置</para>
/// </summary>
public static class SqlSugarExtensions
{
    private static readonly ConcurrentDictionary<string, byte> InitializedConnections = new();

    public static IServiceCollection AddNovaMultiTenantSqlSugar<TLogCategory>(
        this IServiceCollection services,
        params Type[] initTables)
    {
        services.AddScoped<ISqlSugarClient>(sp =>
        {
            var currentTenant = sp.GetRequiredService<ICurrentTenant>();
            var connectionResolver = sp.GetRequiredService<TenantConnectionResolver>();
            var logger = sp.GetService<ILogger<TLogCategory>>();
            var connectionString = connectionResolver.ResolveAsync().GetAwaiter().GetResult();

            var client = new SqlSugarClient(new ConnectionConfig
            {
                DbType = DbType.MySql,
                ConnectionString = connectionString,
                IsAutoCloseConnection = true,
                InitKeyType = InitKeyType.Attribute
            }, db =>
            {
                db.QueryFilter.AddTableFilter<EntityBase>(entity => entity.TenantId == currentTenant.Id);
                db.QueryFilter.AddTableFilter<EntityBase>(entity => !entity.IsDeleted);

                db.Aop.OnLogExecuting = (sql, _) =>
                {
                    logger?.LogDebug("SQL: {Sql}", sql);
                };
            });

            EnsureTablesInitialized(client, connectionString, initTables);
            return client;
        });

        return services;
    }

    private static void EnsureTablesInitialized(
        ISqlSugarClient client,
        string connectionString,
        IReadOnlyCollection<Type> initTables)
    {
        if (initTables.Count == 0)
            return;

        var tableKey = $"{connectionString}::{string.Join('|', initTables.Select(type => type.FullName))}";
        if (!InitializedConnections.TryAdd(tableKey, 0))
            return;

        try
        {
            client.CodeFirst.InitTables(initTables.ToArray());
        }
        catch
        {
            InitializedConnections.TryRemove(tableKey, out _);
            throw;
        }
    }
}
