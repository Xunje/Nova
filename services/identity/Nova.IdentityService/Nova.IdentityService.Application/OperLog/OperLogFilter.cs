using System.Reflection;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Nova.IdentityService.Core.Entities;
using Nova.IdentityService.Core.OperLog;
using Nova.Shared.Hosting.Extensions;
using SqlSugar;
using Volo.Abp.DependencyInjection;
using Volo.Abp.MultiTenancy;
using Volo.Abp.Users;

namespace Nova.IdentityService.Application.OperLog;

/// <summary>
/// 操作日志全局过滤器，对标记 [OperLog] 的方法自动记录操作日志
/// </summary>
public class OperLogFilter : IAsyncActionFilter, ITransientDependency
{
    private readonly ISqlSugarClient _db;
    private readonly ICurrentUser _currentUser;
    private readonly ICurrentTenant _currentTenant;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public OperLogFilter(
        ISqlSugarClient db,
        ICurrentUser currentUser,
        ICurrentTenant currentTenant,
        IHttpContextAccessor httpContextAccessor)
    {
        _db = db;
        _currentUser = currentUser;
        _currentTenant = currentTenant;
        _httpContextAccessor = httpContextAccessor;
    }

    /// <summary>
    /// 在 Action 执行后检查是否标记了 [OperLog]，若有则写入操作日志
    /// </summary>
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var resultContext = await next();

        // 仅处理 Controller Action
        if (resultContext.ActionDescriptor is not ControllerActionDescriptor controllerActionDescriptor)
            return;

        // 检查方法是否标记了 [OperLog] 特性
        var operLogAttribute = controllerActionDescriptor.MethodInfo
            .GetCustomAttributes(inherit: true)
            .FirstOrDefault(a => a.GetType() == typeof(OperLogAttribute)) as OperLogAttribute;

        if (operLogAttribute == null)
            return;

        // 收集请求信息：IP、路径、用户等
        var httpContext = resultContext.HttpContext;
        var ip = httpContext.GetClientIp();
        var location = ResolveLocation(ip);

        var logEntity = new OperationLogEntity
        {
            Title = operLogAttribute.Title,
            OperType = operLogAttribute.OperType,
            RequestMethod = httpContext.Request.Method,
            OperUser = _currentUser.UserName ?? _currentUser.Id?.ToString() ?? "匿名",
            OperIp = ip,
            OperLocation = location,
            Method = httpContext.Request.Path.Value,
            CreateTime = DateTime.UtcNow,
            TenantId = _currentTenant.Id
        };

        // 可选：保存请求参数
        if (operLogAttribute.IsSaveRequestData && context.ActionArguments.Count > 0)
        {
            try
            {
                logEntity.RequestParam = JsonSerializer.Serialize(context.ActionArguments);
            }
            catch
            {
                logEntity.RequestParam = "(序列化失败)";
            }
        }

        // 可选：保存响应内容
        if (operLogAttribute.IsSaveResponseData && resultContext.Result != null)
        {
            logEntity.RequestResult = TryGetResultContent(resultContext.Result);
        }

        try
        {
            await _db.Insertable(logEntity).ExecuteCommandAsync();
        }
        catch
        {
            // 日志记录失败不影响主流程
        }
    }

    /// <summary>
    /// 尝试从 IActionResult 中提取 JSON 内容
    /// </summary>
    private static string? TryGetResultContent(IActionResult result)
    {
        return result switch
        {
            ContentResult content when content.ContentType?.Contains("json") == true
                => content.Content?.Replace("\r\n", "").Trim(),
            JsonResult json => json.Value == null ? null : JsonSerializer.Serialize(json.Value),
            ObjectResult obj => obj.Value == null ? null : JsonSerializer.Serialize(obj.Value),
            _ => null
        };
    }

    /// <summary>
    /// 根据 IP 解析地理位置（内网/外网，暂不接入 IP 库）
    /// </summary>
    private static string ResolveLocation(string? ip)
    {
        if (string.IsNullOrEmpty(ip)) return "未知";
        if (ip is "127.0.0.1" or "::1" or "localhost") return "内网";
        if (ip.StartsWith("192.168.") || ip.StartsWith("10.") || ip.StartsWith("172."))
            return "内网";
        return ip; // 外网 IP 暂不解析地理位置，可后续接入 IP 库
    }
}
