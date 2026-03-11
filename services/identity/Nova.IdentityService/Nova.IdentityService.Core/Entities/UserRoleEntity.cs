using Nova.Shared.MultiTenancy;
using SqlSugar;

namespace Nova.IdentityService.Core.Entities;

[SugarTable("sys_user_role")]
public class UserRoleEntity : EntityBase
{
    [SugarColumn(ColumnName = "user_id")]
    public long UserId { get; set; }

    [SugarColumn(ColumnName = "role_id")]
    public long RoleId { get; set; }
}
