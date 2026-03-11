using Microsoft.Extensions.Logging;
using Nova.IdentityService.Core.Consts;
using Nova.IdentityService.Core.Entities;
using Nova.IdentityService.Core.Enums;
using Nova.Shared.MultiTenancy;
using Nova.SystemService.Core.Entities;
using SqlSugar;
using Volo.Abp.DependencyInjection;

namespace Nova.IdentityService.Application.Seeds;

/// <summary>
/// Identity RBAC 种子数据服务
/// <para>负责初始化角色、菜单、部门、岗位及关联关系</para>
/// </summary>
public class IdentityRbacSeedService : ITransientDependency
{
    private readonly ISqlSugarClient _db;
    private readonly ILogger<IdentityRbacSeedService>? _logger;

    public IdentityRbacSeedService(ISqlSugarClient db, ILogger<IdentityRbacSeedService>? logger = null)
    {
        _db = db;
        _logger = logger;
    }

    /// <summary>
    /// 执行 RBAC 种子数据初始化（角色、菜单、部门、岗位、关联）
    /// </summary>
    public async Task SeedAsync()
    {
        var adminRole = await EnsureRoleAsync("管理员", IdentityRbacSeedConstants.AdminRoleCode, 999, "系统管理员");
        await EnsureRoleAsync("普通角色", IdentityRbacSeedConstants.CommonRoleCode, 1, "普通用户默认角色");

        var menuIds = await EnsureMenusAsync();
        await EnsureAdminRoleMenusAsync(adminRole.Id, menuIds);
        await EnsureAdminUserRoleAsync(adminRole.Id);

        // RBAC 第二阶段：部门、岗位、角色-部门、用户-岗位（容错：单步失败不影响启动）
        // try
        // {
        var deptIds = await EnsureDeptsAsync();
        var postIds = await EnsurePostsAsync();
        await EnsureAdminRoleDeptsAsync(adminRole.Id, deptIds);
        await EnsureAdminUserPostAsync(postIds);
        if (deptIds.Count > 0)
            await EnsureAdminUserDeptAsync(deptIds[0]);
        // }
        // catch (Exception ex)
        // {
        //     _logger?.LogWarning(ex, "RBAC 第二阶段种子数据执行失败，应用将继续启动");
        // }
    }

    private async Task<RoleEntity> EnsureRoleAsync(string roleName, string roleCode, int orderNum, string remark)
    {
        var role = await _db.Queryable<RoleEntity>().FirstAsync(item => item.RoleCode == roleCode);
        if (role != null)
            return role;

        var entity = new RoleEntity
        {
            RoleName = roleName,
            RoleCode = roleCode,
            DataScope = DataScopeType.All,
            Status = true,
            OrderNum = orderNum,
            Remark = remark,
            TenantId = null,
            CreateTime = DateTime.UtcNow
        };

        return await _db.Insertable(entity).ExecuteReturnEntityAsync();
    }

