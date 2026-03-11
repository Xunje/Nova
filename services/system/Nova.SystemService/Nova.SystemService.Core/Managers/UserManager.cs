using Nova.Shared.MultiTenancy;
using Nova.SystemService.Core.Dtos;
using Nova.SystemService.Core.Entities;
using Nova.SystemService.Core.Repositories;
using Nova.SystemService.Core.Security;
using Volo.Abp;
using Volo.Abp.DependencyInjection;

namespace Nova.SystemService.Core.Managers;

/// <summary>
/// 用户写侧管理器
/// <para>封装用户创建、删除等写侧业务规则</para>
/// </summary>
public class UserManager : ITransientDependency
{
    private readonly IUserRepository _userRepository;
    private readonly TenantEntityInterceptor _tenantInterceptor;
    private readonly UserPasswordHasher _passwordHasher;

    public UserManager(
        IUserRepository userRepository,
        TenantEntityInterceptor tenantInterceptor,
        UserPasswordHasher passwordHasher)
    {
        _userRepository = userRepository;
        _tenantInterceptor = tenantInterceptor;
        _passwordHasher = passwordHasher;
    }

    /// <summary>创建用户（校验用户名/邮箱唯一性，密码哈希）</summary>
    public async Task<UserEntity> CreateAsync(CreateUserInput input)
    {
        var exists = await _userRepository.ExistsByUserNameOrEmailAsync(input.UserName, input.Email);
        if (exists)
            throw new UserFriendlyException("用户名或邮箱已存在");

        var user = new UserEntity
        {
            UserName = input.UserName,
            Email = input.Email,
            Phone = input.Phone
        };

        user.SetPassword(_passwordHasher.HashPassword(input.Password));
        _tenantInterceptor.FillTenantId(user);

        return await _userRepository.InsertReturnEntityAsync(user);
    }

    /// <summary>删除用户（软删除）</summary>
    public async Task DeleteAsync(long id)
    {
        var user = await _userRepository.GetByIdAsync(id);
        if (user == null)
            throw new UserFriendlyException("用户不存在");

        await _userRepository.SoftDeleteAsync(id);
    }
}
