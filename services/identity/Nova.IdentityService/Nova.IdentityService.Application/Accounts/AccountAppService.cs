using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using SKIT.FlurlHttpClient.Wechat.Api;
using SKIT.FlurlHttpClient.Wechat.Api.Models;
using Nova.IdentityService.Application.Domain;
using Nova.IdentityService.Application.Security;
using Nova.IdentityService.Core.Dtos.Accounts;
using Nova.IdentityService.Core.IServices;
using Nova.IdentityService.Core.Enums;
using Nova.SystemService.Core.Consts;
using Nova.SystemService.Core.Dtos;
using Nova.SystemService.Core.Entities;
using Nova.SystemService.Core.Managers;
using Nova.SystemService.Core.Security;
using SqlSugar;
using Volo.Abp;
using Volo.Abp.Application.Services;
using Volo.Abp.MultiTenancy;
using Volo.Abp.Security.Claims;
using Nova.IdentityService.Core.Wechat;

namespace Nova.IdentityService.Application.Accounts;

/// <summary>
/// 账户应用服务
/// <para>提供登录、获取当前用户信息等认证相关接口</para>
/// </summary>
public class AccountAppService : ApplicationService, IAccountAppService
{
    private readonly ISqlSugarClient _db;
    private readonly ICurrentTenant _currentTenant;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly UserPasswordHasher _passwordHasher;
    private readonly JwtTokenService _jwtTokenService;
    private readonly ILoginLogService _loginLogService;
    private readonly AccountManager _accountManager;
    private readonly UserManager _userManager;
    private readonly WechatOptions _wechatOptions;

    public AccountAppService(
        ISqlSugarClient db,
        ICurrentTenant currentTenant,
        IHttpContextAccessor httpContextAccessor,
        UserPasswordHasher passwordHasher,
        JwtTokenService jwtTokenService,
        ILoginLogService loginLogService,
        AccountManager accountManager,
        UserManager userManager,
        IOptions<WechatOptions> wechatOptions)
    {
        _db = db;
        _currentTenant = currentTenant;
        _httpContextAccessor = httpContextAccessor;
        _passwordHasher = passwordHasher;
        _jwtTokenService = jwtTokenService;
        _loginLogService = loginLogService;
        _accountManager = accountManager;
        _userManager = userManager;
        _wechatOptions = wechatOptions.Value;
    }

