using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Nova.IdentityService.Core.Dtos.Accounts;
using Nova.Shared.Hosting.Security;
using Nova.SystemService.Core.Entities;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Security.Claims;

namespace Nova.IdentityService.Application.Security;

/// <summary>
/// JWT 令牌服务
/// <para>负责生成 AccessToken、RefreshToken，将用户信息与权限写入 Claims</para>
/// </summary>
public class JwtTokenService : ITransientDependency
{
    private readonly IConfiguration _configuration;

    public JwtTokenService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    /// <summary>
    /// 创建访问令牌（AccessToken），有效期 2 小时
    /// </summary>
    /// <param name="user">用户实体</param>
    /// <param name="access">当前用户权限信息（角色、权限码）</param>
    /// <returns>JWT 字符串</returns>
    public string CreateAccessToken(UserEntity user, CurrentUserAccessDto access)
    {
        var claims = new List<Claim>
        {
            new(AbpClaimTypes.UserId, user.Id.ToString()),
            new(AbpClaimTypes.UserName, user.UserName),
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.UserName)
        };

        if (!string.IsNullOrWhiteSpace(user.Email))
            claims.Add(new Claim(AbpClaimTypes.Email, user.Email));

        if (!string.IsNullOrWhiteSpace(user.Phone))
            claims.Add(new Claim(AbpClaimTypes.PhoneNumber, user.Phone));

        if (user.TenantId.HasValue)
        {
            claims.Add(new Claim(AbpClaimTypes.TenantId, user.TenantId.Value.ToString()));
            claims.Add(new Claim(NovaClaimTypes.TenantId, user.TenantId.Value.ToString()));
        }

        foreach (var roleCode in access.RoleCodes.Distinct(StringComparer.OrdinalIgnoreCase))
            claims.Add(new Claim(ClaimTypes.Role, roleCode));


        return CreateToken(
            claims,
            _configuration["Jwt:Secret"] ?? "NovaDefaultSecretKey_PleaseChangeInProduction_2024!",
            _configuration["Jwt:Issuer"] ?? "Nova",
            _configuration["Jwt:Audience"] ?? "Nova",
            120);
    }

    /// <summary>
    /// 创建刷新令牌（RefreshToken），有效期 7 天
    /// </summary>
    /// <param name="user">用户实体</param>
    /// <returns>JWT 字符串</returns>
    public string CreateRefreshToken(UserEntity user)
    {
        var claims = new List<Claim>
        {
            new(AbpClaimTypes.UserId, user.Id.ToString()),
            new("refresh", "true")
        };

        return CreateToken(
            claims,
            _configuration["Jwt:Secret"] ?? "NovaDefaultSecretKey_PleaseChangeInProduction_2024!",
            _configuration["Jwt:Issuer"] ?? "Nova",
            _configuration["Jwt:Audience"] ?? "Nova",
            10080);
    }

    /// <summary>
    /// 使用 HMAC-SHA256 生成 JWT
    /// </summary>
    private static string CreateToken(IEnumerable<Claim> claims, string secret, string issuer, string audience, int expiresMinutes)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(expiresMinutes),
            notBefore: DateTime.UtcNow,
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
