using Nova.IdentityService.Core.Enums;

namespace Nova.IdentityService.Core.Dtos.Roles;

public class RoleDto
{
    public long Id { get; set; }
    public string RoleName { get; set; } = string.Empty;
    public string RoleCode { get; set; } = string.Empty;
    public DataScopeType DataScope { get; set; }
    public bool Status { get; set; }
    public int OrderNum { get; set; }
    public string? Remark { get; set; }
}
