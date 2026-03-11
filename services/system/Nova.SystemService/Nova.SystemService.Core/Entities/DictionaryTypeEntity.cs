using Nova.Shared.MultiTenancy;
using SqlSugar;

namespace Nova.SystemService.Core.Entities;

/// <summary>
/// 字典类型表
/// </summary>
[SugarTable("sys_dict_type")]
public class DictionaryTypeEntity : EntityBase
{
    [SugarColumn(ColumnName = "dict_name", Length = 100)]
    public string DictName { get; set; } = string.Empty;

    [SugarColumn(ColumnName = "dict_type", Length = 100)]
    public string DictType { get; set; } = string.Empty;

    [SugarColumn(ColumnName = "status")]
    public bool Status { get; set; } = true;

    [SugarColumn(ColumnName = "order_num")]
    public int OrderNum { get; set; }

    [SugarColumn(ColumnName = "remark", Length = 500, IsNullable = true)]
    public string? Remark { get; set; }
}
