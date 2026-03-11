using Nova.IdentityService.Core.Dtos.Accounts;

namespace Nova.IdentityService.Core.IServices;

public interface IAccountAppService
{
    Task<LoginOutputDto> LoginAsync(LoginInput input);
    Task<WechatLoginDto> MiniLoginAsync(WechatMiniProgramLoginInput input);
    Task<WechatLoginDto> MpLoginAsync(WechatOfficialAccountLoginInput input);
    Task<CurrentUserAccessDto> GetCurrentAsync();
}
