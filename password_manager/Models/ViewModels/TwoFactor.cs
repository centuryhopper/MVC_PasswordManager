using System.ComponentModel.DataAnnotations;
using password_manager.Models;

public class TwoFactor
{
    [Required]
    public string TwoFactorCode { get; set; } = null!;
}