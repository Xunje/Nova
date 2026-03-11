using System.Security.Claims;
using Microsoft.Extensions.DependencyInjection;
using Nova.IdentityService.Core.Entities;
using SqlSugar;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.MultiTenancy;

namespace Nova.Shared.Hosting.Permissions;

public class NovaRolePermissionValueProvider : PermissionValueProvider
{
    public override string Name => "R";

    public NovaRolePermissionValueProvider(IPermissionStore permissionStore)
        : base(permissionStore)
    {
    }

    public override async Task<PermissionGrantResult> CheckAsync(PermissionValueCheckContext context)
    {
        var roleCodes = context.Principal?.FindAll(ClaimTypes.Role)
            .Select(x => x.Value)
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        if (roleCodes == null || roleCodes.Count == 0)
        {
            return PermissionGrantResult.Undefined;
        }

        var isGranted = await IsGrantedAsync(context.Principal, context.Permission.Name, context);
        return isGranted ? PermissionGrantResult.Granted : PermissionGrantResult.Undefined;
    }

    public override async Task<MultiplePermissionGrantResult> CheckAsync(PermissionValuesCheckContext context)
    {
        var result = new MultiplePermissionGrantResult();

        foreach (var permission in context.Permissions)
        {
            var isGranted = await IsGrantedAsync(context.Principal, permission.Name, context);
            result.Result[permission.Name] = isGranted
                ? PermissionGrantResult.Granted
                : PermissionGrantResult.Undefined;
        }

        return result;
    }

    private static async Task<bool> IsGrantedAsync(ClaimsPrincipal? principal, string permissionName, object context)
    {
        if (principal?.Identity?.IsAuthenticated != true)
        {
            return false;
        }

        var userName = principal.FindFirst(ClaimTypes.Name)?.Value;
        if (string.Equals(userName, "admin", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        var roleCodes = principal.FindAll(ClaimTypes.Role)
            .Select(x => x.Value)
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        if (roleCodes.Count == 0)
        {
            return false;
        }

        var serviceProvider = GetServiceProvider(context);
        var db = serviceProvider.GetRequiredService<ISqlSugarClient>();
        var currentTenant = serviceProvider.GetRequiredService<ICurrentTenant>();
        var tenantId = currentTenant.Id;

        var roleList = await db.Queryable<RoleEntity>()
            .Where(role => roleCodes.Contains(role.RoleCode) && role.Status && !role.IsDeleted)
            .WhereIF(tenantId.HasValue, role => role.TenantId == tenantId)
            .ToListAsync();

        if (roleList.Count == 0)
        {
            return false;
        }

        var roleIds = roleList.Select(role => role.Id).ToList();

        var menuIds = await db.Queryable<RoleMenuEntity>()
            .Where(link => roleIds.Contains(link.RoleId) && !link.IsDeleted)
            .Select(link => link.MenuId)
            .ToListAsync();

        if (menuIds.Count == 0)
        {
            return false;
        }

        return await db.Queryable<MenuEntity>()
            .Where(menu => menuIds.Contains(menu.Id) && menu.Status && !menu.IsDeleted)
            .AnyAsync(menu => menu.PermissionCode == permissionName);
    }

    private static IServiceProvider GetServiceProvider(object context)
    {
        var property = context.GetType().GetProperty("ServiceProvider");
        if (property?.GetValue(context) is IServiceProvider serviceProvider)
        {
            return serviceProvider;
        }

        throw new InvalidOperationException("Permission context does not expose ServiceProvider.");
    }
}
