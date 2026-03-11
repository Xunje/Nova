using Nova.IdentityService.Core.Enums;
using Nova.Shared.MultiTenancy;
using SqlSugar;

namespace Nova.IdentityService.Core.Entities;

[SugarTable("sys_menu")]
public class MenuEntity : EntityBase
{
    [SugarColumn(ColumnName = "menu_name", Length = 50)]
    public string MenuName { get; set; } = string.Empty;

    [SugarColumn(ColumnName = "permission_code", Length = 100, IsNullable = true)]
    public string? PermissionCode { get; set; }

    [SugarColumn(ColumnName = "parent_id")]
    public long ParentId { get; set; }

    [SugarColumn(ColumnName = "menu_type")]
    public MenuType MenuType { get; set; } = MenuType.Menu;

    [SugarColumn(ColumnName = "router_name", Length = 100, IsNullable = true)]
    public string? RouterName { get; set; }

    [SugarColumn(ColumnName = "router", Length = 200, IsNullable = true)]
    public string? Router { get; set; }

    [SugarColumn(ColumnName = "component", Length = 200, IsNullable = true)]
    public string? Component { get; set; }

    [SugarColumn(ColumnName = "icon", Length = 100, IsNullable = true)]
    public string? Icon { get; set; }

    [SugarColumn(ColumnName = "is_link")]
    public bool IsLink { get; set; }

    [SugarColumn(ColumnName = "is_cache")]
    public bool IsCache { get; set; }

    [SugarColumn(ColumnName = "is_visible")]
    public bool IsVisible { get; set; } = true;

    [SugarColumn(ColumnName = "status")]
    public bool Status { get; set; } = true;

    [SugarColumn(ColumnName = "order_num")]
    public int OrderNum { get; set; }

    [SugarColumn(ColumnName = "remark", Length = 200, IsNullable = true)]
    public string? Remark { get; set; }
}
