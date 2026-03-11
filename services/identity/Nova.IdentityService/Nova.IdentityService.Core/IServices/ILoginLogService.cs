namespace Nova.IdentityService.Core.IServices;

/// <summary>
/// 登录日志服务
/// </summary>
public interface ILoginLogService
{
    /// <summary>
    /// 记录登录日志
    /// </summary>
    Task RecordAsync(string loginUser, Core.Enums.LoginStatus status, string? logMsg = null);
}
