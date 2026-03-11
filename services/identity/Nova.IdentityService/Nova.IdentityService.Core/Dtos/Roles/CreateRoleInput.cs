using System.ComponentModel.DataAnnotations;
using Nova.IdentityService.Core.Enums;

namespace Nova.IdentityService.Core.Dtos.Roles;

public class CreateRoleInput
{
    [Required]
    [MaxLength(50)]
    public string RoleName { get; set; } = string.Empty;

    [Required]
    [MaxLength(50)]
    public string RoleCode { get; set; } = string.Empty;

    public DataScopeType DataScope { get; set; } = DataScopeType.All;
    public bool Status { get; set; } = true;
    public int OrderNum { get; set; }
    public string? Remark { get; set; }
}
