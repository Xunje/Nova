using Nova.Shared.MultiTenancy;
using Nova.SystemService.Core.Dtos.Dict;

namespace Nova.SystemService.Core.IServices;

public interface IDictionaryDataAppService
{
    Task<DictionaryDataDto> CreateAsync(CreateDictionaryDataInput input);
    Task<DictionaryDataDto> GetAsync(long id);
    Task<PageResultDto<DictionaryDataDto>> GetListAsync(GetDictionaryDataListInput input);
    Task<List<DictionaryDataDto>> GetByDictTypeAsync(string dictType);
    Task<DictionaryDataDto> UpdateAsync(long id, UpdateDictionaryDataInput input);
    Task DeleteAsync(long id);
}

public class CreateDictionaryDataInput
{
    public string DictType { get; set; } = string.Empty;
    public string DictLabel { get; set; } = string.Empty;
    public string DictValue { get; set; } = string.Empty;
    public string? CssClass { get; set; }
    public string? ListClass { get; set; }
    public bool IsDefault { get; set; }
    public bool Status { get; set; } = true;
    public int OrderNum { get; set; }
    public string? Remark { get; set; }
}

public class UpdateDictionaryDataInput : CreateDictionaryDataInput { }

public class GetDictionaryDataListInput
{
    public string? DictType { get; set; }
    public string? Keyword { get; set; }
    public int PageIndex { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}
