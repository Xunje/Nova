namespace Nova.SystemService.Core.Dtos;

public class GetUserListInput
{
    public string? KeyWord { get; set; }
    public int PageIndex { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}
