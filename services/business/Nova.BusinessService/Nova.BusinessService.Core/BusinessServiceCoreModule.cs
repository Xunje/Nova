using Nova.Shared.MultiTenancy;
using Volo.Abp.Modularity;

namespace Nova.BusinessService.Core;

[DependsOn(typeof(NovaSharedMultiTenancyModule))]
public class BusinessServiceCoreModule : AbpModule
{
}
