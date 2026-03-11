using Nova.Shared.MultiTenancy;
using SqlSugar;

namespace Nova.IdentityService.Core.Entities;

/// <summary>
/// 用户-岗位关联表
/// </summary>
[SugarTable("sys_user_post")]
public class UserPostEntity : EntityBase
{
    [SugarColumn(ColumnName = "user_id")]
    public long UserId { get; set; }

    [SugarColumn(ColumnName = "post_id")]
    public long PostId { get; set; }
}
