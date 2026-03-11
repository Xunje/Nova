using Nova.IdentityService.Core.Enums;

namespace Nova.IdentityService.Core.Dtos.Menus;

public class MenuDto
{
    public long Id { get; set; }
    public long ParentId { get; set; }
    public string MenuName { get; set; } = string.Empty;
    public string? PermissionCode { get; set; }
    public MenuType MenuType { get; set; }
    public string? RouterName { get; set; }
    public string? Router { get; set; }
    public string? Component { get; set; }
    public string? Icon { get; set; }
    public bool IsLink { get; set; }
    public bool IsCache { get; set; }
    public bool IsVisible { get; set; }
    public bool Status { get; set; }
    public int OrderNum { get; set; }
    public List<MenuDto> Children { get; set; } = [];
}