    private async Task<List<long>> EnsureMenusAsync()
    {
        var ids = new List<long>();
        var root = await EnsureMenuAsync(new MenuEntity
        {
            MenuName = IdentityRbacSeedConstants.SystemRootMenuName,
            ParentId = 0,
            MenuType = MenuType.Directory,
            Router = "/system",
            RouterName = "System",
            Component = "Layout",
            Icon = "system",
            IsVisible = true,
            Status = true,
            OrderNum = 100,
            TenantId = null,
            CreateTime = DateTime.UtcNow
        });
        ids.Add(root.Id);

        ids.Add((await EnsureMenuAsync(new MenuEntity
        {
            MenuName = "用户管理",
            ParentId = root.Id,
            MenuType = MenuType.Menu,
            Router = "users",
            RouterName = "Users",
            Component = "system/user/index",
            Icon = "user",
            PermissionCode = "system:user:list",
            IsVisible = true,
            Status = true,
            OrderNum = 100,
            TenantId = null,
            CreateTime = DateTime.UtcNow
        })).Id);

        ids.Add((await EnsureMenuAsync(new MenuEntity { MenuName = "新增用户", ParentId = root.Id, MenuType = MenuType.Button, PermissionCode = "system:user:add", OrderNum = 99, IsVisible = false, Status = true, TenantId = null, CreateTime = DateTime.UtcNow })).Id);
        ids.Add((await EnsureMenuAsync(new MenuEntity { MenuName = "查看用户", ParentId = root.Id, MenuType = MenuType.Button, PermissionCode = "system:user:get", OrderNum = 98, IsVisible = false, Status = true, TenantId = null, CreateTime = DateTime.UtcNow })).Id);
        ids.Add((await EnsureMenuAsync(new MenuEntity { MenuName = "删除用户", ParentId = root.Id, MenuType = MenuType.Button, PermissionCode = "system:user:delete", OrderNum = 97, IsVisible = false, Status = true, TenantId = null, CreateTime = DateTime.UtcNow })).Id);

        ids.Add((await EnsureMenuAsync(new MenuEntity
        {
            MenuName = "租户管理",
            ParentId = root.Id,
            MenuType = MenuType.Menu,
            Router = "tenants",
            RouterName = "Tenants",
            Component = "system/tenant/index",
            Icon = "tenant",
            PermissionCode = "system:tenant:list",
            IsVisible = true,
            Status = true,
            OrderNum = 96,
            TenantId = null,
            CreateTime = DateTime.UtcNow
        })).Id);

        ids.Add((await EnsureMenuAsync(new MenuEntity { MenuName = "新增租户", ParentId = root.Id, MenuType = MenuType.Button, PermissionCode = "system:tenant:add", OrderNum = 95, IsVisible = false, Status = true, TenantId = null, CreateTime = DateTime.UtcNow })).Id);
        ids.Add((await EnsureMenuAsync(new MenuEntity { MenuName = "查看租户", ParentId = root.Id, MenuType = MenuType.Button, PermissionCode = "system:tenant:get", OrderNum = 94, IsVisible = false, Status = true, TenantId = null, CreateTime = DateTime.UtcNow })).Id);
        ids.Add((await EnsureMenuAsync(new MenuEntity { MenuName = "删除租户", ParentId = root.Id, MenuType = MenuType.Button, PermissionCode = "system:tenant:delete", OrderNum = 93, IsVisible = false, Status = true, TenantId = null, CreateTime = DateTime.UtcNow })).Id);
        ids.Add((await EnsureMenuAsync(new MenuEntity { MenuName = "设置租户连接", ParentId = root.Id, MenuType = MenuType.Button, PermissionCode = "system:tenant:set-connection", OrderNum = 92, IsVisible = false, Status = true, TenantId = null, CreateTime = DateTime.UtcNow })).Id);

        ids.Add((await EnsureMenuAsync(new MenuEntity
        {
            MenuName = "角色管理",
            ParentId = root.Id,
            MenuType = MenuType.Menu,
            Router = "roles",
            RouterName = "Roles",
            Component = "system/role/index",
            Icon = "role",
            PermissionCode = "system:role:list",
            IsVisible = true,
            Status = true,
            OrderNum = 91,
            TenantId = null,
            CreateTime = DateTime.UtcNow
        })).Id);
        ids.Add((await EnsureMenuAsync(new MenuEntity { MenuName = "新增角色", ParentId = root.Id, MenuType = MenuType.Button, PermissionCode = "system:role:add", OrderNum = 90, IsVisible = false, Status = true, TenantId = null, CreateTime = DateTime.UtcNow })).Id);
        ids.Add((await EnsureMenuAsync(new MenuEntity { MenuName = "查看角色", ParentId = root.Id, MenuType = MenuType.Button, PermissionCode = "system:role:get", OrderNum = 89, IsVisible = false, Status = true, TenantId = null, CreateTime = DateTime.UtcNow })).Id);
        ids.Add((await EnsureMenuAsync(new MenuEntity { MenuName = "角色授权菜单", ParentId = root.Id, MenuType = MenuType.Button, PermissionCode = "system:role:grant-menu", OrderNum = 88, IsVisible = false, Status = true, TenantId = null, CreateTime = DateTime.UtcNow })).Id);

        ids.Add((await EnsureMenuAsync(new MenuEntity
        {
            MenuName = "菜单管理",
            ParentId = root.Id,
            MenuType = MenuType.Menu,
            Router = "menus",
            RouterName = "Menus",
            Component = "system/menu/index",
            Icon = "menu",
            PermissionCode = "system:menu:list",
            IsVisible = true,
            Status = true,
            OrderNum = 87,
            TenantId = null,
            CreateTime = DateTime.UtcNow
        })).Id);
        ids.Add((await EnsureMenuAsync(new MenuEntity { MenuName = "新增菜单", ParentId = root.Id, MenuType = MenuType.Button, PermissionCode = "system:menu:add", OrderNum = 86, IsVisible = false, Status = true, TenantId = null, CreateTime = DateTime.UtcNow })).Id);

        ids.Add((await EnsureMenuAsync(new MenuEntity
        {
            MenuName = "部门管理",
            ParentId = root.Id,
            MenuType = MenuType.Menu,
            Router = "depts",
            RouterName = "Depts",
            Component = "system/dept/index",
            Icon = "dept",
            PermissionCode = "system:dept:list",
            IsVisible = true,
            Status = true,
            OrderNum = 85,
            TenantId = null,
            CreateTime = DateTime.UtcNow
        })).Id);
        ids.Add((await EnsureMenuAsync(new MenuEntity { MenuName = "新增部门", ParentId = root.Id, MenuType = MenuType.Button, PermissionCode = "system:dept:add", OrderNum = 84, IsVisible = false, Status = true, TenantId = null, CreateTime = DateTime.UtcNow })).Id);
        ids.Add((await EnsureMenuAsync(new MenuEntity { MenuName = "查看部门", ParentId = root.Id, MenuType = MenuType.Button, PermissionCode = "system:dept:get", OrderNum = 83, IsVisible = false, Status = true, TenantId = null, CreateTime = DateTime.UtcNow })).Id);

        ids.Add((await EnsureMenuAsync(new MenuEntity
        {
            MenuName = "岗位管理",
            ParentId = root.Id,
            MenuType = MenuType.Menu,
            Router = "posts",
            RouterName = "Posts",
            Component = "system/post/index",
            Icon = "post",
            PermissionCode = "system:post:list",
            IsVisible = true,
            Status = true,
            OrderNum = 82,
            TenantId = null,
            CreateTime = DateTime.UtcNow
        })).Id);
        ids.Add((await EnsureMenuAsync(new MenuEntity { MenuName = "新增岗位", ParentId = root.Id, MenuType = MenuType.Button, PermissionCode = "system:post:add", OrderNum = 81, IsVisible = false, Status = true, TenantId = null, CreateTime = DateTime.UtcNow })).Id);
        ids.Add((await EnsureMenuAsync(new MenuEntity { MenuName = "查看岗位", ParentId = root.Id, MenuType = MenuType.Button, PermissionCode = "system:post:get", OrderNum = 80, IsVisible = false, Status = true, TenantId = null, CreateTime = DateTime.UtcNow })).Id);

        // 系统监控 - 操作日志、登录日志
        var monitorRoot = await EnsureMenuAsync(new MenuEntity
        {
            MenuName = "系统监控",
            ParentId = root.Id,
            MenuType = MenuType.Directory,
            Router = "/monitor",
            RouterName = "Monitor",
            Component = "Layout",
            Icon = "monitor",
            IsVisible = true,
            Status = true,
            OrderNum = 79,
            TenantId = null,
            CreateTime = DateTime.UtcNow
        });
        ids.Add(monitorRoot.Id);

        ids.Add((await EnsureMenuAsync(new MenuEntity
        {
            MenuName = "操作日志",
            ParentId = monitorRoot.Id,
            MenuType = MenuType.Menu,
            Router = "operlog",
            RouterName = "OperLog",
            Component = "monitor/operlog/index",
            Icon = "form",
            PermissionCode = "monitor:operlog:list",
            IsVisible = true,
            Status = true,
            OrderNum = 100,
            TenantId = null,
            CreateTime = DateTime.UtcNow
        })).Id);

        ids.Add((await EnsureMenuAsync(new MenuEntity
        {
            MenuName = "登录日志",
            ParentId = monitorRoot.Id,
            MenuType = MenuType.Menu,
            Router = "loginlog",
            RouterName = "LoginLog",
            Component = "monitor/loginlog/index",
            Icon = "logininfor",
            PermissionCode = "monitor:loginlog:list",
            IsVisible = true,
            Status = true,
            OrderNum = 99,
            TenantId = null,
            CreateTime = DateTime.UtcNow
        })).Id);

        // 字典管理
        ids.Add((await EnsureMenuAsync(new MenuEntity
        {
            MenuName = "字典管理",
            ParentId = root.Id,
            MenuType = MenuType.Menu,
            Router = "dict",
            RouterName = "Dict",
            Component = "system/dict/index",
            Icon = "dict",
            PermissionCode = "system:dict:list",
            IsVisible = true,
            Status = true,
            OrderNum = 78,
            TenantId = null,
            CreateTime = DateTime.UtcNow
        })).Id);
        ids.Add((await EnsureMenuAsync(new MenuEntity { MenuName = "字典查询", ParentId = root.Id, MenuType = MenuType.Button, PermissionCode = "system:dict:list", OrderNum = 77, IsVisible = false, Status = true, TenantId = null, CreateTime = DateTime.UtcNow })).Id);
        ids.Add((await EnsureMenuAsync(new MenuEntity { MenuName = "字典新增", ParentId = root.Id, MenuType = MenuType.Button, PermissionCode = "system:dict:add", OrderNum = 76, IsVisible = false, Status = true, TenantId = null, CreateTime = DateTime.UtcNow })).Id);
        ids.Add((await EnsureMenuAsync(new MenuEntity { MenuName = "字典修改", ParentId = root.Id, MenuType = MenuType.Button, PermissionCode = "system:dict:edit", OrderNum = 75, IsVisible = false, Status = true, TenantId = null, CreateTime = DateTime.UtcNow })).Id);
        ids.Add((await EnsureMenuAsync(new MenuEntity { MenuName = "字典删除", ParentId = root.Id, MenuType = MenuType.Button, PermissionCode = "system:dict:delete", OrderNum = 74, IsVisible = false, Status = true, TenantId = null, CreateTime = DateTime.UtcNow })).Id);

        // 参数设置
        ids.Add((await EnsureMenuAsync(new MenuEntity
        {
            MenuName = "参数设置",
            ParentId = root.Id,
            MenuType = MenuType.Menu,
            Router = "config",
            RouterName = "Config",
            Component = "system/config/index",
            Icon = "edit",
            PermissionCode = "system:config:list",
            IsVisible = true,
            Status = true,
            OrderNum = 73,
            TenantId = null,
            CreateTime = DateTime.UtcNow
        })).Id);
        ids.Add((await EnsureMenuAsync(new MenuEntity { MenuName = "参数查询", ParentId = root.Id, MenuType = MenuType.Button, PermissionCode = "system:config:list", OrderNum = 72, IsVisible = false, Status = true, TenantId = null, CreateTime = DateTime.UtcNow })).Id);
        ids.Add((await EnsureMenuAsync(new MenuEntity { MenuName = "参数新增", ParentId = root.Id, MenuType = MenuType.Button, PermissionCode = "system:config:add", OrderNum = 71, IsVisible = false, Status = true, TenantId = null, CreateTime = DateTime.UtcNow })).Id);
        ids.Add((await EnsureMenuAsync(new MenuEntity { MenuName = "参数修改", ParentId = root.Id, MenuType = MenuType.Button, PermissionCode = "system:config:edit", OrderNum = 70, IsVisible = false, Status = true, TenantId = null, CreateTime = DateTime.UtcNow })).Id);
        ids.Add((await EnsureMenuAsync(new MenuEntity { MenuName = "参数删除", ParentId = root.Id, MenuType = MenuType.Button, PermissionCode = "system:config:delete", OrderNum = 69, IsVisible = false, Status = true, TenantId = null, CreateTime = DateTime.UtcNow })).Id);

        // 通知公告
        ids.Add((await EnsureMenuAsync(new MenuEntity
        {
            MenuName = "通知公告",
            ParentId = root.Id,
            MenuType = MenuType.Menu,
            Router = "notice",
            RouterName = "Notice",
            Component = "system/notice/index",
            Icon = "message",
            PermissionCode = "system:notice:list",
            IsVisible = true,
            Status = true,
            OrderNum = 68,
            TenantId = null,
            CreateTime = DateTime.UtcNow
        })).Id);
        ids.Add((await EnsureMenuAsync(new MenuEntity { MenuName = "通知查询", ParentId = root.Id, MenuType = MenuType.Button, PermissionCode = "system:notice:list", OrderNum = 67, IsVisible = false, Status = true, TenantId = null, CreateTime = DateTime.UtcNow })).Id);
        ids.Add((await EnsureMenuAsync(new MenuEntity { MenuName = "通知新增", ParentId = root.Id, MenuType = MenuType.Button, PermissionCode = "system:notice:add", OrderNum = 66, IsVisible = false, Status = true, TenantId = null, CreateTime = DateTime.UtcNow })).Id);
        ids.Add((await EnsureMenuAsync(new MenuEntity { MenuName = "通知修改", ParentId = root.Id, MenuType = MenuType.Button, PermissionCode = "system:notice:edit", OrderNum = 65, IsVisible = false, Status = true, TenantId = null, CreateTime = DateTime.UtcNow })).Id);
        ids.Add((await EnsureMenuAsync(new MenuEntity { MenuName = "通知删除", ParentId = root.Id, MenuType = MenuType.Button, PermissionCode = "system:notice:delete", OrderNum = 64, IsVisible = false, Status = true, TenantId = null, CreateTime = DateTime.UtcNow })).Id);

        return ids;
    }

