using Nova.IdentityService.Core.Dtos.Roles;
using Nova.Shared.MultiTenancy;

namespace Nova.IdentityService.Core.IServices;

public interface IRoleAppService
{
    Task<RoleDto> CreateAsync(CreateRoleInput input);
    Task<RoleDto> GetAsync(long id);
    Task<PageResultDto<RoleDto>> GetListAsync(GetRoleListInput input);
    Task AssignMenusAsync(long roleId, AssignRoleMenusInput input);
}
