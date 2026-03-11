namespace Nova.TenantService.Core.Dtos;

public class GetTenantListInput
{
    public string? KeyWord { get; set; }
    public int PageIndex { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}
