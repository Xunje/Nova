using System.Security.Claims;
using Volo.Abp.Security.Claims;

namespace Nova.Shared.Hosting.Security;

/// <summary>
/// Nova 统一 Claim 类型定义
/// <para>兼容 ABP 标准 Claim、Yi 风格多条 permission Claim 和聚合 permissions Claim</para>
/// </summary>
public static class NovaClaimTypes
{
    public const string Permission = "permission";
    public const string Permissions = "permissions";
    public const string TenantId = "tenant_id";
    public const string UserName = ClaimTypes.Name;

    public static readonly string[] TenantIdAliases =
    [
        AbpClaimTypes.TenantId,
        TenantId,
        "__tenant"
    ];

    public static readonly string[] PermissionAliases =
    [
        Permission,
        Permissions
    ];
}
