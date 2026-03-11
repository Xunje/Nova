using Nova.Shared.MultiTenancy;
using Volo.Abp.Modularity;

namespace Nova.IdentityService.Core;

[DependsOn(typeof(NovaSharedMultiTenancyModule))]
public class IdentityServiceCoreModule : AbpModule
{
}
