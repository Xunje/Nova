using System.ComponentModel.DataAnnotations;

namespace Nova.IdentityService.Core.Dtos.Accounts;

public class LoginInput
{
    [Required]
    [MaxLength(50)]
    public string UserName { get; set; } = string.Empty;

    [Required]
    [MinLength(6)]
    public string Password { get; set; } = string.Empty;
}
