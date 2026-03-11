namespace Nova.SystemService.Core.Dtos.Wechat;

public class WechatOfficialAccountCallbackQuery
{
    public string Signature { get; set; } = string.Empty;
    public string Timestamp { get; set; } = string.Empty;
    public string Nonce { get; set; } = string.Empty;
    public string? EchoStr { get; set; }
    public string? MsgSignature { get; set; }
    public string? OpenId { get; set; }
    public string? EncryptType { get; set; }
}

public class WechatPayCallbackHeaders
{
    public string Timestamp { get; set; } = string.Empty;
    public string Nonce { get; set; } = string.Empty;
    public string Signature { get; set; } = string.Empty;
    public string SerialNumber { get; set; } = string.Empty;
}

public class WechatPayCallbackResultDto
{
    public bool Success { get; set; }
    public string? EventType { get; set; }
    public string? OutTradeNumber { get; set; }
    public string? TransactionId { get; set; }
}

public class WechatOfficialAccountMessageResultDto
{
    public bool Success { get; set; }
    public string ResponseXml { get; set; } = "success";
    public string? FromUserName { get; set; }
    public string? ToUserName { get; set; }
    public string? MessageType { get; set; }
    public string? Content { get; set; }
}
