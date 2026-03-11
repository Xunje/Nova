namespace Nova.TenantService.Core.Dtos;

/// <summary>
/// 更新租户独立数据库连接配置
/// </summary>
public class UpdateTenantConnectionStringInput
{
    /// <summary>
    /// 租户独立数据库连接字符串
    /// <para>null 或空字符串表示切回共享数据库模式</para>
    /// </summary>
    public string? ConnectionString { get; set; }
}
