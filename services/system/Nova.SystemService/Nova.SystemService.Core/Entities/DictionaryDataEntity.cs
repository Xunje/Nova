using Nova.Shared.MultiTenancy;
using SqlSugar;

namespace Nova.SystemService.Core.Entities;

/// <summary>
/// 字典数据表
/// </summary>
[SugarTable("sys_dict_data")]
public class DictionaryDataEntity : EntityBase
{
    [SugarColumn(ColumnName = "dict_type", Length = 100)]
    public string DictType { get; set; } = string.Empty;

    [SugarColumn(ColumnName = "dict_label", Length = 100)]
    public string DictLabel { get; set; } = string.Empty;

    [SugarColumn(ColumnName = "dict_value", Length = 100)]
    public string DictValue { get; set; } = string.Empty;

    [SugarColumn(ColumnName = "css_class", Length = 100, IsNullable = true)]
    public string? CssClass { get; set; }

    [SugarColumn(ColumnName = "list_class", Length = 100, IsNullable = true)]
    public string? ListClass { get; set; }

    [SugarColumn(ColumnName = "is_default")]
    public bool IsDefault { get; set; }

    [SugarColumn(ColumnName = "status")]
    public bool Status { get; set; } = true;

    [SugarColumn(ColumnName = "order_num")]
    public int OrderNum { get; set; }

    [SugarColumn(ColumnName = "remark", Length = 500, IsNullable = true)]
    public string? Remark { get; set; }
}
