using Nova.Shared.SqlSugar;
using Nova.SystemService.Core.Entities;
using Nova.SystemService.Core.Repositories;
using SqlSugar;
using Volo.Abp.DependencyInjection;

namespace Nova.SystemService.Host.Repositories;

/// <summary>
/// 用户仓储 SqlSugar 实现
/// <para>继承通用仓储，只补充用户域特有查询</para>
/// </summary>
public class UserRepository : NovaRepository<UserEntity>, IUserRepository, ITransientDependency
{
    public UserRepository(ISqlSugarClient db) : base(db)
    {
    }

    public Task<bool> ExistsByUserNameOrEmailAsync(string userName, string email)
    {
        return IsAnyAsync(u => u.UserName == userName || u.Email == email);
    }
}
