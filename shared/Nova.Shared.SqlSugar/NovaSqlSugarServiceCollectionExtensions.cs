using System.Collections.Concurrent;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Nova.Shared.MultiTenancy;
using Nova.Shared.SqlSugar.Abstractions;
using SqlSugar;
using Volo.Abp.MultiTenancy;

namespace Nova.Shared.SqlSugar;

/// <summary>
/// Nova SqlSugar 基础设施注册扩展
/// <para>统一 ISqlSugarClient 注册、多租户切库、过滤器和自动建表</para>
/// </summary>
public static class NovaSqlSugarServiceCollectionExtensions
{
    private static readonly ConcurrentDictionary<string, byte> InitializedConnections = new();

    /// <summary>
    /// 注册多租户 SqlSugar 客户端和通用仓储
    /// </summary>
    public static IServiceCollection AddNovaSqlSugar<TLogCategory>(
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

        services.AddScoped(typeof(INovaRepository<>), typeof(NovaRepository<>));

        return services;
    }

    private static void EnsureTablesInitialized(
        ISqlSugarClient client,
        string connectionString,
        IReadOnlyCollection<Type> initTables)
    {
        if (initTables.Count == 0)
            return;

        var tableKey = $"{connectionString}::{string.Join('|', initTables.Select(t => t.FullName))}";
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
