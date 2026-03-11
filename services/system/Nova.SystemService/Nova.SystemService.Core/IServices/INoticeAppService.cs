using Nova.Shared.MultiTenancy;
using Nova.SystemService.Core.Dtos.Notice;

namespace Nova.SystemService.Core.IServices;

public interface INoticeAppService
{
    Task<NoticeDto> CreateAsync(CreateNoticeInput input);
    Task<NoticeDto> GetAsync(long id);
    Task<PageResultDto<NoticeDto>> GetListAsync(GetNoticeListInput input);
    Task<NoticeDto> UpdateAsync(long id, UpdateNoticeInput input);
    Task DeleteAsync(long id);
}

public class CreateNoticeInput
{
    public string Title { get; set; } = string.Empty;
    public Core.Enums.NoticeType NoticeType { get; set; }
    public string Content { get; set; } = string.Empty;
    public bool Status { get; set; } = true;
    public int OrderNum { get; set; }
}

public class UpdateNoticeInput : CreateNoticeInput { }

public class GetNoticeListInput
{
    public string? Keyword { get; set; }
    public int PageIndex { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}
