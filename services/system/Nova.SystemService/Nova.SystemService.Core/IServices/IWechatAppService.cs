using Nova.SystemService.Core.Dtos.Wechat;

namespace Nova.SystemService.Core.IServices;

public interface IWechatAppService
{
    Task<string> GetOfficialAccountAccessTokenAsync();
    Task<long?> SendTemplateMessageAsync(SendOfficialAccountTemplateMessageInput input);
    Task SendCustomTextMessageAsync(SendOfficialAccountCustomMessageInput input);
    Task<WechatPayPrepayDto> CreateJsapiPayAsync(CreateWechatJsapiPayInput input);
    Task<WechatPayCallbackResultDto> HandlePayCallbackAsync(WechatPayCallbackHeaders headers, string body);
    string VerifyOfficialAccountUrlAsync(WechatOfficialAccountCallbackQuery query);
    Task<WechatOfficialAccountMessageResultDto> HandleOfficialAccountMessageAsync(WechatOfficialAccountCallbackQuery query, string body);
}
