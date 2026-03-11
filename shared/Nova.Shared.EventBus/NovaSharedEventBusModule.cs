using Volo.Abp.Modularity;

namespace Nova.Shared.EventBus;

/// <summary>
/// 共享事件总线模块
/// <para>定义跨服务集成事件的契约</para>
/// <para>所有需要发布/订阅集成事件的服务都应依赖此模块</para>
/// </summary>
public class NovaSharedEventBusModule : AbpModule
{
}
