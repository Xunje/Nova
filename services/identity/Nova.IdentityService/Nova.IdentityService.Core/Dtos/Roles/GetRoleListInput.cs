namespace Nova.IdentityService.Core.Dtos.Roles;

public class GetRoleListInput
{
    public string? Keyword { get; set; }
    public int PageIndex { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}
