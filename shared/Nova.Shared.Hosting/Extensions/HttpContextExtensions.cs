namespace Nova.Shared.Hosting.Extensions;

/// <summary>
/// HttpContext 扩展方法
/// </summary>
public static class HttpContextExtensions
{
    /// <summary>
    /// 获取客户端 IP 地址（支持代理 X-Forwarded-For）
    /// </summary>
    public static string? GetClientIp(this Microsoft.AspNetCore.Http.HttpContext? context)
    {
        if (context == null) return null;

        var forwardedFor = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();
        if (!string.IsNullOrWhiteSpace(forwardedFor))
        {
            // 多级代理时取第一个（真实客户端）
            return forwardedFor.Split(',')[0].Trim();
        }

        return context.Connection.RemoteIpAddress?.ToString();
    }

    /// <summary>
    /// 获取 User-Agent
    /// </summary>
    public static string? GetUserAgent(this Microsoft.AspNetCore.Http.HttpContext? context)
    {
        return context?.Request.Headers["User-Agent"].FirstOrDefault();
    }
}
