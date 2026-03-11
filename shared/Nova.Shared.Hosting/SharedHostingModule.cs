using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Nova.Shared.Hosting.Permissions;
using Volo.Abp.AspNetCore.Mvc;
using Volo.Abp.Authorization;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Autofac;
using Volo.Abp.Modularity;
using Volo.Abp.Swashbuckle;

namespace Nova.Shared.Hosting;

/// <summary>
/// 共享托管模块
/// <para>提供微服务通用的托管基础设施</para>
/// <para>包含：ABP MVC、Autofac依赖注入、Swagger、全局异常过滤、多租户支持</para>
/// </summary>
[DependsOn(
    typeof(AbpAspNetCoreMvcModule),
    typeof(AbpAuthorizationModule),
    typeof(AbpAutofacModule),
    typeof(AbpSwashbuckleModule),
    typeof(MultiTenancy.NovaSharedMultiTenancyModule)
)]
public class NovaSharedHostingModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddHttpContextAccessor();

        // 配置MVC选项：添加全局异常过滤器
        Configure<MvcOptions>(options =>
        {
            options.Filters.Add<Filters.GlobalExceptionFilter>();
        });

        Configure<AbpPermissionOptions>(options =>
        {
            options.ValueProviders.Add<NovaRolePermissionValueProvider>();
        });
    }
}
