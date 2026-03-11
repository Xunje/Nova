using Nova.Shared.MultiTenancy;
using Nova.SystemService.Core.Enums;
using SqlSugar;

namespace Nova.SystemService.Core.Entities;

/// <summary>
/// 通知公告表
/// </summary>
[SugarTable("sys_notice")]
public class NoticeEntity : EntityBase
{
    [SugarColumn(ColumnName = "title", Length = 200)]
    public string Title { get; set; } = string.Empty;

    [SugarColumn(ColumnName = "notice_type")]
    public NoticeType NoticeType { get; set; }

    [SugarColumn(ColumnName = "content", ColumnDataType = "text")]
    public string Content { get; set; } = string.Empty;

    [SugarColumn(ColumnName = "status")]
    public bool Status { get; set; } = true;

    [SugarColumn(ColumnName = "order_num")]
    public int OrderNum { get; set; }
}
