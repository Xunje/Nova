using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Caching.Distributed;
using Nova.Shared.EventBus.Events;
using Nova.Shared.Hosting.Permissions;
using Nova.Shared.MultiTenancy;
using Nova.TenantService.Core.Dtos;
using Nova.TenantService.Core.Entities;
using Nova.TenantService.Core.IServices;
using SqlSugar;
using Volo.Abp;
using Volo.Abp.Application.Services;
using Volo.Abp.EventBus.Distributed;

namespace Nova.TenantService.Application.Tenants;

/// <summary>
/// 租户应用服务
/// <para>管理租户的CRUD操作</para>
/// <para>注意：租户表属于平台级数据，不继承EntityBase，不受租户隔离过滤</para>
/// </summary>
public class TenantAppService : ApplicationService, ITenantAppService
{
    private readonly ISqlSugarClient _db;
    private readonly IDistributedEventBus _distributedEventBus;
    private readonly IDistributedCache _cache;

    public TenantAppService(
        ISqlSugarClient db,
        IDistributedEventBus distributedEventBus,
        IDistributedCache cache)
    {
        _db = db;
        _distributedEventBus = distributedEventBus;
        _cache = cache;
    }

    /// <summary>
    /// 创建租户
    /// <para>创建后会发布租户创建事件，通知其他服务进行初始化</para>
    /// </summary>
    [Authorize(NovaPermissions.TenantAdd)]
    public async Task<TenantDto> CreateAsync(CreateTenantInput input)
    {
        // 检查租户标识是否已存在
        var exists = await _db.Queryable<TenantEntity>()
            .AnyAsync(t => t.Code == input.Code && !t.IsDeleted);
        if (exists) throw new UserFriendlyException($"租户标识 {input.Code} 已存在");

        // 创建租户实体
        var tenant = input.Adapt<TenantEntity>();
        tenant.Id = Guid.NewGuid();
        await _db.Insertable(tenant).ExecuteCommandAsync();

        // 发布租户创建集成事件，通知其他服务初始化租户数据
        await _distributedEventBus.PublishAsync(new TenantCreatedIntegrationEvent
        {
            TenantId = tenant.Id,
            TenantCode = tenant.Code
        });

        // 清除租户连接缓存
        await _cache.RemoveAsync($"tenant:conn:{tenant.Id}");

        return tenant.Adapt<TenantDto>();
    }

    /// <summary>
    /// 根据ID获取租户
    /// </summary>
    [Authorize(NovaPermissions.TenantGet)]
    public async Task<TenantDto> GetAsync(Guid id)
    {
        var tenant = await _db.Queryable<TenantEntity>()
            .FirstAsync(t => t.Id == id && !t.IsDeleted);
        if (tenant == null) throw new UserFriendlyException("租户不存在");
        return tenant.Adapt<TenantDto>();
    }

    /// <summary>
    /// 分页查询租户列表
    /// </summary>
    [Authorize(NovaPermissions.TenantList)]
    public async Task<PageResultDto<TenantDto>> GetListAsync(GetTenantListInput input)
    {
        var total = new RefAsync<int>();
        var list = await _db.Queryable<TenantEntity>()
            .Where(t => !t.IsDeleted)
            .WhereIF(!string.IsNullOrWhiteSpace(input.KeyWord),
                t => t.Name.Contains(input.KeyWord!) || t.Code.Contains(input.KeyWord!))
            .OrderByDescending(t => t.CreateTime)
            .ToPageListAsync(input.PageIndex, input.PageSize, total);

        return new PageResultDto<TenantDto>(total.Value, list.Adapt<List<TenantDto>>());
    }

    /// <summary>
    /// 设置租户独立数据库连接字符串
    /// <para>为空时切回共享数据库 + TenantId 隔离模式</para>
    /// </summary>
    [Authorize(NovaPermissions.TenantSetConnection)]
    public async Task SetConnectionStringAsync(Guid id, UpdateTenantConnectionStringInput input)
    {
        var affected = await _db.Updateable<TenantEntity>()
            .SetColumns(t => t.ConnectionString == (string.IsNullOrWhiteSpace(input.ConnectionString) ? null : input.ConnectionString!.Trim()))
            .Where(t => t.Id == id && !t.IsDeleted)
            .ExecuteCommandAsync();

        if (affected == 0)
            throw new UserFriendlyException("租户不存在");

        await _cache.RemoveAsync($"tenant:conn:{id}");
    }

    /// <summary>
    /// 删除租户（软删除)
    /// <para>删除后清除租户连接缓存</para>
    /// </summary>
    [Authorize(NovaPermissions.TenantDelete)]
    public async Task DeleteAsync(Guid id)
    {
        await _db.Updateable<TenantEntity>()
            .SetColumns(t => t.IsDeleted == true)
            .Where(t => t.Id == id)
            .ExecuteCommandAsync();

        // 清除租户连接缓存
        await _cache.RemoveAsync($"tenant:conn:{id}");
    }
}
