using Microsoft.AspNetCore.Http;
using Nova.IdentityService.Core.Entities;
using Volo.Abp.MultiTenancy;
using Nova.IdentityService.Core.Enums;
using Nova.IdentityService.Core.IServices;
using Nova.Shared.Hosting.Extensions;
using Nova.Shared.MultiTenancy;
using SqlSugar;
using UAParser;
using Volo.Abp.DependencyInjection;

namespace Nova.IdentityService.Application.LoginLog;

/// <summary>
/// 登录日志服务
/// <para>记录用户登录成功/失败，包含 IP、浏览器、操作系统等信息</para>
/// </summary>
public class LoginLogService : ILoginLogService, ITransientDependency
{
    private readonly ISqlSugarClient _db;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ICurrentTenant _currentTenant;

    public LoginLogService(
        ISqlSugarClient db,
        IHttpContextAccessor httpContextAccessor,
        ICurrentTenant currentTenant)
    {
        _db = db;
        _httpContextAccessor = httpContextAccessor;
        _currentTenant = currentTenant;
    }

    /// <summary>
    /// 记录登录日志（成功或失败）
    /// </summary>
    /// <param name="loginUser">登录用户名</param>
    /// <param name="status">登录状态（Success/Fail）</param>
    /// <param name="logMsg">失败时的原因说明</param>
    public async Task RecordAsync(string loginUser, LoginStatus status, string? logMsg = null)
    {
        var context = _httpContextAccessor.HttpContext;
        var ip = context?.GetClientIp();
        var userAgent = context?.GetUserAgent();
        // 解析 User-Agent 获取浏览器、操作系统
        var (browser, os) = ParseUserAgent(userAgent);
        var location = ResolveLocation(ip);

        var entity = new LoginLogEntity
        {
            LoginUser = loginUser,
            LoginIp = ip,
            LoginLocation = location,
            Browser = browser,
            Os = os,
            LogMsg = logMsg,
            LoginStatus = status,
            CreateTime = DateTime.UtcNow,
            TenantId = _currentTenant.Id
        };

        try
        {
            await _db.Insertable(entity).ExecuteCommandAsync();
        }
        catch
        {
            // 日志记录失败不影响主流程
        }
    }

    /// <summary>
    /// 解析 User-Agent 字符串，提取浏览器和操作系统
    /// </summary>
    private static (string? browser, string? os) ParseUserAgent(string? userAgent)
    {
        if (string.IsNullOrWhiteSpace(userAgent))
            return (null, null);

        try
        {
            var parser = Parser.GetDefault();
            var client = parser.Parse(userAgent);
            return (client.UA.ToString(), client.OS.ToString());
        }
        catch
        {
            return (null, null);
        }
    }

    private static string ResolveLocation(string? ip)
    {
        if (string.IsNullOrEmpty(ip)) return "未知";
        if (ip is "127.0.0.1" or "::1" or "localhost") return "内网-本机";
        if (ip.StartsWith("192.168.") || ip.StartsWith("10.") || ip.StartsWith("172."))
            return "内网";
        return ip;
    }
}
