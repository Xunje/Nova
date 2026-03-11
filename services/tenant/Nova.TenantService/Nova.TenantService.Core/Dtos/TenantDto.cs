using Nova.TenantService.Core.Entities;

namespace Nova.TenantService.Core.Dtos;

public class TenantDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public TenantStatus Status { get; set; }
    public TenantPlan Plan { get; set; }
    public DateTime? ExpireTime { get; set; }
    public string? ConnectionString { get; set; }
    public DateTime CreateTime { get; set; }
}
