namespace Nova.SystemService.Core.Wechat;

public class WechatOptions
{
    public MiniProgramOptions MiniProgram { get; set; } = new();
    public OfficialAccountOptions OfficialAccount { get; set; } = new();
    public WechatPayOptions Pay { get; set; } = new();
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
    public string Token { get; set; } = string.Empty;
    public string EncodingAesKey { get; set; } = string.Empty;
    public string DefaultReplyMessage { get; set; } = "收到消息了";
    public bool ForwardToCustomService { get; set; }
}

public class WechatPayOptions
{
    public string MerchantId { get; set; } = string.Empty;
    public string MerchantV3Secret { get; set; } = string.Empty;
    public string MerchantCertificateSerialNumber { get; set; } = string.Empty;
    public string MerchantCertificatePrivateKey { get; set; } = string.Empty;
    public string PlatformCertificateSerialNumber { get; set; } = string.Empty;
    public string PlatformCertificatePublicKey { get; set; } = string.Empty;
    public string NotifyUrl { get; set; } = string.Empty;
}
