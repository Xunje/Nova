using Volo.Abp.Modularity;
using Volo.Abp.MultiTenancy;

namespace Nova.Shared.MultiTenancy;

/// <summary>
/// 多租户共享模块
/// <para>提供多租户基础设施：EntityBase、租户上下文、连接解析器等</para>
/// <para>所有需要多租户支持的服务都应依赖此模块</para>
/// </summary>
[DependsOn(typeof(AbpMultiTenancyModule))]
public class NovaSharedMultiTenancyModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        // 启用ABP多租户功能
        Configure<AbpMultiTenancyOptions>(options =>
        {
            options.IsEnabled = true;
        });
    }
}
