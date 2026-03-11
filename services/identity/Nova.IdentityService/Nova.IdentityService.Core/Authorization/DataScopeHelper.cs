namespace Nova.IdentityService.Core.Authorization;

/// <summary>
/// 数据权限范围说明
/// <para>用于 RBAC 数据权限过滤，与 RoleEntity.DataScope 配合使用</para>
/// <para>All=全部 | Custom=自定义部门(RoleDept) | Dept=本部门 | DeptWithChildren=本部门及下级 | Self=仅本人</para>
/// </summary>
public static class DataScopeHelper
{
    /// <summary>
    /// 全部数据：无过滤条件
    /// </summary>
    public static bool IsAll(Enums.DataScopeType scope) => scope == Enums.DataScopeType.All;

    /// <summary>
    /// 自定义：仅能访问 RoleDept 关联的部门数据
    /// </summary>
    public static bool IsCustom(Enums.DataScopeType scope) => scope == Enums.DataScopeType.Custom;

    /// <summary>
    /// 本部门：仅能访问用户 DeptId 所在部门的数据
    /// </summary>
    public static bool IsDept(Enums.DataScopeType scope) => scope == Enums.DataScopeType.Dept;

    /// <summary>
    /// 本部门及下级：能访问用户部门及其子部门的数据
    /// </summary>
    public static bool IsDeptWithChildren(Enums.DataScopeType scope) => scope == Enums.DataScopeType.DeptWithChildren;

    /// <summary>
    /// 仅本人：仅能访问当前用户自己的数据
    /// </summary>
    public static bool IsSelf(Enums.DataScopeType scope) => scope == Enums.DataScopeType.Self;
}
