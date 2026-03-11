using Nova.Shared.MultiTenancy;
using Nova.SystemService.Core.Dtos;

namespace Nova.SystemService.Core.IServices;

/// <summary>
/// 用户应用服务接口
/// <para>定义用户管理的核心业务操作</para>
/// </summary>
public interface IUserAppService
{
    /// <summary>
    /// 创建用户
    /// </summary>
    /// <param name="input">创建用户输入</param>
    /// <returns>创建的用户DTO</returns>
    Task<UserDto> CreateAsync(CreateUserInput input);

    /// <summary>
    /// 根据ID获取用户
    /// </summary>
    /// <param name="id">用户ID</param>
    /// <returns>用户DTO</returns>
    Task<UserDto> GetAsync(long id);

    /// <summary>
    /// 分页查询用户列表
    /// </summary>
    /// <param name="input">查询条件</param>
    /// <returns>分页结果</returns>
    Task<PageResultDto<UserDto>> GetListAsync(GetUserListInput input);

    /// <summary>
    /// 删除用户（软删除）
    /// </summary>
    /// <param name="id">用户ID</param>
    Task DeleteAsync(long id);
}
