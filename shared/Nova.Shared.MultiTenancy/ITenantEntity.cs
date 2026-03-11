namespace Nova.Shared.MultiTenancy;

/// <summary>
/// 租户实体标记接口
/// <para>实现此接口的实体将被视为需要租户隔离的实体</para>
/// <para>用于类型约束和过滤器识别</para>
/// </summary>
public interface ITenantEntity
{
    /// <summary>
    /// 租户ID（可为null，表示平台级数据）
    /// </summary>
    Guid? TenantId { get; set; }
}
