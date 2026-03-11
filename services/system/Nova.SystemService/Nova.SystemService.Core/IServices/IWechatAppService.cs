using Nova.SystemService.Core.Dtos.Wechat;

namespace Nova.SystemService.Core.IServices;

public interface IWechatAppService
{
    Task<string> GetMpTokenAsync();
    Task<long?> SendTemplateAsync(SendOfficialAccountTemplateMessageInput input);
    Task SendKfTextAsync(SendOfficialAccountCustomMessageInput input);
    Task<WechatPayPrepayDto> JsapiPayAsync(CreateWechatJsapiPayInput input);
    Task<WechatPayCallbackResultDto> NotifyPayAsync(WechatPayCallbackHeaders headers, string body);
    string VerifyMpAsync(WechatOfficialAccountCallbackQuery query);
    Task<WechatOfficialAccountMessageResultDto> NotifyMpAsync(WechatOfficialAccountCallbackQuery query, string body);
}
