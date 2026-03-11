using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nova.Shared.Hosting;
using Nova.Shared.Hosting.BackgroundJobs;
using Nova.Shared.Hosting.Extensions;
using Nova.Shared.MultiTenancy;
using Nova.Shared.SqlSugar;
using Nova.SystemService.Application;
using Nova.SystemService.Application.Seeds;
using Nova.SystemService.Core.Consts;
using Nova.SystemService.Core.Entities;
using Nova.SystemService.Core.Security;
using Nova.SystemService.Core.Entities;
using Nova.SystemService.Host.Controllers;
using SqlSugar;
using Volo.Abp;
using Volo.Abp.AspNetCore.Mvc;
using Volo.Abp.Modularity;

namespace Nova.SystemService.Host;

[DependsOn(
    typeof(SystemServiceApplicationModule),
    typeof(NovaSharedHostingModule),
    typeof(NovaBackgroundWorkersHangfireModule)
)]
public class SystemServiceHostModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        var configuration = context.Services.GetConfiguration();

        Configure<AbpAspNetCoreMvcOptions>(options =>
        {
            options.ConventionalControllers.Create(
                typeof(SystemServiceApplicationModule).Assembly,
                opts => opts.RootPath = "system");
        });

        context.Services.AddNovaSwagger("Nova System Service API");
        context.Services.AddNovaJwtAuthentication(configuration);

        context.Services.AddNovaSqlSugar<SystemServiceHostModule>(
            typeof(UserEntity),
            typeof(DictionaryTypeEntity),
            typeof(DictionaryDataEntity),
            typeof(ConfigEntity),
            typeof(NoticeEntity),
            typeof(WechatUserBindingEntity),
            typeof(WechatPayOrderEntity));
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
        app.UseSwaggerUI(options => options.SwaggerEndpoint("/swagger/v1/swagger.json", "System Service API"));
    }

    private static void InitializeDatabase(IServiceProvider serviceProvider)
    {
        var configuration = serviceProvider.GetRequiredService<IConfiguration>();
        var passwordHasher = serviceProvider.GetRequiredService<UserPasswordHasher>();
        var connectionString = configuration.GetConnectionString("Default");

        MySqlDatabaseCreator.EnsureCreated(connectionString);

        var db = new SqlSugarClient(new ConnectionConfig
        {
            DbType = DbType.MySql,
            ConnectionString = connectionString,
            IsAutoCloseConnection = true,
            InitKeyType = InitKeyType.Attribute
        });

        db.CodeFirst.InitTables(
            typeof(UserEntity),
            typeof(DictionaryTypeEntity),
            typeof(DictionaryDataEntity),
            typeof(ConfigEntity),
            typeof(NoticeEntity),
            typeof(WechatUserBindingEntity),
            typeof(WechatPayOrderEntity));

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

        var dictSeeder = serviceProvider.GetRequiredService<SystemDictSeedService>();
        dictSeeder.SeedAsync().GetAwaiter().GetResult();
    }
}
