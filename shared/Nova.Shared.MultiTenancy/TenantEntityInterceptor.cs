using Volo.Abp.DependencyInjection;
using Volo.Abp.MultiTenancy;

namespace Nova.Shared.MultiTenancy;

/// <summary>
/// 租户实体拦截器
/// <para>在实体插入前自动填充租户ID</para>
/// <para>确保所有新增数据都正确关联到当前租户</para>
/// </summary>
public class TenantEntityInterceptor : ITransientDependency
{
    private readonly ICurrentTenant _currentTenant;

    public TenantEntityInterceptor(ICurrentTenant currentTenant)
    {
        _currentTenant = currentTenant;
    }

    /// <summary>
    /// 为实体填充当前租户ID
    /// <para>仅在实体未设置租户ID时才填充</para>
    /// </summary>
    /// <param name="entity">需要填充租户ID的实体</param>
    public void FillTenantId(EntityBase entity)
    {
        if (entity.TenantId == null)
            entity.TenantId = _currentTenant.Id;
    }
}
