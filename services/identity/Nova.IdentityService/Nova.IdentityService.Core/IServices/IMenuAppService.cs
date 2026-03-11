using Nova.IdentityService.Core.Dtos.Menus;

namespace Nova.IdentityService.Core.IServices;

public interface IMenuAppService
{
    Task<MenuDto> CreateAsync(CreateMenuInput input);
    Task<List<MenuDto>> GetListAsync();
}
