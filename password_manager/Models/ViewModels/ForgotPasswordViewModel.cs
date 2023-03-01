
using System.ComponentModel.DataAnnotations;

namespace PasswordManager.Models;

public class ForgotPasswordViewModel
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = null!;
}