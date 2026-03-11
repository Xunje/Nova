using Nova.Shared.MultiTenancy;
using SqlSugar;

namespace Nova.SystemService.Core.Entities;

/// <summary>
/// 系统配置表
/// </summary>
[SugarTable("sys_config")]
public class ConfigEntity : EntityBase
{
    [SugarColumn(ColumnName = "config_name", Length = 100)]
    public string ConfigName { get; set; } = string.Empty;

    [SugarColumn(ColumnName = "config_key", Length = 100)]
    public string ConfigKey { get; set; } = string.Empty;

    [SugarColumn(ColumnName = "config_value", Length = 500)]
    public string ConfigValue { get; set; } = string.Empty;

    [SugarColumn(ColumnName = "config_type", Length = 50, IsNullable = true)]
    public string? ConfigType { get; set; }

    [SugarColumn(ColumnName = "order_num")]
    public int OrderNum { get; set; }

    [SugarColumn(ColumnName = "remark", Length = 500, IsNullable = true)]
    public string? Remark { get; set; }
}
