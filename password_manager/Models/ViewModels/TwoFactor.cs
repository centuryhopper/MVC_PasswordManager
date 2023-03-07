using System.ComponentModel.DataAnnotations;
namespace password_manager.Models;

public class TwoFactor
{
    [Required]
    public string TwoFactorCode { get; set; } = null!;
}