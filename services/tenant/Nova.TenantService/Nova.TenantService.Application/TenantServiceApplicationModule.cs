using Nova.Shared.EventBus;
using Nova.TenantService.Core;
using Volo.Abp.Modularity;

namespace Nova.TenantService.Application;

[DependsOn(
    typeof(TenantServiceCoreModule),
    typeof(NovaSharedEventBusModule)
)]
public class TenantServiceApplicationModule : AbpModule
{
}
