using Volo.Abp.EventBus;

namespace Nova.Shared.EventBus.Events;

/// <summary>
/// 用户创建集成事件
/// <para>当用户服务创建新用户时发布此事件</para>
/// <para>其他服务可订阅此事件进行数据同步（如：初始化用户配置、发送欢迎邮件等）</para>
/// </summary>
[EventName("nova.user.created")]
public class UserCreatedIntegrationEvent : TenantIntegrationEvent
{
    /// <summary>
    /// 用户ID
    /// </summary>
    public long UserId { get; set; }

    /// <summary>
    /// 用户名
    /// </summary>
    public string UserName { get; set; } = string.Empty;

    /// <summary>
    /// 邮箱地址
    /// </summary>
    public string Email { get; set; } = string.Empty;
}
