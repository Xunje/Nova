using Nova.IdentityService.Core.Enums;
using Nova.Shared.MultiTenancy;
using SqlSugar;

namespace Nova.IdentityService.Core.Entities;

[SugarTable("sys_role")]
public class RoleEntity : EntityBase
{
    [SugarColumn(ColumnName = "role_name", Length = 50)]
    public string RoleName { get; set; } = string.Empty;

    [SugarColumn(ColumnName = "role_code", Length = 50)]
    public string RoleCode { get; set; } = string.Empty;

    [SugarColumn(ColumnName = "data_scope")]
    public DataScopeType DataScope { get; set; } = DataScopeType.All;

    [SugarColumn(ColumnName = "status")]
    public bool Status { get; set; } = true;

    [SugarColumn(ColumnName = "order_num")]
    public int OrderNum { get; set; }

    [SugarColumn(ColumnName = "remark", Length = 200, IsNullable = true)]
    public string? Remark { get; set; }
}
