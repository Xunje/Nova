using System.ComponentModel.DataAnnotations;
using Nova.TenantService.Core.Entities;

namespace Nova.TenantService.Core.Dtos;

public class CreateTenantInput
{
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [MaxLength(50)]
    public string Code { get; set; } = string.Empty;

    public TenantPlan Plan { get; set; } = TenantPlan.Free;

    public DateTime? ExpireTime { get; set; }

    public string? ConnectionString { get; set; }
}
