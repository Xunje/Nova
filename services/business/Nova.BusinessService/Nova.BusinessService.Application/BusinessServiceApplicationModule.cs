using Nova.BusinessService.Core;
using Nova.Shared.EventBus;
using Volo.Abp.Modularity;

namespace Nova.BusinessService.Application;

[DependsOn(
    typeof(BusinessServiceCoreModule),
    typeof(NovaSharedEventBusModule)
)]
public class BusinessServiceApplicationModule : AbpModule
{
}
