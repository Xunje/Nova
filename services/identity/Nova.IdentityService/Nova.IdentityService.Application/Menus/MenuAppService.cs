using Mapster;
using Microsoft.AspNetCore.Authorization;
using Nova.IdentityService.Core.Dtos.Menus;
using Nova.IdentityService.Core.Entities;
using Nova.IdentityService.Core.IServices;
using Nova.Shared.Hosting.Permissions;
using Nova.Shared.SqlSugar.Abstractions;
using Volo.Abp.Application.Services;
using Volo.Abp.MultiTenancy;

namespace Nova.IdentityService.Application.Menus;

/// <summary>
/// 菜单应用服务
/// <para>管理 RBAC 菜单的增删改查</para>
/// </summary>
public class MenuAppService : ApplicationService, IMenuAppService
{
    private readonly INovaRepository<MenuEntity> _menuRepository;
    private readonly ICurrentTenant _currentTenant;

    public MenuAppService(INovaRepository<MenuEntity> menuRepository, ICurrentTenant currentTenant)
    {
        _menuRepository = menuRepository;
        _currentTenant = currentTenant;
    }

    /// <summary>创建菜单</summary>
    [Authorize(NovaPermissions.MenuAdd)]
    public async Task<MenuDto> CreateAsync(CreateMenuInput input)
    {
        var entity = input.Adapt<MenuEntity>();
        entity.TenantId = _currentTenant.Id;
        entity.CreateTime = DateTime.UtcNow;
        var created = await _menuRepository.InsertReturnEntityAsync(entity);
        return created.Adapt<MenuDto>();
    }

    /// <summary>获取菜单列表（按排序号）</summary>
    [Authorize(NovaPermissions.MenuList)]
    public async Task<List<MenuDto>> GetListAsync()
    {
        var list = await _menuRepository.Queryable
            .OrderBy(menu => menu.OrderNum)
            .ToListAsync();

        return list.Adapt<List<MenuDto>>();
    }
}
