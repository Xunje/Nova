using Volo.Abp.Localization;
using Volo.Abp.Modularity;

namespace Nova.Shared.Localization;

[DependsOn(typeof(AbpLocalizationModule))]
public class NovaSharedLocalizationModule : AbpModule
{
}
