namespace Nova.Shared.MultiTenancy;

public static class NovaSeedConstants
{
    public static readonly Guid DefaultTenantId = Guid.Parse("11111111-1111-1111-1111-111111111111");

    public const string DefaultTenantName = "默认演示租户";
    public const string DefaultTenantCode = "demo";
    public const string DefaultAdminUserName = "admin";
    public const string DefaultAdminEmail = "admin@nova.local";
    public const string DefaultAdminPhone = "13800000000";
    public const string DefaultAdminPassword = "123456";
}
