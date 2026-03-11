using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nova.IdentityService.Application;
using Nova.IdentityService.Application.OperLog;
using Nova.IdentityService.Application.Seeds;
using Nova.IdentityService.Core.Entities;
using Nova.Shared.Hosting;
using Nova.Shared.Hosting.Extensions;
using Nova.Shared.MultiTenancy;
using Nova.Shared.SqlSugar;
using Volo.Abp;
using Volo.Abp.AspNetCore.Mvc;
using Volo.Abp.Modularity;

namespace Nova.IdentityService.Host;

[DependsOn(
    typeof(IdentityServiceApplicationModule),
    typeof(NovaSharedHostingModule)
)]
public class IdentityServiceHostModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        var configuration = context.Services.GetConfiguration();

        Configure<AbpAspNetCoreMvcOptions>(options =>
        {
            options.ConventionalControllers.Create(
                typeof(IdentityServiceApplicationModule).Assembly,
                opts => opts.RootPath = "identity");
        });

        Configure<MvcOptions>(options =>
        {
            options.Filters.Add<OperLogFilter>();
        });

        context.Services.AddNovaSwagger("Nova Identity Service API");
        context.Services.AddNovaJwtAuthentication(configuration);
        context.Services.AddNovaSqlSugar<IdentityServiceHostModule>(
            typeof(RoleEntity),
            typeof(MenuEntity),
            typeof(UserRoleEntity),
            typeof(RoleMenuEntity),
            typeof(DeptEntity),
            typeof(PostEntity),
            typeof(RoleDeptEntity),
            typeof(UserPostEntity),
            typeof(OperationLogEntity),
            typeof(LoginLogEntity));
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
        app.UseSwaggerUI(options => options.SwaggerEndpoint("/swagger/v1/swagger.json", "Identity Service API"));
    }

    private static void InitializeDatabase(IServiceProvider serviceProvider)
    {
        var configuration = serviceProvider.GetRequiredService<IConfiguration>();
        var connectionString = configuration.GetConnectionString("Default");
        MySqlDatabaseCreator.EnsureCreated(connectionString);

        var db = serviceProvider.GetRequiredService<SqlSugar.ISqlSugarClient>();
        db.CodeFirst.InitTables(typeof(RoleEntity), typeof(MenuEntity), typeof(UserRoleEntity), typeof(RoleMenuEntity),
            typeof(DeptEntity), typeof(PostEntity), typeof(RoleDeptEntity), typeof(UserPostEntity),
            typeof(OperationLogEntity), typeof(LoginLogEntity));

        var seeder = serviceProvider.GetRequiredService<IdentityRbacSeedService>();
        seeder.SeedAsync().GetAwaiter().GetResult();
    }
}
