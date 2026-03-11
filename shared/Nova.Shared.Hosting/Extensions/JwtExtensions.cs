using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Nova.Shared.Hosting.Security;

namespace Nova.Shared.Hosting.Extensions;

/// <summary>
/// JWT认证扩展方法
/// <para>提供统一的JWT认证配置</para>
/// </summary>
public static class JwtExtensions
{
    /// <summary>
    /// 添加Nova JWT认证服务
    /// <para>从配置文件读取JWT配置（Jwt:Secret/Issuer/Audience）</para>
    /// </summary>
    /// <param name="services">服务集合</param>
    /// <param name="configuration">配置对象</param>
    /// <returns>服务集合（支持链式调用）</returns>
    public static IServiceCollection AddNovaJwtAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        var jwtSection = configuration.GetSection("Jwt");
        // JWT密钥（生产环境必须修改）
        var secret = jwtSection["Secret"] ?? "NovaDefaultSecretKey_PleaseChangeInProduction_2024!";
        // 签发者
        var issuer = jwtSection["Issuer"] ?? "Nova";
        // 受众
        var audience = jwtSection["Audience"] ?? "Nova";

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,           // 验证签发者
                ValidateAudience = true,         // 验证受众
                ValidateLifetime = true,         // 验证过期时间
                ValidateIssuerSigningKey = true, // 验证签名密钥
                ValidIssuer = issuer,
                ValidAudience = audience,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret))
            };

            options.Events = new JwtBearerEvents
            {
                OnMessageReceived = context =>
                {
                    if (string.IsNullOrWhiteSpace(context.Token))
                    {
                        var accessToken = context.Request.Headers["access_token"].ToString();
                        if (string.IsNullOrWhiteSpace(accessToken))
                            accessToken = context.Request.Query["access_token"].ToString();

                        if (!string.IsNullOrWhiteSpace(accessToken))
                            context.Token = accessToken;
                    }

                    return Task.CompletedTask;
                },
                OnTokenValidated = context =>
                {
                    var tenantId = context.Principal.GetTenantIdOrNull();
                    if (tenantId.HasValue && string.IsNullOrWhiteSpace(context.Request.Headers["__tenant"]))
                        context.Request.Headers["__tenant"] = tenantId.Value.ToString();

                    return Task.CompletedTask;
                }
            };
        });

        return services;
    }
}
