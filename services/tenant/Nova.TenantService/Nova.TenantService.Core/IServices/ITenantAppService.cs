using Nova.Shared.MultiTenancy;
using Nova.TenantService.Core.Dtos;

namespace Nova.TenantService.Core.IServices;

public interface ITenantAppService
{
    Task<TenantDto> CreateAsync(CreateTenantInput input);
    Task<TenantDto> GetAsync(Guid id);
    Task<PageResultDto<TenantDto>> GetListAsync(GetTenantListInput input);
    Task SetConnectionStringAsync(Guid id, UpdateTenantConnectionStringInput input);
    Task DeleteAsync(Guid id);
}
