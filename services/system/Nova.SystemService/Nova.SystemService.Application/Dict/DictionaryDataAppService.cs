using Microsoft.AspNetCore.Authorization;
using Mapster;
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
/// 字典数据应用服务
/// <para>管理字典项（如：男、女；启用、禁用等）</para>
/// </summary>
public class DictionaryDataAppService : ApplicationService, IDictionaryDataAppService
{
    private readonly INovaRepository<DictionaryDataEntity> _repository;
    private readonly ISqlSugarClient _db;
    private readonly ICurrentTenant _currentTenant;

    public DictionaryDataAppService(INovaRepository<DictionaryDataEntity> repository, ISqlSugarClient db, ICurrentTenant currentTenant)
    {
        _repository = repository;
        _db = db;
        _currentTenant = currentTenant;
    }

    /// <summary>创建字典数据</summary>
    [Authorize(NovaPermissions.DictAdd)]
    public async Task<DictionaryDataDto> CreateAsync(CreateDictionaryDataInput input)
    {
        var entity = input.Adapt<DictionaryDataEntity>();
        entity.TenantId = _currentTenant.Id;
        entity.CreateTime = DateTime.UtcNow;
        var created = await _repository.InsertReturnEntityAsync(entity);
        return created.Adapt<DictionaryDataDto>();
    }

    /// <summary>根据 ID 获取字典数据</summary>
    [Authorize(NovaPermissions.DictGet)]
    public async Task<DictionaryDataDto> GetAsync(long id)
    {
        var entity = await _repository.GetByIdAsync(id);
        if (entity == null) throw new UserFriendlyException("字典数据不存在");
        return entity.Adapt<DictionaryDataDto>();
    }

    /// <summary>分页查询字典数据列表</summary>
    [Authorize(NovaPermissions.DictList)]
    public async Task<PageResultDto<DictionaryDataDto>> GetListAsync(GetDictionaryDataListInput input)
    {
        var total = new RefAsync<int>();
        var list = await _repository.Queryable
            .WhereIF(!string.IsNullOrWhiteSpace(input.DictType), x => x.DictType == input.DictType)
            .WhereIF(!string.IsNullOrWhiteSpace(input.Keyword),
                x => x.DictLabel.Contains(input.Keyword!) || x.DictValue.Contains(input.Keyword!))
            .OrderBy(x => x.OrderNum)
            .ToPageListAsync(input.PageIndex, input.PageSize, total);
        return new PageResultDto<DictionaryDataDto>(total.Value, list.Adapt<List<DictionaryDataDto>>());
    }

    /// <summary>根据字典类型获取字典项列表（匿名可访问，用于下拉等）</summary>
    [AllowAnonymous]
    public async Task<List<DictionaryDataDto>> GetByDictTypeAsync(string dictType)
    {
        var list = await _db.Queryable<DictionaryDataEntity>()
            .Where(x => x.DictType == dictType && x.Status && !x.IsDeleted)
            .OrderBy(x => x.OrderNum)
            .ToListAsync();
        return list.Adapt<List<DictionaryDataDto>>();
    }

    /// <summary>更新字典数据</summary>
    [Authorize(NovaPermissions.DictEdit)]
    public async Task<DictionaryDataDto> UpdateAsync(long id, UpdateDictionaryDataInput input)
    {
        var entity = await _repository.GetByIdAsync(id);
        if (entity == null) throw new UserFriendlyException("字典数据不存在");

        input.Adapt(entity);
        entity.UpdateTime = DateTime.UtcNow;
        await _repository.UpdateAsync(entity);
        return entity.Adapt<DictionaryDataDto>();
    }

    /// <summary>删除字典数据（软删除）</summary>
    [Authorize(NovaPermissions.DictDelete)]
    public async Task DeleteAsync(long id)
    {
        await _repository.SoftDeleteAsync(id);
    }
}
