using Nova.Shared.MultiTenancy;
using Nova.SystemService.Core.Dtos.Config;

namespace Nova.SystemService.Core.IServices;

public interface IConfigAppService
{
    Task<ConfigDto> CreateAsync(CreateConfigInput input);
    Task<ConfigDto> GetAsync(long id);
    Task<string?> GetByKeyAsync(string configKey);
    Task<PageResultDto<ConfigDto>> GetListAsync(GetConfigListInput input);
    Task<ConfigDto> UpdateAsync(long id, UpdateConfigInput input);
    Task DeleteAsync(long id);
}

public class CreateConfigInput
{
    public string ConfigName { get; set; } = string.Empty;
    public string ConfigKey { get; set; } = string.Empty;
    public string ConfigValue { get; set; } = string.Empty;
    public string? ConfigType { get; set; }
    public int OrderNum { get; set; }
    public string? Remark { get; set; }
}

public class UpdateConfigInput : CreateConfigInput { }

public class GetConfigListInput
{
    public string? Keyword { get; set; }
    public int PageIndex { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}
