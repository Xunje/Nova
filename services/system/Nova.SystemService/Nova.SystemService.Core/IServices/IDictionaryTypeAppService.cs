using Nova.Shared.MultiTenancy;
using Nova.SystemService.Core.Dtos.Dict;

namespace Nova.SystemService.Core.IServices;

public interface IDictionaryTypeAppService
{
    Task<DictionaryTypeDto> CreateAsync(CreateDictionaryTypeInput input);
    Task<DictionaryTypeDto> GetAsync(long id);
    Task<PageResultDto<DictionaryTypeDto>> GetListAsync(GetDictionaryTypeListInput input);
    Task<DictionaryTypeDto> UpdateAsync(long id, UpdateDictionaryTypeInput input);
    Task DeleteAsync(long id);
}

public class CreateDictionaryTypeInput
{
    public string DictName { get; set; } = string.Empty;
    public string DictType { get; set; } = string.Empty;
    public bool Status { get; set; } = true;
    public int OrderNum { get; set; }
    public string? Remark { get; set; }
}

public class UpdateDictionaryTypeInput : CreateDictionaryTypeInput { }

public class GetDictionaryTypeListInput
{
    public string? Keyword { get; set; }
    public int PageIndex { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}
