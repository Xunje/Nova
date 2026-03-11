using Nova.Shared.MultiTenancy;
using Nova.SystemService.Core.Consts;
using SqlSugar;

namespace Nova.SystemService.Core.Entities;

/// <summary>
/// 用户实体
/// <para>继承EntityBase，自动具备多租户隔离能力</para>
/// <para>查询时自动添加TenantId过滤，插入时自动填充TenantId</para>
/// </summary>
[SugarTable("sys_user")]
public class UserEntity : EntityBase
{
    /// <summary>
    /// 用户名（登录名）
    /// </summary>
    [SugarColumn(ColumnName = "user_name", Length = 50)]
    public string UserName { get; set; } = string.Empty;

    /// <summary>
    /// 邮箱地址
    /// </summary>
    [SugarColumn(ColumnName = "email", Length = 100)]
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// 手机号码
    /// </summary>
    [SugarColumn(ColumnName = "phone", Length = 20, IsNullable = true)]
    public string? Phone { get; set; }

    /// <summary>
    /// 部门ID（用于数据权限：本部门/本部门及下级/仅本人）
    /// </summary>
    [SugarColumn(ColumnName = "dept_id", IsNullable = true)]
    public long? DeptId { get; set; }

    /// <summary>
    /// 用户状态
    /// </summary>
    [SugarColumn(ColumnName = "status")]
    public UserStatus Status { get; set; } = UserStatus.Active;

    /// <summary>
    /// 密码哈希值（只读，通过SetPassword设置）
    /// </summary>
    [SugarColumn(ColumnName = "password_hash", Length = 256)]
    public string PasswordHash { get; private set; } = string.Empty;

    /// <summary>
    /// 设置密码哈希
    /// <para>密码应在应用层进行哈希处理后再传入</para>
    /// </summary>
    /// <param name="hash">密码哈希值</param>
    public void SetPassword(string hash) => PasswordHash = hash;
}
