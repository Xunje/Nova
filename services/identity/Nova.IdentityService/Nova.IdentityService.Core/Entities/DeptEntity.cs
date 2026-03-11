using Nova.Shared.MultiTenancy;
using SqlSugar;

namespace Nova.IdentityService.Core.Entities;

/// <summary>
/// 部门表
/// </summary>
[SugarTable("sys_dept")]
public class DeptEntity : EntityBase
{
    [SugarColumn(ColumnName = "dept_name", Length = 50)]
    public string DeptName { get; set; } = string.Empty;

    [SugarColumn(ColumnName = "dept_code", Length = 50)]
    public string DeptCode { get; set; } = string.Empty;

    [SugarColumn(ColumnName = "parent_id")]
    public long ParentId { get; set; }

    [SugarColumn(ColumnName = "leader", Length = 50, IsNullable = true)]
    public string? Leader { get; set; }

    [SugarColumn(ColumnName = "order_num")]
    public int OrderNum { get; set; }

    [SugarColumn(ColumnName = "status")]
    public bool Status { get; set; } = true;

    [SugarColumn(ColumnName = "remark", Length = 200, IsNullable = true)]
    public string? Remark { get; set; }
}