    /// <summary>
    /// 用户登录，验证用户名密码后返回 JWT 及当前用户信息
    /// </summary>
    /// <param name="input">登录参数（用户名、密码）</param>
    /// <returns>访问令牌、刷新令牌及当前用户权限信息</returns>
    [AllowAnonymous]
    public async Task<LoginOutputDto> LoginAsync(LoginInput input)
    {
        // 按租户过滤用户（多租户场景）
        var userQuery = _db.Queryable<UserEntity>()
            .Where(u => !u.IsDeleted && u.UserName == input.UserName);

        if (_currentTenant.Id.HasValue)
            userQuery = userQuery.Where(u => u.TenantId == _currentTenant.Id);

        var user = await userQuery.FirstAsync();
        // 用户不存在或已禁用时记录失败日志并抛出友好异常
        if (user == null || user.Status != UserStatus.Active)
        {
            await _loginLogService.RecordAsync(input.UserName, LoginStatus.Fail, "用户不存在或已禁用");
            throw new UserFriendlyException("用户名或密码错误");
        }

        if (!_accountManager.VerifyPassword(user, input.Password))
        {
            await _loginLogService.RecordAsync(input.UserName, LoginStatus.Fail, "密码错误");
            throw new UserFriendlyException("用户名或密码错误");
        }

        await _loginLogService.RecordAsync(user.UserName, LoginStatus.Success);

        // 构建当前用户权限信息（角色、权限码、菜单树）
        var current = await _accountManager.BuildCurrentUserAccessAsync(user);
        var accessToken = _jwtTokenService.CreateAccessToken(user, current);
        var refreshToken = _jwtTokenService.CreateRefreshToken(user);

        return new LoginOutputDto
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            CurrentUser = current
        };
    }

    [AllowAnonymous]
    [HttpPost("login-by-mini-program")]
    public async Task<WechatLoginDto> LoginByMiniProgramAsync(WechatMiniProgramLoginInput input)
    {
        if (string.IsNullOrWhiteSpace(_wechatOptions.MiniProgram.AppId) || string.IsNullOrWhiteSpace(_wechatOptions.MiniProgram.AppSecret))
            throw new UserFriendlyException("未配置微信小程序参数");

        var client = WechatApiClientBuilder.Create(
            new WechatApiClientOptions
            {
                AppId = _wechatOptions.MiniProgram.AppId,
                AppSecret = _wechatOptions.MiniProgram.AppSecret
            }).Build();

        var response = await client.ExecuteSnsJsCode2SessionAsync(new SnsJsCode2SessionRequest
        {
            GrantType = "authorization_code",
            JsCode = input.Code
        });

        if (response.IsSuccessful() == false)
            throw new UserFriendlyException($"微信小程序登录失败：{response.ErrorCode}-{response.ErrorMessage}");

        var loginResult = await BuildWechatLoginResultAsync(
            appType: "MiniProgram",
            appId: _wechatOptions.MiniProgram.AppId,
            openId: response.OpenId ?? string.Empty,
            unionId: response.UnionId);
        loginResult.SessionKey = response.SessionKey;
        return loginResult;
    }

    [AllowAnonymous]
    [HttpPost("login-by-official-account")]
    public async Task<WechatLoginDto> LoginByOfficialAccountAsync(WechatOfficialAccountLoginInput input)
    {
        if (string.IsNullOrWhiteSpace(_wechatOptions.OfficialAccount.AppId) || string.IsNullOrWhiteSpace(_wechatOptions.OfficialAccount.AppSecret))
            throw new UserFriendlyException("未配置微信公众号参数");

        var client = WechatApiClientBuilder.Create(
            new WechatApiClientOptions
            {
                AppId = _wechatOptions.OfficialAccount.AppId,
                AppSecret = _wechatOptions.OfficialAccount.AppSecret
            }).Build();

        var response = await client.ExecuteSnsOAuth2AccessTokenAsync(new SnsOAuth2AccessTokenRequest
        {
            Code = input.Code,
            GrantType = "authorization_code"
        });

        if (response.IsSuccessful() == false)
            throw new UserFriendlyException($"微信公众号登录失败：{response.ErrorCode}-{response.ErrorMessage}");

        var loginResult = await BuildWechatLoginResultAsync(
            appType: "OfficialAccount",
            appId: _wechatOptions.OfficialAccount.AppId,
            openId: response.OpenId ?? string.Empty,
            unionId: response.UnionId);
        loginResult.WechatAccessToken = response.AccessToken;
        loginResult.WechatRefreshToken = response.RefreshToken;
        loginResult.ExpiresIn = response.ExpiresIn;
        loginResult.Scope = response.Scope;
        return loginResult;
    }

    private async Task<WechatLoginDto> BuildWechatLoginResultAsync(string appType, string appId, string openId, string? unionId)
    {
        var binding = await _db.Queryable<WechatUserBindingEntity>()
            .Where(x => !x.IsDeleted && x.WechatAppType == appType && x.AppId == appId && x.OpenId == openId)
            .FirstAsync();

        var isAutoRegistered = false;
        UserEntity user;
        if (binding == null && !string.IsNullOrWhiteSpace(unionId))
        {
            binding = await _db.Queryable<WechatUserBindingEntity>()
                .Where(x => !x.IsDeleted && x.UnionId == unionId)
                .FirstAsync();
        }

        if (binding == null)
        {
            isAutoRegistered = true;
            user = await _userManager.CreateAsync(new CreateUserInput
            {
                UserName = $"wx_{appType.ToLowerInvariant()}_{Guid.NewGuid():N}"[..24],
                Email = $"wx_{Guid.NewGuid():N}@nova.local",
                Password = $"Wx@{Guid.NewGuid():N}",
                Phone = null
            });

            binding = await _db.Insertable(new WechatUserBindingEntity
            {
                UserId = user.Id,
                WechatAppType = appType,
                AppId = appId,
                OpenId = openId,
                UnionId = unionId,
                LastLoginTime = DateTime.UtcNow,
                TenantId = user.TenantId
            }).ExecuteReturnEntityAsync();
        }
        else
        {
            user = await _db.Queryable<UserEntity>().InSingleAsync(binding.UserId);
            if (user == null || user.IsDeleted || user.Status != UserStatus.Active)
                throw new UserFriendlyException("绑定用户不存在或已禁用");

            binding.UnionId ??= unionId;
            binding.LastLoginTime = DateTime.UtcNow;
            await _db.Updateable(binding).ExecuteCommandAsync();
        }

        var current = await _accountManager.BuildCurrentUserAccessAsync(user);
        return new WechatLoginDto
        {
            OpenId = openId,
            UnionId = unionId,
            AccessToken = _jwtTokenService.CreateAccessToken(user, current),
            RefreshToken = _jwtTokenService.CreateRefreshToken(user),
            CurrentUser = current,
            IsAutoRegistered = isAutoRegistered
        };
    }

    /// <summary>
    /// 获取当前登录用户的权限信息（角色、权限码、菜单树）
    /// </summary>
    [Authorize]
    public async Task<CurrentUserAccessDto> GetCurrentAsync()
    {
        // 从 Claims 中解析用户 ID
        var userIdClaim = CurrentUser.FindClaim(AbpClaimTypes.UserId)?.Value
            ?? _httpContextAccessor.HttpContext?.User.FindFirst(AbpClaimTypes.UserId)?.Value;

        if (!long.TryParse(userIdClaim, out var userId))
            throw new UserFriendlyException("无效的登录状态");

        var user = await _db.Queryable<UserEntity>().InSingleAsync(userId);
        if (user == null || user.IsDeleted)
            throw new UserFriendlyException("用户不存在");

        return await _accountManager.BuildCurrentUserAccessAsync(user);
    }

}
