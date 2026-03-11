using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Nova.Shared.Hosting;
using Nova.Shared.Hosting.Extensions;
using Nova.Shared.MultiTenancy;
using Nova.TenantService.Application;
using Nova.TenantService.Core.Entities;
using SqlSugar;
using System.Data.Common;
using Volo.Abp;
using Volo.Abp.AspNetCore.Mvc;
using Volo.Abp.Modularity;
using Volo.Abp.MultiTenancy;

namespace Nova.TenantService.Host;

[DependsOn(
    typeof(TenantServiceApplicationModule),
    typeof(NovaSharedHostingModule)
)]
public class TenantServiceHostModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        var configuration = context.Services.GetConfiguration();

        Configure<AbpAspNetCoreMvcOptions>(options =>
        {
            options.ConventionalControllers.Create(
                typeof(TenantServiceApplicationModule).Assembly,
                opts => opts.RootPath = "tenant");
        });

        context.Services.AddNovaSwagger("Nova Tenant Service API");
        context.Services.AddNovaJwtAuthentication(configuration);

        ConfigureSqlSugar(context, configuration);
    }

    private void ConfigureSqlSugar(ServiceConfigurationContext context, IConfiguration configuration)
    {
        context.Services.AddSingleton<ISqlSugarClient>(sp =>
        {
            return new SqlSugarClient(new ConnectionConfig
            {
                DbType = DbType.MySql,
                ConnectionString = configuration.GetConnectionString("Default"),
                IsAutoCloseConnection = true,
                InitKeyType = InitKeyType.Attribute
            }, db =>
            {
                db.QueryFilter.AddTableFilter<TenantEntity>(e => !e.IsDeleted);

                db.Aop.OnLogExecuting = (sql, _) =>
                {
                    var logger = sp.GetService<ILogger<TenantServiceHostModule>>();
                    logger?.LogDebug("SQL: {Sql}", sql);
                };
            });
        });
    }

    public override void OnApplicationInitialization(ApplicationInitializationContext context)
    {
        InitializeDatabase(context.ServiceProvider);

        var app = context.GetApplicationBuilder();
        app.UseRouting();
        app.UseAuthentication();
        app.UseAuthorization();
        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();
        });
        app.UseSwagger();
        app.UseSwaggerUI(options => options.SwaggerEndpoint("/swagger/v1/swagger.json", "Tenant Service API"));
    }

    private static void InitializeDatabase(IServiceProvider serviceProvider)
    {
        var configuration = serviceProvider.GetRequiredService<IConfiguration>();
        var connectionString = configuration.GetConnectionString("Default");

        // 默认采用单库共享模式：租户表与业务表共用一个库，独立库仅作为后续扩展能力保留。
        EnsureMySqlDatabaseCreated(connectionString);

        var db = serviceProvider.GetRequiredService<ISqlSugarClient>();
        db.CodeFirst.InitTables<TenantEntity>();

        if (!db.Queryable<TenantEntity>().Any(x => x.Code == NovaSeedConstants.DefaultTenantCode))
        {
            db.Insertable(new TenantEntity
            {
                Id = NovaSeedConstants.DefaultTenantId,
                Name = NovaSeedConstants.DefaultTenantName,
                Code = NovaSeedConstants.DefaultTenantCode,
                Status = TenantStatus.Active,
                Plan = TenantPlan.Free,
                CreateTime = DateTime.UtcNow
            }).ExecuteCommand();
        }
    }

    private static void EnsureMySqlDatabaseCreated(string? connectionString)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
            return;

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
