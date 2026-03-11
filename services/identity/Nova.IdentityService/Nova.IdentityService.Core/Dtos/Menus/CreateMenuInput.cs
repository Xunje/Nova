using System.ComponentModel.DataAnnotations;
using Nova.IdentityService.Core.Enums;

namespace Nova.IdentityService.Core.Dtos.Menus;

public class CreateMenuInput
{
    [Required]
    [MaxLength(50)]
    public string MenuName { get; set; } = string.Empty;

    public string? PermissionCode { get; set; }
    public long ParentId { get; set; }
    public MenuType MenuType { get; set; } = MenuType.Menu;
    public string? RouterName { get; set; }
    public string? Router { get; set; }
    public string? Component { get; set; }
    public string? Icon { get; set; }
    public bool IsLink { get; set; }
    public bool IsCache { get; set; }
    public bool IsVisible { get; set; } = true;
    public bool Status { get; set; } = true;
    public int OrderNum { get; set; }
    public string? Remark { get; set; }
}
