using Nova.IdentityService.Core.Dtos.Menus;

namespace Nova.IdentityService.Core.Dtos.Accounts;

public class CurrentUserAccessDto
{
    public long UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public Guid? TenantId { get; set; }
    public List<string> RoleCodes { get; set; } = [];
    public List<string> PermissionCodes { get; set; } = [];
    public List<MenuDto> Menus { get; set; } = [];
}
