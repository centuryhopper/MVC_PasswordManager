using System.ComponentModel.DataAnnotations;

namespace password_manager.Models;

public class password_managerEntryModel
{
    [Key]
    public int Id { get; set; }

    [Required(ErrorMessage = "Please enter your username: "), StringLength(32), Display(Name = "username: ")]
    public string? username { get; set; }

    [Required(ErrorMessage = "Please enter your password: "), StringLength(32), Display(Name = "password: ")]
    public string? password { get; set; }

    public override string ToString()
    {
        return $"username: {username}, password: {password}";
    }
}

