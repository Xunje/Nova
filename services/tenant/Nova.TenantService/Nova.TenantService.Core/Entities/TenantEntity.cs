using SqlSugar;

namespace Nova.TenantService.Core.Entities;

/// <summary>
/// 租户实体
/// <para>注意：此实体不继承EntityBase，因为租户表属于平台级数据</para>
/// <para>租户数据不应受租户隔离过滤影响</para>
/// </summary>
[SugarTable("sys_tenant")]
public class TenantEntity
{
    /// <summary>
    /// 租户ID（主键，GUID）
    /// </summary>
    [SugarColumn(IsPrimaryKey = true)]
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// 租户名称（显示名称）
    /// </summary>
    [SugarColumn(ColumnName = "name", Length = 100)]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 租户标识（用于子域名解析，如 tenant1.yourapp.com）
    /// </summary>
    [SugarColumn(ColumnName = "code", Length = 50)]
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// 租户状态（启用/禁用）
    /// </summary>
    [SugarColumn(ColumnName = "status")]
    public TenantStatus Status { get; set; } = TenantStatus.Active;

    /// <summary>
    /// 租户套餐（免费版/专业版/企业版）
    /// </summary>
    [SugarColumn(ColumnName = "plan")]
    public TenantPlan Plan { get; set; } = TenantPlan.Free;

    /// <summary>
    /// 套餐过期时间（null表示永不过期）
    /// </summary>
    [SugarColumn(ColumnName = "expire_time", IsNullable = true)]
    public DateTime? ExpireTime { get; set; }

    /// <summary>
    /// 独立数据库连接字符串
    /// <para>VIP租户可使用独立数据库，null表示使用共享数据库</para>
    /// </summary>
    [SugarColumn(ColumnName = "connection_string", Length = 500, IsNullable = true)]
    public string? ConnectionString { get; set; }

    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTime CreateTime { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// 软删除标记
    /// </summary>
    public bool IsDeleted { get; set; } = false;
}

/// <summary>
/// 租户状态枚举
/// </summary>
public enum TenantStatus
{
    /// <summary>
    /// 启用
    /// </summary>
    Active = 1,

    /// <summary>
    /// 禁用
    /// </summary>
    Disabled = 0
}

/// <summary>
/// 租户套餐枚举
/// </summary>
public enum TenantPlan
{
    /// <summary>
    /// 免费版（共享数据库）
    /// </summary>
    Free = 0,

    /// <summary>
    /// 专业版
    /// </summary>
    Pro = 1,

    /// <summary>
    /// 企业版（独立数据库）
    /// </summary>
    Enterprise = 2
}
