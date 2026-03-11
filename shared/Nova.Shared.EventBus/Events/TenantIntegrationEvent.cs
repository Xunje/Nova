namespace Nova.Shared.EventBus.Events;

/// <summary>
/// 租户集成事件基类
/// <para>所有跨服务的集成事件都应继承此类</para>
/// <para>自动携带租户ID，确保消费方能在正确的租户上下文中处理事件</para>
/// </summary>
public abstract class TenantIntegrationEvent
{
    /// <summary>
    /// 租户ID
    /// <para>消费方据此切换到正确的租户上下文</para>
    /// <para>null表示平台级事件</para>
    /// </summary>
    public Guid? TenantId { get; set; }

    /// <summary>
    /// 事件发生时间（UTC）
    /// </summary>
    public DateTime OccurredAt { get; set; } = DateTime.UtcNow;
}
