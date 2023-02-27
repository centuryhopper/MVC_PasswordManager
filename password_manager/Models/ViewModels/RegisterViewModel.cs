using System.ComponentModel.DataAnnotations;

namespace PasswordManager.Models;

public class RegisterViewModel
{
    [Required(ErrorMessage="Email is required"), StringLength(32), Display(Name="Email")]
    public string Email { get; set; } = null!;

    [Required(ErrorMessage="FirstName is required"), StringLength(32), Display(Name="FirstName")]
    public string FirstName { get; set; } = null!;

    [Required(ErrorMessage="LastName is required"), StringLength(32), Display(Name="LastName")]
    public string LastName { get; set; } = null!;

    [Required(ErrorMessage="Username is required"), StringLength(32), Display(Name="Username")]
    public string username { get; set; } = null!;

    [DataType(DataType.Password)]
    [Required(ErrorMessage="Password is required"), StringLength(512), Display(Name="Password")]
    public string password { get; set; } = null!;

    [Required]
    [DataType(DataType.Password)]
    [Display(Name = "Confirm password")]
    [Compare(nameof(password), ErrorMessage = "The password and confirmation password do not match.")]
    public string confirmPassword { get; set; } = null!;

}


