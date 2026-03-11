using Mapster;
using Microsoft.AspNetCore.Authorization;
using Nova.Shared.Hosting.Permissions;
using Nova.Shared.MultiTenancy;
using Nova.Shared.SqlSugar.Abstractions;
using Nova.SystemService.Core.Dtos.Dict;
using Nova.SystemService.Core.Entities;
using Nova.SystemService.Core.IServices;
using SqlSugar;
using Volo.Abp;
using Volo.Abp.Application.Services;
using Volo.Abp.MultiTenancy;

namespace Nova.SystemService.Application.Dict;

/// <summary>
/// 字典类型应用服务
/// <para>管理字典分类（如：性别、状态等）</para>
/// </summary>
public class DictionaryTypeAppService : ApplicationService, IDictionaryTypeAppService
{
    private readonly INovaRepository<DictionaryTypeEntity> _repository;
    private readonly ISqlSugarClient _db;
    private readonly ICurrentTenant _currentTenant;

    public DictionaryTypeAppService(INovaRepository<DictionaryTypeEntity> repository, ISqlSugarClient db, ICurrentTenant currentTenant)
    {
        _repository = repository;
        _db = db;
        _currentTenant = currentTenant;
    }

    /// <summary>创建字典类型</summary>
    [Authorize(NovaPermissions.DictAdd)]
    public async Task<DictionaryTypeDto> CreateAsync(CreateDictionaryTypeInput input)
    {
        var exists = await _repository.IsAnyAsync(x => x.DictType == input.DictType);
        if (exists) throw new UserFriendlyException($"字典类型 {input.DictType} 已存在");

        var entity = input.Adapt<DictionaryTypeEntity>();
        entity.TenantId = _currentTenant.Id;
        entity.CreateTime = DateTime.UtcNow;
        var created = await _repository.InsertReturnEntityAsync(entity);
        return created.Adapt<DictionaryTypeDto>();
    }

    /// <summary>根据 ID 获取字典类型</summary>
    [Authorize(NovaPermissions.DictGet)]
    public async Task<DictionaryTypeDto> GetAsync(long id)
    {
        var entity = await _repository.GetByIdAsync(id);
        if (entity == null) throw new UserFriendlyException("字典类型不存在");
        return entity.Adapt<DictionaryTypeDto>();
    }

    /// <summary>分页查询字典类型列表</summary>
    [Authorize(NovaPermissions.DictList)]
    public async Task<PageResultDto<DictionaryTypeDto>> GetListAsync(GetDictionaryTypeListInput input)
    {
        var total = new RefAsync<int>();
        var list = await _repository.Queryable
            .WhereIF(!string.IsNullOrWhiteSpace(input.Keyword),
                x => x.DictName.Contains(input.Keyword!) || x.DictType.Contains(input.Keyword!))
            .OrderBy(x => x.OrderNum)
            .ToPageListAsync(input.PageIndex, input.PageSize, total);
        return new PageResultDto<DictionaryTypeDto>(total.Value, list.Adapt<List<DictionaryTypeDto>>());
    }

    /// <summary>更新字典类型</summary>
    [Authorize(NovaPermissions.DictEdit)]
    public async Task<DictionaryTypeDto> UpdateAsync(long id, UpdateDictionaryTypeInput input)
    {
        var entity = await _repository.GetByIdAsync(id);
        if (entity == null) throw new UserFriendlyException("字典类型不存在");

        input.Adapt(entity);
        entity.UpdateTime = DateTime.UtcNow;
        await _repository.UpdateAsync(entity);
        return entity.Adapt<DictionaryTypeDto>();
    }

    /// <summary>删除字典类型（软删除）</summary>
    [Authorize(NovaPermissions.DictDelete)]
    public async Task DeleteAsync(long id)
    {
        await _repository.SoftDeleteAsync(id);
    }
}
