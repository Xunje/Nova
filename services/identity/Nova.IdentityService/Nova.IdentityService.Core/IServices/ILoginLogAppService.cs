using Nova.IdentityService.Core.Dtos.Logs;
using Nova.Shared.MultiTenancy;

namespace Nova.IdentityService.Core.IServices;

public interface ILoginLogAppService
{
    Task<PageResultDto<LoginLogDto>> GetListAsync(GetLoginLogListInput input);
}
