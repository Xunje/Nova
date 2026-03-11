using Nova.Shared.MultiTenancy;
using Nova.SystemService.Core.Dtos;

namespace Nova.SystemService.Core.Queries;

/// <summary>
/// 用户读侧查询服务
/// <para>负责列表、详情等读模型查询</para>
/// </summary>
public interface IUserQueryService
{
    Task<UserDto> GetAsync(long id);

    Task<PageResultDto<UserDto>> GetListAsync(GetUserListInput input);
}
