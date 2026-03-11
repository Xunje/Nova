using Nova.Shared.MultiTenancy;
using SqlSugar;

namespace Nova.IdentityService.Core.Entities;

[SugarTable("sys_role_menu")]
public class RoleMenuEntity : EntityBase
{
    [SugarColumn(ColumnName = "role_id")]
    public long RoleId { get; set; }

    [SugarColumn(ColumnName = "menu_id")]
    public long MenuId { get; set; }
}
