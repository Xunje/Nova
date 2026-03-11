using Mapster;
using Microsoft.AspNetCore.Authorization;
using Nova.Shared.EventBus.Events;
using Nova.Shared.Hosting.Permissions;
using Nova.Shared.MultiTenancy;
using Nova.SystemService.Core.Managers;
using Nova.SystemService.Core.Dtos;
using Nova.SystemService.Core.Queries;
using Nova.SystemService.Core.IServices;
using Volo.Abp.Application.Services;
using Volo.Abp.EventBus.Distributed;
using Volo.Abp.MultiTenancy;

namespace Nova.SystemService.Application.Users;

/// <summary>
/// 用户应用服务
/// <para>管理用户的CRUD操作</para>
/// <para>继承EntityBase，自动受多租户隔离</para>
/// </summary>
public class UserAppService : ApplicationService, IUserAppService
{
    private readonly UserManager _userManager;
    private readonly IUserQueryService _userQueryService;
    private readonly ICurrentTenant _currentTenant;
    private readonly IDistributedEventBus _eventBus;

    public UserAppService(
        UserManager userManager,
        IUserQueryService userQueryService,
        ICurrentTenant currentTenant,
        IDistributedEventBus eventBus)
    {
        _userManager = userManager;
        _userQueryService = userQueryService;
        _currentTenant = currentTenant;
        _eventBus = eventBus;
    }

    /// <summary>
    /// 创建用户
    /// <para>创建后发布用户创建事件，通知其他服务</para>
    /// </summary>
    [Authorize(NovaPermissions.UserAdd)]
    public async Task<UserDto> CreateAsync(CreateUserInput input)
    {
        var user = await _userManager.CreateAsync(input);

        // 发布用户创建集成事件，通知其他服务
        await _eventBus.PublishAsync(new UserCreatedIntegrationEvent
        {
            TenantId = _currentTenant.Id,
            UserId = user.Id,
            UserName = user.UserName,
            Email = user.Email
        });

        return user.Adapt<UserDto>();
    }

    /// <summary>
    /// 根据ID获取用户
    /// </summary>
    [Authorize(NovaPermissions.UserGet)]
    public async Task<UserDto> GetAsync(long id)
    {
        return await _userQueryService.GetAsync(id);
    }

    /// <summary>
    /// 分页查询用户列表
    /// <para>TenantId过滤已由SqlSugar全局过滤器自动处理</para>
    /// </summary>
    [Authorize(NovaPermissions.UserList)]
    public async Task<PageResultDto<UserDto>> GetListAsync(GetUserListInput input)
    {
        return await _userQueryService.GetListAsync(input);
    }

    /// <summary>
    /// 删除用户（软删除）
    /// </summary>
    [Authorize(NovaPermissions.UserDelete)]
    public async Task DeleteAsync(long id)
    {
        await _userManager.DeleteAsync(id);
    }
}
