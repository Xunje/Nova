using Mapster;
using Microsoft.AspNetCore.Authorization;
using Nova.Shared.Hosting.Permissions;
using Nova.Shared.MultiTenancy;
using Nova.Shared.SqlSugar.Abstractions;
using Nova.SystemService.Core.Dtos.Config;
using Nova.SystemService.Core.Entities;
using Nova.SystemService.Core.IServices;
using SqlSugar;
using Volo.Abp;
using Volo.Abp.Application.Services;
using Volo.Abp.MultiTenancy;

namespace Nova.SystemService.Application.Config;

/// <summary>
/// 系统配置应用服务
/// <para>管理键值对形式的系统配置</para>
/// </summary>
public class ConfigAppService : ApplicationService, IConfigAppService
{
    private readonly INovaRepository<ConfigEntity> _repository;
    private readonly ISqlSugarClient _db;
    private readonly ICurrentTenant _currentTenant;

    public ConfigAppService(INovaRepository<ConfigEntity> repository, ISqlSugarClient db, ICurrentTenant currentTenant)
    {
        _repository = repository;
        _db = db;
        _currentTenant = currentTenant;
    }

    /// <summary>创建配置</summary>
    [Authorize(NovaPermissions.ConfigAdd)]
    public async Task<ConfigDto> CreateAsync(CreateConfigInput input)
    {
        var exists = await _repository.IsAnyAsync(x => x.ConfigKey == input.ConfigKey);
        if (exists) throw new UserFriendlyException($"配置键 {input.ConfigKey} 已存在");

        var entity = input.Adapt<ConfigEntity>();
        entity.TenantId = _currentTenant.Id;
        entity.CreateTime = DateTime.UtcNow;
        var created = await _repository.InsertReturnEntityAsync(entity);
        return created.Adapt<ConfigDto>();
    }

    /// <summary>根据 ID 获取配置</summary>
    [Authorize(NovaPermissions.ConfigGet)]
    public async Task<ConfigDto> GetAsync(long id)
    {
        var entity = await _repository.GetByIdAsync(id);
        if (entity == null) throw new UserFriendlyException("配置不存在");
        return entity.Adapt<ConfigDto>();
    }

    /// <summary>根据配置键获取配置值</summary>
    [Authorize(NovaPermissions.ConfigGet)]
    public async Task<string?> GetByKeyAsync(string configKey)
    {
        var entity = await _db.Queryable<ConfigEntity>()
            .Where(x => x.ConfigKey == configKey && !x.IsDeleted)
            .FirstAsync();
        return entity?.ConfigValue;
    }

    /// <summary>分页查询配置列表</summary>
    [Authorize(NovaPermissions.ConfigList)]
    public async Task<PageResultDto<ConfigDto>> GetListAsync(GetConfigListInput input)
    {
        var total = new RefAsync<int>();
        var list = await _repository.Queryable
            .WhereIF(!string.IsNullOrWhiteSpace(input.Keyword),
                x => x.ConfigName.Contains(input.Keyword!) || x.ConfigKey.Contains(input.Keyword!))
            .OrderBy(x => x.OrderNum)
            .ToPageListAsync(input.PageIndex, input.PageSize, total);
        return new PageResultDto<ConfigDto>(total.Value, list.Adapt<List<ConfigDto>>());
    }

    /// <summary>更新配置</summary>
    [Authorize(NovaPermissions.ConfigEdit)]
    public async Task<ConfigDto> UpdateAsync(long id, UpdateConfigInput input)
    {
        var entity = await _repository.GetByIdAsync(id);
        if (entity == null) throw new UserFriendlyException("配置不存在");

        input.Adapt(entity);
        entity.UpdateTime = DateTime.UtcNow;
        await _repository.UpdateAsync(entity);
        return entity.Adapt<ConfigDto>();
    }

    /// <summary>删除配置（软删除）</summary>
    [Authorize(NovaPermissions.ConfigDelete)]
    public async Task DeleteAsync(long id)
    {
        await _repository.SoftDeleteAsync(id);
    }
}