    private async Task<MenuEntity> EnsureMenuAsync(MenuEntity menu)
    {
        var existing = await _db.Queryable<MenuEntity>()
            .FirstAsync(item => item.MenuName == menu.MenuName && item.ParentId == menu.ParentId);
        if (existing != null)
            return existing;

        return await _db.Insertable(menu).ExecuteReturnEntityAsync();
    }

    private async Task EnsureAdminRoleMenusAsync(long roleId, List<long> menuIds)
    {
        var existingMenuIds = await _db.Queryable<RoleMenuEntity>()
            .Where(link => link.RoleId == roleId)
            .Select(link => link.MenuId)
            .ToListAsync();

        var toInsert = menuIds.Except(existingMenuIds)
            .Select(menuId => new RoleMenuEntity
            {
                RoleId = roleId,
                MenuId = menuId,
                TenantId = null,
                CreateTime = DateTime.UtcNow
            })
            .ToList();

        if (toInsert.Count > 0)
            await _db.Insertable(toInsert).ExecuteCommandAsync();
    }

    private async Task EnsureAdminUserRoleAsync(long roleId)
    {
        if (!_db.DbMaintenance.IsAnyTable("sys_user", false))
            return;

        var adminUser = await _db.Queryable<UserEntity>()
            .FirstAsync(user => user.UserName == NovaSeedConstants.DefaultAdminUserName && !user.IsDeleted);
        if (adminUser == null)
            return;

        var exists = await _db.Queryable<UserRoleEntity>()
            .AnyAsync(link => link.UserId == adminUser.Id && link.RoleId == roleId);
        if (exists)
            return;

        await _db.Insertable(new UserRoleEntity
        {
            UserId = adminUser.Id,
            RoleId = roleId,
            TenantId = adminUser.TenantId,
            CreateTime = DateTime.UtcNow
        }).ExecuteCommandAsync();
    }

