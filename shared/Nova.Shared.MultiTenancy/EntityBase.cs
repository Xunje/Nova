using SqlSugar;
using Volo.Abp.MultiTenancy;

namespace Nova.Shared.MultiTenancy;

/// <summary>
/// 多租户实体基类
/// <para>所有需要租户隔离的业务实体都应继承此类</para>
/// <para>自动包含：主键ID、租户ID、审计字段、软删除标记</para>
/// </summary>
public abstract class EntityBase : IMultiTenant
{
    /// <summary>
    /// 主键ID（自增）
    /// </summary>
    [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
    public long Id { get; set; }

    /// <summary>
    /// 创建时间（UTC时间）
    /// </summary>
    public DateTime CreateTime { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// 最后更新时间
    /// </summary>
    [SugarColumn(IsNullable = true)]
    public DateTime? UpdateTime { get; set; }

    /// <summary>
    /// 创建者用户ID
    /// </summary>
    [SugarColumn(IsNullable = true)]
    public long? CreateUserId { get; set; }

    /// <summary>
    /// 软删除标记（true表示已删除）
    /// </summary>
    public bool IsDeleted { get; set; } = false;

    /// <summary>
    /// 租户ID
    /// <para>null 表示无租户上下文下可见的平台数据</para>
    /// <para>非null 表示租户数据，查询时会附加租户过滤条件</para>
    /// </summary>
    [SugarColumn(ColumnName = "tenant_id", IsNullable = true)]
    public Guid? TenantId { get; set; }
}
