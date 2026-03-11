using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nova.Shared.EventBus;
using Nova.SystemService.Application.Wechat;
using Nova.SystemService.Core.Wechat;
using Nova.SystemService.Core;
using Nova.SystemService.Core.IServices;
using Volo.Abp.Modularity;

namespace Nova.SystemService.Application;

[DependsOn(
    typeof(SystemServiceCoreModule),
    typeof(NovaSharedEventBusModule)
)]
public class SystemServiceApplicationModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        var configuration = context.Services.GetConfiguration();
        context.Services.Configure<WechatOptions>(configuration.GetSection("Wechat"));
        context.Services.AddTransient<IWechatAppService, WechatAppService>();
    }
}
