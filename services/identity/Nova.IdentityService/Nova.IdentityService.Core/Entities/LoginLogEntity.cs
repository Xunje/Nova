using Nova.IdentityService.Core.Enums;
using Nova.Shared.MultiTenancy;
using SqlSugar;

namespace Nova.IdentityService.Core.Entities;

/// <summary>
/// 登录日志表
/// </summary>
[SugarTable("sys_login_log")]
[SugarIndex("idx_login_user", nameof(LoginUser), OrderByType.Asc)]
[SugarIndex("idx_login_time", nameof(CreateTime), OrderByType.Desc)]
public class LoginLogEntity : EntityBase
{
    [SugarColumn(ColumnName = "login_user", Length = 50, IsNullable = true)]
    public string? LoginUser { get; set; }

    [SugarColumn(ColumnName = "login_location", Length = 200, IsNullable = true)]
    public string? LoginLocation { get; set; }

    [SugarColumn(ColumnName = "login_ip", Length = 50, IsNullable = true)]
    public string? LoginIp { get; set; }

    [SugarColumn(ColumnName = "browser", Length = 100, IsNullable = true)]
    public string? Browser { get; set; }

    [SugarColumn(ColumnName = "os", Length = 100, IsNullable = true)]
    public string? Os { get; set; }

    [SugarColumn(ColumnName = "log_msg", Length = 500, IsNullable = true)]
    public string? LogMsg { get; set; }

    [SugarColumn(ColumnName = "login_status")]
    public LoginStatus LoginStatus { get; set; }
}
