using Nova.Shared.MultiTenancy;
using SqlSugar;

namespace Nova.IdentityService.Core.Entities;

/// <summary>
/// 岗位表
/// </summary>
[SugarTable("sys_post")]
public class PostEntity : EntityBase
{
    [SugarColumn(ColumnName = "post_code", Length = 50)]
    public string PostCode { get; set; } = string.Empty;

    [SugarColumn(ColumnName = "post_name", Length = 50)]
    public string PostName { get; set; } = string.Empty;

    [SugarColumn(ColumnName = "order_num")]
    public int OrderNum { get; set; }

    [SugarColumn(ColumnName = "status")]
    public bool Status { get; set; } = true;

    [SugarColumn(ColumnName = "remark", Length = 200, IsNullable = true)]
    public string? Remark { get; set; }
}
