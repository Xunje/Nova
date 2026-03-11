using Nova.IdentityService.Core.Consts;
using Nova.IdentityService.Core.Dtos.Accounts;
using Nova.Shared.MultiTenancy;
using Nova.IdentityService.Core.Dtos.Menus;
using Nova.IdentityService.Core.Entities;
using Nova.SystemService.Core.Entities;
using Nova.SystemService.Core.Security;
using SqlSugar;
using Volo.Abp;
using Volo.Abp.Domain.Services;
using Volo.Abp.MultiTenancy;

namespace Nova.IdentityService.Application.Domain;

/// <summary>
/// 账户领域服务
/// <para>封装登录、当前用户信息构建等业务逻辑</para>
/// </summary>
public class AccountManager : DomainService
{
    private readonly ISqlSugarClient _db;
    private readonly ICurrentTenant _currentTenant;
    private readonly UserPasswordHasher _passwordHasher;

    public AccountManager(
        ISqlSugarClient db,
        ICurrentTenant currentTenant,
        UserPasswordHasher passwordHasher)
    {
        _db = db;
        _currentTenant = currentTenant;
        _passwordHasher = passwordHasher;
    }

    /// <summary>
    /// 验证用户密码是否正确
    /// </summary>
    /// <param name="user">用户实体（含 PasswordHash）</param>
    /// <param name="password">明文密码</param>
    /// <returns>密码正确返回 true</returns>
    public bool VerifyPassword(UserEntity user, string password)
    {
        return _passwordHasher.VerifyPassword(password, user.PasswordHash);
    }

    /// <summary>
    /// 构建当前用户访问信息（角色、权限码、菜单树）
    /// </summary>
    /// <param name="user">用户实体</param>
    /// <returns>包含角色、权限、菜单树的 DTO</returns>
    public async Task<CurrentUserAccessDto> BuildCurrentUserAccessAsync(UserEntity user)
    {
        // 1. 查询用户关联的角色 ID
        var roleIds = await _db.Queryable<UserRoleEntity>()
            .Where(link => link.UserId == user.Id)
            .Select(link => link.RoleId)
            .ToListAsync();

        var roles = roleIds.Count == 0
            ? new List<RoleEntity>()
            : await _db.Queryable<RoleEntity>()
                .Where(role => roleIds.Contains(role.Id) && role.Status)
                .OrderBy(role => role.OrderNum)
                .ToListAsync();

        // 2. 管理员用户若未分配管理员角色，则自动补全
        if (string.Equals(user.UserName, NovaSeedConstants.DefaultAdminUserName, StringComparison.OrdinalIgnoreCase) &&
            roles.All(role => !string.Equals(role.RoleCode, IdentityRbacSeedConstants.AdminRoleCode, StringComparison.OrdinalIgnoreCase)))
        {
            var adminRole = await _db.Queryable<RoleEntity>()
                .FirstAsync(role => role.RoleCode == IdentityRbacSeedConstants.AdminRoleCode);
            if (adminRole != null)
                roles.Insert(0, adminRole);
        }

        // 3. 通过角色关联查询菜单 ID
        var roleCodeSet = roles.Select(role => role.RoleCode).Distinct(StringComparer.OrdinalIgnoreCase).ToList();
        var menuIds = roles.Count == 0
            ? new List<long>()
            : await _db.Queryable<RoleMenuEntity>()
                .Where(link => roles.Select(role => role.Id).Contains(link.RoleId))
                .Select(link => link.MenuId)
                .ToListAsync();

        var menus = menuIds.Count == 0
            ? new List<MenuEntity>()
            : await _db.Queryable<MenuEntity>()
                .Where(menu => menuIds.Contains(menu.Id) && menu.Status)
                .OrderBy(menu => menu.OrderNum)
                .ToListAsync();

        // 4. 从菜单中提取权限码
        var permissionCodes = menus
            .Where(menu => !string.IsNullOrWhiteSpace(menu.PermissionCode))
            .Select(menu => menu.PermissionCode!)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        // 5. 管理员用户自动拥有超级权限 *:*:*
        if (string.Equals(user.UserName, NovaSeedConstants.DefaultAdminUserName, StringComparison.OrdinalIgnoreCase) &&
            !permissionCodes.Contains("*:*:*", StringComparer.OrdinalIgnoreCase))
        {
            permissionCodes.Insert(0, "*:*:*");
        }

        return new CurrentUserAccessDto
        {
            UserId = user.Id,
            UserName = user.UserName,
            Email = user.Email,
            Phone = user.Phone,
            TenantId = user.TenantId,
            RoleCodes = roleCodeSet,
            PermissionCodes = permissionCodes,
            Menus = BuildMenuTree(menus.Where(menu => menu.MenuType != Core.Enums.MenuType.Button).ToList())
        };
    }

    /// <summary>
    /// 将扁平菜单列表构建为树形结构（按 ParentId 父子关系）
    /// </summary>
    private static List<MenuDto> BuildMenuTree(List<MenuEntity> menus)
    {
        var lookup = menus.ToDictionary(
            menu => menu.Id,
            menu => new MenuDto
            {
                Id = menu.Id,
                ParentId = menu.ParentId,
                MenuName = menu.MenuName,
                PermissionCode = menu.PermissionCode,
                MenuType = menu.MenuType,
                RouterName = menu.RouterName,
                Router = menu.Router,
                Component = menu.Component,
                Icon = menu.Icon,
                IsLink = menu.IsLink,
                IsCache = menu.IsCache,
                IsVisible = menu.IsVisible,
                Status = menu.Status,
                OrderNum = menu.OrderNum
            });

        // 按 OrderNum、Id 排序后挂载到父节点
        var roots = new List<MenuDto>();
        foreach (var menu in lookup.Values.OrderBy(item => item.OrderNum).ThenBy(item => item.Id))
        {
            if (menu.ParentId == 0 || !lookup.TryGetValue(menu.ParentId, out var parent))
                roots.Add(menu);
            else
                parent.Children.Add(menu);
        }

        return roots;
    }
}
