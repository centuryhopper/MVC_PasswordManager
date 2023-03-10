using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;

namespace password_manager.Models;

public class ProfileViewModel
{
    [Required(ErrorMessage = "Username is required"), StringLength(32), Display(Name = "Username")]
    public string UserName { get; set; } = null!;
    [Required(ErrorMessage = "Email is required"), StringLength(32), Display(Name = "Email")]
    [Remote(action: "IsEmailInUse", controller: "Account")]
    public string Email { get; set; } = null!;

    [Required(ErrorMessage = "FirstName is required"), StringLength(32), Display(Name = "FirstName")]
    public string FirstName { get; set; } = null!;
    [Required(ErrorMessage = "LastName is required"), StringLength(32), Display(Name = "LastName")]
    public string LastName { get; set; } = null!;

    [Required(ErrorMessage = "Role is required")]
    public string Role { get; set; } = null!;
}

