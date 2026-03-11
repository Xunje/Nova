namespace Nova.SystemService.Core.Dtos.Wechat;

public class SendOfficialAccountTemplateMessageInput
{
    public string AccessToken { get; set; } = string.Empty;
    public string TemplateId { get; set; } = string.Empty;
    public string ToUserOpenId { get; set; } = string.Empty;
    public string? Url { get; set; }
    public Dictionary<string, WechatTemplateDataItemInput> Data { get; set; } = new();
}

public class WechatTemplateDataItemInput
{
    public string Value { get; set; } = string.Empty;
    public string? Color { get; set; }
}

public class SendOfficialAccountCustomMessageInput
{
    public string AccessToken { get; set; } = string.Empty;
    public string ToUserOpenId { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
}

public class CreateWechatJsapiPayInput
{
    public string AppId { get; set; } = string.Empty;
    public string OpenId { get; set; } = string.Empty;
    public string OutTradeNumber { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int Total { get; set; }
}

public class WechatPayPrepayDto
{
    public string PrepayId { get; set; } = string.Empty;
    public IDictionary<string, string> PayParameters { get; set; } = new Dictionary<string, string>();
}