    /// <summary>
    /// 确保部门种子数据（树形结构）
    /// </summary>
    private async Task<List<long>> EnsureDeptsAsync()
    {
        var root = await EnsureDeptAsync("Nova科技", "Nova", 0, "Nova", 100);
        var sz = await EnsureDeptAsync("深圳总公司", "SZ", root.Id, null, 100);
        var jx = await EnsureDeptAsync("江西总公司", "JX", root.Id, null, 100);
        await EnsureDeptAsync("研发部门", "YF", sz.Id, null, 100);
        await EnsureDeptAsync("市场部门", "SC", sz.Id, null, 100);
        await EnsureDeptAsync("测试部门", "CS", sz.Id, null, 100);
        await EnsureDeptAsync("财务部门", "CW", sz.Id, null, 100);
        await EnsureDeptAsync("运维部门", "YW", sz.Id, null, 100);
        await EnsureDeptAsync("市场部门", "SC2", jx.Id, null, 100);
        await EnsureDeptAsync("财务部门", "CW2", jx.Id, null, 100);

        return new List<long> { root.Id, sz.Id, jx.Id };
    }

    private async Task<DeptEntity> EnsureDeptAsync(string deptName, string deptCode, long parentId, string? leader, int orderNum)
    {
        var existing = await _db.Queryable<DeptEntity>()
            .FirstAsync(d => d.DeptCode == deptCode && d.ParentId == parentId);
        if (existing != null)
            return existing;

        var entity = new DeptEntity
        {
            DeptName = deptName,
            DeptCode = deptCode,
            ParentId = parentId,
            Leader = leader,
            OrderNum = orderNum,
            Status = true,
            TenantId = null,
            CreateTime = DateTime.UtcNow
        };
        return await _db.Insertable(entity).ExecuteReturnEntityAsync();
    }

