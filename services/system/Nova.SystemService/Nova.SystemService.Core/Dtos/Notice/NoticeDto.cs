using Nova.SystemService.Core.Enums;

namespace Nova.SystemService.Core.Dtos.Notice;

public class NoticeDto
{
    public long Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public NoticeType NoticeType { get; set; }
    public string Content { get; set; } = string.Empty;
    public bool Status { get; set; }
    public int OrderNum { get; set; }
}
