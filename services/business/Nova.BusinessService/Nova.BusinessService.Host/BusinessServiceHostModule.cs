using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nova.BusinessService.Application;
using Nova.Shared.Hosting;
using Nova.Shared.Hosting.Extensions;
using Nova.Shared.SqlSugar;
using Volo.Abp;
using Volo.Abp.AspNetCore.Mvc;
using Volo.Abp.Modularity;

namespace Nova.BusinessService.Host;

[DependsOn(
    typeof(BusinessServiceApplicationModule),
    typeof(NovaSharedHostingModule)
)]
public class BusinessServiceHostModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        var configuration = context.Services.GetConfiguration();

        Configure<AbpAspNetCoreMvcOptions>(options =>
        {
            options.ConventionalControllers.Create(
                typeof(BusinessServiceApplicationModule).Assembly,
                opts => opts.RootPath = "business");
        });

        context.Services.AddNovaSwagger("Nova Business Service API");
        context.Services.AddNovaJwtAuthentication(configuration);

        context.Services.AddNovaSqlSugar<BusinessServiceHostModule>();
    }

    public override void OnApplicationInitialization(ApplicationInitializationContext context)
    {
        var app = context.GetApplicationBuilder();
        app.UseRouting();
        app.UseAuthentication();
        app.UseAuthorization();
        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();
        });
        app.UseSwagger();
        app.UseSwaggerUI(options => options.SwaggerEndpoint("/swagger/v1/swagger.json", "Business Service API"));
    }
}
