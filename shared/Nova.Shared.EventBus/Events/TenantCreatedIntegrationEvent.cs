using Volo.Abp.EventBus;

namespace Nova.Shared.EventBus.Events;

/// <summary>
/// 租户创建集成事件
/// <para>当创建新租户时发布此事件</para>
/// <para>其他服务可订阅此事件进行租户初始化（如：创建默认数据、分配资源等）</para>
/// </summary>
[EventName("nova.tenant.created")]
public class TenantCreatedIntegrationEvent : TenantIntegrationEvent
{
    /// <summary>
    /// 租户标识（用于子域名解析）
    /// </summary>
    public string TenantCode { get; set; } = string.Empty;
}
