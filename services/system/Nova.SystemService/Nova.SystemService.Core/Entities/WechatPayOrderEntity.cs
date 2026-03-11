using Nova.Shared.MultiTenancy;
using SqlSugar;

namespace Nova.SystemService.Core.Entities;

[SugarTable("sys_wechat_pay_order")]
public class WechatPayOrderEntity : EntityBase
{
    [SugarColumn(ColumnName = "out_trade_number", Length = 64)]
    public string OutTradeNumber { get; set; } = string.Empty;

    [SugarColumn(ColumnName = "wechat_transaction_id", Length = 64, IsNullable = true)]
    public string? WechatTransactionId { get; set; }

    [SugarColumn(ColumnName = "app_id", Length = 64)]
    public string AppId { get; set; } = string.Empty;

    [SugarColumn(ColumnName = "open_id", Length = 128)]
    public string OpenId { get; set; } = string.Empty;

    [SugarColumn(ColumnName = "description", Length = 255)]
    public string Description { get; set; } = string.Empty;

    [SugarColumn(ColumnName = "total")]
    public int Total { get; set; }

    [SugarColumn(ColumnName = "trade_state", Length = 32, IsNullable = true)]
    public string? TradeState { get; set; }

    [SugarColumn(ColumnName = "paid_time", IsNullable = true)]
    public DateTime? PaidTime { get; set; }

    [SugarColumn(ColumnName = "notify_raw", ColumnDataType = "longtext", IsNullable = true)]
    public string? NotifyRaw { get; set; }
}
