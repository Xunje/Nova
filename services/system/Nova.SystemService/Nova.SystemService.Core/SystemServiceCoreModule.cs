using Nova.Shared.MultiTenancy;
using Volo.Abp.Modularity;

namespace Nova.SystemService.Core;

[DependsOn(typeof(NovaSharedMultiTenancyModule))]
public class SystemServiceCoreModule : AbpModule
{
}
