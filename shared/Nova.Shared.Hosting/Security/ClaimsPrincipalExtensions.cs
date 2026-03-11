using System.Security.Claims;
using System.Text.Json;

namespace Nova.Shared.Hosting.Security;

/// <summary>
/// ClaimsPrincipal 扩展
/// <para>统一解析权限集合与租户信息</para>
/// </summary>
public static class ClaimsPrincipalExtensions
{
    public static IReadOnlyCollection<string> GetPermissionCodes(this ClaimsPrincipal? principal)
    {
        if (principal == null)
            return Array.Empty<string>();

        var permissions = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var alias in NovaClaimTypes.PermissionAliases)
        {
            foreach (var claim in principal.FindAll(alias))
            {
                foreach (var item in SplitPermissionValue(claim.Value))
                {
                    permissions.Add(item);
                }
            }
        }

        return permissions.ToArray();
    }

    public static Guid? GetTenantIdOrNull(this ClaimsPrincipal? principal)
    {
        if (principal == null)
            return null;

        foreach (var alias in NovaClaimTypes.TenantIdAliases)
        {
            var value = principal.FindFirst(alias)?.Value;
            if (Guid.TryParse(value, out var tenantId))
                return tenantId;
        }

        return null;
    }

    private static IEnumerable<string> SplitPermissionValue(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            yield break;

        var trimmed = value.Trim();
        if (trimmed.StartsWith('[') && trimmed.EndsWith(']'))
        {
            string[]? parsed = null;

            try
            {
                parsed = JsonSerializer.Deserialize<string[]>(trimmed);
            }
            catch
            {
                parsed = null;
            }

            if (parsed != null)
            {
                foreach (var item in parsed.Where(item => !string.IsNullOrWhiteSpace(item)))
                {
                    yield return item.Trim();
                }

                yield break;
            }
        }

        foreach (var item in trimmed
                     .Split([',', ';', ' '], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            yield return item;
        }
    }
}
