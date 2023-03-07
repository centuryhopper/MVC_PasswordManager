
using System.ComponentModel.DataAnnotations;

namespace password_manager.Models;

public class ForgotPasswordViewModel
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = null!;
}