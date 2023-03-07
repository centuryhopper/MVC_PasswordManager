using System.ComponentModel.DataAnnotations;
namespace password_manager.Models;

public class ResetPasswordViewModel
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = null!;

    [Required]
    [DataType(DataType.Password)]
    public string Password { get; set; } = null!;

    [DataType(DataType.Password)]
    [Display(Name = "Confirm password")]
    [Compare("Password",
        ErrorMessage = "Password and Confirm Password must match")]
    public string ConfirmPassword { get; set; } = null!;

    public string Token { get; set; } = null!;
}
