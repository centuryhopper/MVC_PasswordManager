using System.ComponentModel.DataAnnotations;
namespace password_manager.Models;

public class ConfirmDeleteViewModel
{
    [Required(ErrorMessage="Please enter your password"), Display(Name="Password")]
    public string ConfirmPassword { get; set; } = null!;
}