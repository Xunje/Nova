namespace Nova.IdentityService.Application.Wechat;

public class WechatOptions
{
    public MiniProgramOptions MiniProgram { get; set; } = new();
    public OfficialAccountOptions OfficialAccount { get; set; } = new();
}

public class MiniProgramOptions
{
    public string AppId { get; set; } = string.Empty;
    public string AppSecret { get; set; } = string.Empty;
}

public class OfficialAccountOptions
{
    public string AppId { get; set; } = string.Empty;
    public string AppSecret { get; set; } = string.Empty;
}
