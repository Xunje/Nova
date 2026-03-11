namespace Nova.IdentityService.Core.OperLog;

/// <summary>
/// 操作日志特性，标记在 Controller 方法上以记录操作日志
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
public class OperLogAttribute : Attribute
{
    /// <summary>
    /// 操作类型
    /// </summary>
    public Enums.OperType OperType { get; set; }

    /// <summary>
    /// 日志标题（模块）
    /// </summary>
    public string Title { get; set; }

    /// <summary>
    /// 是否保存请求数据
    /// </summary>
    public bool IsSaveRequestData { get; set; } = true;

    /// <summary>
    /// 是否保存返回数据
    /// </summary>
    public bool IsSaveResponseData { get; set; } = true;

    public OperLogAttribute(string title, Enums.OperType operationType)
    {
        Title = title;
        OperType = operationType;
    }
}
