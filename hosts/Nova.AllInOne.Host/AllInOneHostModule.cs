using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Nova.BusinessService.Application;
using Nova.IdentityService.Application;
using Nova.IdentityService.Application.Seeds;
using Nova.IdentityService.Core.Entities;
using Nova.SystemService.Application.Seeds;
using Nova.SystemService.Core.Entities;
using Nova.Shared.Hosting;
using Nova.Shared.Hosting.BackgroundJobs;
using Nova.Shared.Hosting.Extensions;
using Nova.Shared.MultiTenancy;
using Nova.Shared.SqlSugar;
using Nova.Shared.SqlSugar.Abstractions;
using Nova.TenantService.Application;
using Nova.SystemService.Core.Repositories;
using Nova.SystemService.Host.Repositories;
using Nova.TenantService.Core.Entities;
using Nova.SystemService.Application;
using Nova.SystemService.Core.Consts;
using Nova.SystemService.Core.Security;
using Nova.SystemService.Host;
using SqlSugar;
using Volo.Abp;
using Volo.Abp.AspNetCore.Mvc;
using Volo.Abp.Modularity;

namespace Nova.AllInOne.Host;

[DependsOn(
    typeof(NovaSharedHostingModule),
    typeof(NovaBackgroundWorkersHangfireModule),
    typeof(TenantServiceApplicationModule),
    typeof(IdentityServiceApplicationModule),
    typeof(SystemServiceApplicationModule),
    typeof(BusinessServiceApplicationModule)
)]
public class AllInOneHostModule : AbpModule
{
    private static readonly (string Name, string Title, string NamespacePrefix)[] SwaggerDocs =
    [
        ("tenant", "Nova Tenant Service API", "Nova.TenantService"),
        ("identity", "Nova Identity Service API", "Nova.IdentityService"),
        ("system", "Nova System Service API", "Nova.SystemService"),
        ("business", "Nova Business Service API", "Nova.BusinessService")
    ];

    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        var configuration = context.Services.GetConfiguration();

        ConfigureConventionalControllers();
        context.Services.AddNovaSwaggerMultiDoc(SwaggerDocs);
        context.Services.AddNovaJwtAuthentication(configuration);

        ConfigureSqlSugar(context, configuration);
    }

    private void ConfigureConventionalControllers()
    {
        Configure<AbpAspNetCoreMvcOptions>(options =>
        {
            options.ConventionalControllers.Create(
                typeof(TenantServiceApplicationModule).Assembly,
                opts => opts.RootPath = "tenant");
            options.ConventionalControllers.Create(
                typeof(IdentityServiceApplicationModule).Assembly,
                opts => opts.RootPath = "identity");
            options.ConventionalControllers.Create(
                typeof(SystemServiceApplicationModule).Assembly,
                opts => opts.RootPath = "system");
            options.ConventionalControllers.Create(
                typeof(BusinessServiceApplicationModule).Assembly,
                opts => opts.RootPath = "business");
        });
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
                db.Aop.OnLogExecuting = (sql, _) =>
                {
                    var logger = sp.GetService<ILogger<AllInOneHostModule>>();
                    logger?.LogDebug("SQL: {Sql}", sql);
                };
            });
        });

        context.Services.AddScoped(typeof(INovaRepository<>), typeof(NovaRepository<>));

        context.Services.AddTransient<IUserRepository, UserRepository>();
    }

    public override void OnApplicationInitialization(ApplicationInitializationContext context)
    {
        InitializeDatabase(context.ServiceProvider);

        var app = context.GetApplicationBuilder();
        app.UseRouting();
        app.UseAuthentication();
        app.UseAuthorization();

        app.UseNovaHangfireDashboard();

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();
        });
        app.UseSwagger();
        app.UseSwaggerUI(options =>
        {
            foreach (var (name, title, _) in SwaggerDocs)
            {
                options.SwaggerEndpoint($"/swagger/{name}/swagger.json", title);
            }
        });
    }

    private static void InitializeDatabase(IServiceProvider serviceProvider)
    {
        var configuration = serviceProvider.GetRequiredService<IConfiguration>();
        var passwordHasher = serviceProvider.GetRequiredService<UserPasswordHasher>();
        var connectionString = configuration.GetConnectionString("Default");

        // 默认采用单库共享模式：所有表先落在共享库，租户通过 TenantId 做隔离。
        MySqlDatabaseCreator.EnsureCreated(connectionString);

        var db = serviceProvider.GetRequiredService<ISqlSugarClient>();
        db.CodeFirst.InitTables(typeof(TenantEntity), typeof(UserEntity), typeof(RoleEntity), typeof(MenuEntity), typeof(UserRoleEntity), typeof(RoleMenuEntity),
            typeof(DeptEntity), typeof(PostEntity), typeof(RoleDeptEntity), typeof(UserPostEntity),
            typeof(OperationLogEntity), typeof(LoginLogEntity),
            typeof(DictionaryTypeEntity), typeof(DictionaryDataEntity), typeof(ConfigEntity), typeof(NoticeEntity));

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

        if (!db.Queryable<UserEntity>().Any(x => x.UserName == NovaSeedConstants.DefaultAdminUserName))
        {
            var admin = new UserEntity
            {
                UserName = NovaSeedConstants.DefaultAdminUserName,
                Email = NovaSeedConstants.DefaultAdminEmail,
                Phone = NovaSeedConstants.DefaultAdminPhone,
                Status = UserStatus.Active,
                TenantId = null,
                CreateTime = DateTime.UtcNow
            };
            admin.SetPassword(passwordHasher.HashPassword(NovaSeedConstants.DefaultAdminPassword));

            db.Insertable(admin).ExecuteCommand();
        }

        try
        {
            var systemDictSeeder = serviceProvider.GetRequiredService<SystemDictSeedService>();
            systemDictSeeder.SeedAsync().GetAwaiter().GetResult();
        }
        catch (Exception ex)
        {
            var logger = serviceProvider.GetService<ILogger<AllInOneHostModule>>();
            logger?.LogError(ex, "System Dict 种子数据执行失败");
        }

        try
        {
            var identitySeeder = serviceProvider.GetRequiredService<IdentityRbacSeedService>();
            identitySeeder.SeedAsync().GetAwaiter().GetResult();
        }
        catch (Exception ex)
        {
            var logger = serviceProvider.GetService<ILogger<AllInOneHostModule>>();
            logger?.LogError(ex, "Identity RBAC 种子数据执行失败");
            throw;
        }
    }
}
