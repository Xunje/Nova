using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nova.IdentityService.Core;
using Nova.Shared.EventBus;
using Nova.SystemService.Core;
using Nova.IdentityService.Core.Wechat;
using Volo.Abp.Modularity;

namespace Nova.IdentityService.Application;

[DependsOn(
    typeof(IdentityServiceCoreModule),
    typeof(NovaSharedEventBusModule),
    typeof(SystemServiceCoreModule)
)]
public class IdentityServiceApplicationModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        var configuration = context.Services.GetConfiguration();
        context.Services.Configure<WechatOptions>(configuration.GetSection("Wechat"));
    }
}