    /// <summary>
    /// 确保岗位种子数据
    /// </summary>
    private async Task<List<long>> EnsurePostsAsync()
    {
        var posts = new[]
        {
            ("ceo", "董事长", 100),
            ("se", "项目经理", 100),
            ("hr", "人力资源", 100),
            ("user", "普通员工", 100)
        };

        var ids = new List<long>();
        foreach (var (code, name, order) in posts)
        {
            var post = await EnsurePostAsync(code, name, order);
            ids.Add(post.Id);
        }
        return ids;
    }

    private async Task<PostEntity> EnsurePostAsync(string postCode, string postName, int orderNum)
    {
        var existing = await _db.Queryable<PostEntity>()
            .FirstAsync(p => p.PostCode == postCode);
        if (existing != null)
            return existing;

        var entity = new PostEntity
        {
            PostCode = postCode,
            PostName = postName,
            OrderNum = orderNum,
            Status = true,
            TenantId = null,
            CreateTime = DateTime.UtcNow
        };
        return await _db.Insertable(entity).ExecuteReturnEntityAsync();
    }

    /// <summary>
    /// 管理员角色关联部门（DataScope=Custom 时生效，此处预置全部部门）
    /// </summary>
    private async Task EnsureAdminRoleDeptsAsync(long roleId, List<long> deptIds)
    {
        var existing = await _db.Queryable<RoleDeptEntity>()
            .Where(rd => rd.RoleId == roleId)
            .Select(rd => rd.DeptId)
            .ToListAsync();

        var toInsert = deptIds.Except(existing)
            .Select(deptId => new RoleDeptEntity
            {
                RoleId = roleId,
                DeptId = deptId,
                TenantId = null,
                CreateTime = DateTime.UtcNow
            })
            .ToList();

        if (toInsert.Count > 0)
            await _db.Insertable(toInsert).ExecuteCommandAsync();
    }

