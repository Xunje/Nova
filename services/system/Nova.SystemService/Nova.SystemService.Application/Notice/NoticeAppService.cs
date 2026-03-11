using Mapster;
using Microsoft.AspNetCore.Authorization;
using Nova.Shared.Hosting.Permissions;
using SqlSugar;
using Nova.Shared.MultiTenancy;
using Nova.Shared.SqlSugar.Abstractions;
using Nova.SystemService.Core.Dtos.Notice;
using Nova.SystemService.Core.Entities;
using Nova.SystemService.Core.IServices;
using Volo.Abp;
using Volo.Abp.Application.Services;
using Volo.Abp.MultiTenancy;

namespace Nova.SystemService.Application.Notice;

/// <summary>
/// 系统通知应用服务
/// <para>管理系统公告/通知的 CRUD</para>
/// </summary>
public class NoticeAppService : ApplicationService, INoticeAppService
{
    private readonly INovaRepository<NoticeEntity> _repository;
    private readonly ICurrentTenant _currentTenant;

    public NoticeAppService(INovaRepository<NoticeEntity> repository, ICurrentTenant currentTenant)
    {
        _repository = repository;
        _currentTenant = currentTenant;
    }

    /// <summary>创建通知</summary>
    [Authorize(NovaPermissions.NoticeAdd)]
    public async Task<NoticeDto> CreateAsync(CreateNoticeInput input)
    {
        var entity = input.Adapt<NoticeEntity>();
        entity.TenantId = _currentTenant.Id;
        entity.CreateTime = DateTime.UtcNow;
        var created = await _repository.InsertReturnEntityAsync(entity);
        return created.Adapt<NoticeDto>();
    }

    /// <summary>根据 ID 获取通知</summary>
    [Authorize(NovaPermissions.NoticeGet)]
    public async Task<NoticeDto> GetAsync(long id)
    {
        var entity = await _repository.GetByIdAsync(id);
        if (entity == null) throw new UserFriendlyException("通知不存在");
        return entity.Adapt<NoticeDto>();
    }

    /// <summary>分页查询通知列表</summary>
    [Authorize(NovaPermissions.NoticeList)]
    public async Task<PageResultDto<NoticeDto>> GetListAsync(GetNoticeListInput input)
    {
        var total = new RefAsync<int>();
        var list = await _repository.Queryable
            .WhereIF(!string.IsNullOrWhiteSpace(input.Keyword), x => x.Title.Contains(input.Keyword!))
            .OrderByDescending(x => x.CreateTime)
            .ToPageListAsync(input.PageIndex, input.PageSize, total);
        return new PageResultDto<NoticeDto>(total.Value, list.Adapt<List<NoticeDto>>());
    }

    /// <summary>更新通知</summary>
    [Authorize(NovaPermissions.NoticeEdit)]
    public async Task<NoticeDto> UpdateAsync(long id, UpdateNoticeInput input)
    {
        var entity = await _repository.GetByIdAsync(id);
        if (entity == null) throw new UserFriendlyException("通知不存在");

        input.Adapt(entity);
        entity.UpdateTime = DateTime.UtcNow;
        await _repository.UpdateAsync(entity);
        return entity.Adapt<NoticeDto>();
    }

    /// <summary>删除通知（软删除）</summary>
    [Authorize(NovaPermissions.NoticeDelete)]
    public async Task DeleteAsync(long id)
    {
        await _repository.SoftDeleteAsync(id);
    }
}
