using Nova.IdentityService.Core.Entities;
using Nova.Shared.SqlSugar.Abstractions;
using SqlSugar;
using Volo.Abp;
using Volo.Abp.Domain.Services;
using Volo.Abp.MultiTenancy;

namespace Nova.IdentityService.Application.Domain;

/// <summary>
/// 角色领域服务
/// <para>封装角色相关的业务逻辑</para>
/// </summary>
public class RoleManager : DomainService
{
    private readonly INovaRepository<RoleEntity> _roleRepository;
    private readonly ISqlSugarClient _db;
    private readonly ICurrentTenant _currentTenant;

    public RoleManager(
        INovaRepository<RoleEntity> roleRepository,
        ISqlSugarClient db,
        ICurrentTenant currentTenant)
    {
        _roleRepository = roleRepository;
        _db = db;
        _currentTenant = currentTenant;
    }

    /// <summary>
    /// 为角色分配菜单（先删除旧关联，再插入新关联）
    /// </summary>
    /// <param name="roleId">角色 ID</param>
    /// <param name="menuIds">菜单 ID 列表</param>
    public async Task AssignMenusAsync(long roleId, IReadOnlyList<long> menuIds)
    {
        var role = await _roleRepository.GetByIdAsync(roleId);
        if (role == null)
            throw new UserFriendlyException("角色不存在");

        // 删除该角色原有的所有菜单关联
        await _db.Deleteable<RoleMenuEntity>().Where(link => link.RoleId == roleId).ExecuteCommandAsync();

        if (menuIds == null || menuIds.Count == 0)
            return;

        // 批量插入新的角色-菜单关联
        var links = menuIds.Distinct()
            .Select(menuId => new RoleMenuEntity
            {
                RoleId = roleId,
                MenuId = menuId,
                TenantId = _currentTenant.Id,
                CreateTime = DateTime.UtcNow
            })
            .ToList();

        await _db.Insertable(links).ExecuteCommandAsync();
    }

    /// <summary>
    /// 检查角色编码是否已存在（用于新增/编辑时的唯一性校验）
    /// </summary>
    /// <param name="roleCode">角色编码</param>
    /// <param name="excludeId">排除的 ID（编辑时排除自身）</param>
    /// <returns>已存在返回 true</returns>
    public async Task<bool> IsRoleCodeExistsAsync(string roleCode, long? excludeId = null)
    {
        var query = _roleRepository.Queryable.Where(r => r.RoleCode == roleCode);
        if (excludeId.HasValue)
            query = query.Where(r => r.Id != excludeId.Value);
        return await query.AnyAsync();
    }
}
