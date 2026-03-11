using Nova.IdentityService.Core.Dtos.Accounts;

namespace Nova.IdentityService.Core.IServices;

public interface IAccountAppService
{
    Task<LoginOutputDto> LoginAsync(LoginInput input);
    Task<WechatLoginDto> LoginByMiniProgramAsync(WechatMiniProgramLoginInput input);
    Task<WechatLoginDto> LoginByOfficialAccountAsync(WechatOfficialAccountLoginInput input);
    Task<CurrentUserAccessDto> GetCurrentAsync();
}
