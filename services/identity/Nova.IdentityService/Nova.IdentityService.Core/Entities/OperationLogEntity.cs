using Nova.IdentityService.Core.Enums;
using Nova.Shared.MultiTenancy;
using SqlSugar;

namespace Nova.IdentityService.Core.Entities;

/// <summary>
/// 操作日志表
/// </summary>
[SugarTable("sys_operation_log")]
public class OperationLogEntity : EntityBase
{
    [SugarColumn(ColumnName = "title", Length = 100, IsNullable = true)]
    public string? Title { get; set; }

    [SugarColumn(ColumnName = "oper_type")]
    public OperType OperType { get; set; }

    [SugarColumn(ColumnName = "request_method", Length = 10, IsNullable = true)]
    public string? RequestMethod { get; set; }

    [SugarColumn(ColumnName = "oper_user", Length = 50, IsNullable = true)]
    public string? OperUser { get; set; }

    [SugarColumn(ColumnName = "oper_ip", Length = 50, IsNullable = true)]
    public string? OperIp { get; set; }

    [SugarColumn(ColumnName = "oper_location", Length = 200, IsNullable = true)]
    public string? OperLocation { get; set; }

    [SugarColumn(ColumnName = "method", Length = 500, IsNullable = true)]
    public string? Method { get; set; }

    [SugarColumn(ColumnName = "request_param", ColumnDataType = "text", IsNullable = true)]
    public string? RequestParam { get; set; }

    [SugarColumn(ColumnName = "request_result", ColumnDataType = "text", IsNullable = true)]
    public string? RequestResult { get; set; }
}
