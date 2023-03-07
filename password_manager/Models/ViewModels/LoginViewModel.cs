using System.ComponentModel.DataAnnotations;

namespace password_manager.Models;

public class LoginViewModel
{
    [Required(ErrorMessage = "Username is required"), StringLength(32), Display(Name = "Username")]
    public string username { get; set; } = null!;

    [DataType(DataType.Password)]
    [Required(ErrorMessage = "Password is required"), StringLength(512), Display(Name = "Password")]
    public string password { get; set; } = null!;

}