    /// <summary>
    /// 管理员用户关联岗位（董事长）
    /// </summary>
    private async Task EnsureAdminUserPostAsync(List<long> postIds)
    {
        if (!_db.DbMaintenance.IsAnyTable("sys_user", false))
            return;

        var adminUser = await _db.Queryable<UserEntity>()
            .FirstAsync(u => u.UserName == NovaSeedConstants.DefaultAdminUserName && !u.IsDeleted);
        if (adminUser == null)
            return;

        var ceoPostId = postIds.First(); // 董事长
        var exists = await _db.Queryable<UserPostEntity>()
            .AnyAsync(up => up.UserId == adminUser.Id && up.PostId == ceoPostId);
        if (exists)
            return;

        await _db.Insertable(new UserPostEntity
        {
            UserId = adminUser.Id,
            PostId = ceoPostId,
            TenantId = adminUser.TenantId,
            CreateTime = DateTime.UtcNow
        }).ExecuteCommandAsync();
    }

    /// <summary>
    /// 管理员用户关联部门（根部门）
    /// </summary>
    private async Task EnsureAdminUserDeptAsync(long rootDeptId)
    {
        if (!_db.DbMaintenance.IsAnyTable("sys_user", false))
            return;

        var adminUser = await _db.Queryable<UserEntity>()
            .FirstAsync(u => u.UserName == NovaSeedConstants.DefaultAdminUserName && !u.IsDeleted);
        if (adminUser == null)
            return;

        if (adminUser.DeptId.HasValue)
            return;

        await _db.Updateable<UserEntity>()
            .SetColumns(u => u.DeptId == rootDeptId)
            .Where(u => u.Id == adminUser.Id)
            .ExecuteCommandAsync();
    }
}
