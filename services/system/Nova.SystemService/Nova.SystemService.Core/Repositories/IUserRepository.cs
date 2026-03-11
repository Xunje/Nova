using Nova.Shared.SqlSugar.Abstractions;
using Nova.SystemService.Core.Entities;

namespace Nova.SystemService.Core.Repositories;

/// <summary>
/// 用户仓储接口
/// <para>继承通用仓储，仅声明用户域特有的数据访问能力</para>
/// </summary>
public interface IUserRepository : INovaRepository<UserEntity>
{
    Task<bool> ExistsByUserNameOrEmailAsync(string userName, string email);
}
