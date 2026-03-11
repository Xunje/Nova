namespace Nova.IdentityService.Core.Dtos.Accounts;

public class LoginOutputDto
{
    public string AccessToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public CurrentUserAccessDto CurrentUser { get; set; } = new();
}
