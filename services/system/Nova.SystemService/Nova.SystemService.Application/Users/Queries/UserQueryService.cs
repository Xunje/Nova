using Mapster;
using Nova.Shared.MultiTenancy;
using Nova.Shared.SqlSugar.Abstractions;
using Nova.SystemService.Core.Dtos;
using Nova.SystemService.Core.Entities;
using Nova.SystemService.Core.Queries;
using SqlSugar;
using Volo.Abp;
using Volo.Abp.DependencyInjection;

namespace Nova.SystemService.Application.Users.Queries;

/// <summary>
/// 用户读侧查询服务实现
/// <para>通过通用仓储的 Queryable 接口完成列表和详情投影</para>
/// </summary>
public class UserQueryService : IUserQueryService, ITransientDependency
{
    private readonly INovaRepository<UserEntity> _repository;

    public UserQueryService(INovaRepository<UserEntity> repository)
    {
        _repository = repository;
    }

    /// <summary>根据 ID 获取用户详情</summary>
    public async Task<UserDto> GetAsync(long id)
    {
        var user = await _repository.GetByIdAsync(id);
        if (user == null)
            throw new UserFriendlyException("用户不存在");

        return user.Adapt<UserDto>();
    }

    /// <summary>分页查询用户列表（支持关键词搜索）</summary>
    public async Task<PageResultDto<UserDto>> GetListAsync(GetUserListInput input)
    {
        var total = new RefAsync<int>();
        var list = await _repository.Queryable
            .WhereIF(!string.IsNullOrWhiteSpace(input.KeyWord),
                u => u.UserName.Contains(input.KeyWord!) || u.Email.Contains(input.KeyWord!))
            .OrderByDescending(u => u.CreateTime)
            .ToPageListAsync(input.PageIndex, input.PageSize, total);

        return new PageResultDto<UserDto>(total.Value, list.Adapt<List<UserDto>>());
    }
}
