namespace Nova.IdentityService.Core.Dtos.Accounts;

public class WechatMiniProgramLoginInput
{
    public string Code { get; set; } = string.Empty;
}

public class WechatOfficialAccountLoginInput
{
    public string Code { get; set; } = string.Empty;
}

public class WechatLoginDto : LoginOutputDto
{
    public string OpenId { get; set; } = string.Empty;
    public string? UnionId { get; set; }
    public string? SessionKey { get; set; }
    public string? WechatAccessToken { get; set; }
    public string? WechatRefreshToken { get; set; }
    public int? ExpiresIn { get; set; }
    public string? Scope { get; set; }
    public bool IsAutoRegistered { get; set; }
}
