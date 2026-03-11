using Nova.IdentityService.Core.Dtos.Logs;
using Nova.Shared.MultiTenancy;

namespace Nova.IdentityService.Core.IServices;

public interface IOperationLogAppService
{
    Task<PageResultDto<OperationLogDto>> GetListAsync(GetOperationLogListInput input);
}
