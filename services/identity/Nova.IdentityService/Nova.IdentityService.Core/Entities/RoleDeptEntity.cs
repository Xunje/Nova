using Nova.Shared.MultiTenancy;
using SqlSugar;

namespace Nova.IdentityService.Core.Entities;

/// <summary>
/// 角色-部门关联表（数据权限：自定义时指定可访问的部门）
/// </summary>
[SugarTable("sys_role_dept")]
public class RoleDeptEntity : EntityBase
{
    [SugarColumn(ColumnName = "role_id")]
    public long RoleId { get; set; }

    [SugarColumn(ColumnName = "dept_id")]
    public long DeptId { get; set; }
}
