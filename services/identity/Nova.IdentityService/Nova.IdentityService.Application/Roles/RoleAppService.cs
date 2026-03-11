using Mapster;
using Microsoft.AspNetCore.Authorization;
using Nova.IdentityService.Application.Domain;
using Nova.IdentityService.Core.Dtos.Roles;
using Nova.IdentityService.Core.Entities;
using Nova.IdentityService.Core.IServices;
using Nova.IdentityService.Core.OperLog;
using Nova.Shared.Hosting.Permissions;
using Nova.Shared.MultiTenancy;
using Nova.Shared.SqlSugar.Abstractions;
using SqlSugar;
using Volo.Abp;
using Volo.Abp.Application.Services;
using Volo.Abp.MultiTenancy;

namespace Nova.IdentityService.Application.Roles;

/// <summary>
/// 角色应用服务
/// <para>管理角色的 CRUD、权限分配（菜单）</para>
/// </summary>
public class RoleAppService : ApplicationService, IRoleAppService
{
    private readonly INovaRepository<RoleEntity> _roleRepository;
    private readonly ISqlSugarClient _db;
    private readonly ICurrentTenant _currentTenant;
    private readonly RoleManager _roleManager;

    public RoleAppService(INovaRepository<RoleEntity> roleRepository, ISqlSugarClient db, ICurrentTenant currentTenant, RoleManager roleManager)
    {
        _roleRepository = roleRepository;
        _db = db;
        _currentTenant = currentTenant;
        _roleManager = roleManager;
    }

    /// <summary>创建角色</summary>
    [Authorize(NovaPermissions.RoleAdd)]
    [OperLog("角色管理", Core.Enums.OperType.Insert)]
    public async Task<RoleDto> CreateAsync(CreateRoleInput input)
    {
        var exists = await _roleManager.IsRoleCodeExistsAsync(input.RoleCode);
        if (exists)
            throw new UserFriendlyException($"角色编码 {input.RoleCode} 已存在");

        var role = input.Adapt<RoleEntity>();
        role.TenantId = _currentTenant.Id;
        role.CreateTime = DateTime.UtcNow;
        var created = await _roleRepository.InsertReturnEntityAsync(role);
        return created.Adapt<RoleDto>();
    }

    /// <summary>根据 ID 获取角色</summary>
    [Authorize(NovaPermissions.RoleGet)]
    public async Task<RoleDto> GetAsync(long id)
    {
        var role = await _roleRepository.GetByIdAsync(id);
        if (role == null)
            throw new UserFriendlyException("角色不存在");
        return role.Adapt<RoleDto>();
    }

    /// <summary>分页查询角色列表</summary>
    [Authorize(NovaPermissions.RoleList)]
    public async Task<PageResultDto<RoleDto>> GetListAsync(GetRoleListInput input)
    {
        var total = new RefAsync<int>();
        var list = await _roleRepository.Queryable
            .WhereIF(!string.IsNullOrWhiteSpace(input.Keyword),
                role => role.RoleName.Contains(input.Keyword!) || role.RoleCode.Contains(input.Keyword!))
            .OrderBy(role => role.OrderNum)
            .ToPageListAsync(input.PageIndex, input.PageSize, total);

        return new PageResultDto<RoleDto>(total.Value, list.Adapt<List<RoleDto>>());
    }

    /// <summary>为角色分配菜单</summary>
    [Authorize(NovaPermissions.RoleGrantMenu)]
    [OperLog("角色授权", Core.Enums.OperType.Auth)]
    public async Task AssignMenusAsync(long roleId, AssignRoleMenusInput input)
    {
        await _roleManager.AssignMenusAsync(roleId, input.MenuIds);
    }
}
