using Nova.Shared.MultiTenancy;
using SqlSugar;

namespace Nova.SystemService.Core.Entities;

[SugarTable("sys_wechat_user_binding")]
public class WechatUserBindingEntity : EntityBase
{
    [SugarColumn(ColumnName = "user_id")]
    public long UserId { get; set; }

    [SugarColumn(ColumnName = "wechat_app_type", Length = 32)]
    public string WechatAppType { get; set; } = string.Empty;

    [SugarColumn(ColumnName = "app_id", Length = 64)]
    public string AppId { get; set; } = string.Empty;

    [SugarColumn(ColumnName = "open_id", Length = 128)]
    public string OpenId { get; set; } = string.Empty;

    [SugarColumn(ColumnName = "union_id", Length = 128, IsNullable = true)]
    public string? UnionId { get; set; }

    [SugarColumn(ColumnName = "nick_name", Length = 100, IsNullable = true)]
    public string? NickName { get; set; }

    [SugarColumn(ColumnName = "last_login_time", IsNullable = true)]
    public DateTime? LastLoginTime { get; set; }
}
