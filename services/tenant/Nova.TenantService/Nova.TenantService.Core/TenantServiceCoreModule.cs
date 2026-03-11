using Nova.Shared.MultiTenancy;
using Volo.Abp.Modularity;

namespace Nova.TenantService.Core;

[DependsOn(typeof(NovaSharedMultiTenancyModule))]
public class TenantServiceCoreModule : AbpModule
{
}
