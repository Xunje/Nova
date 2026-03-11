namespace Nova.IdentityService.Core.Enums;

/// <summary>
/// 操作日志类型
/// </summary>
public enum OperType
{
    Insert = 1,
    Update = 2,
    Delete = 3,
    Auth = 4,
    Export = 5,
    Import = 6,
    ForcedOut = 7,
    GenerateCode = 8,
    ClearData = 9
}
