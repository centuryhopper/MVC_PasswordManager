using System.ComponentModel.DataAnnotations;

namespace password_manager.Models;

public class UserModel
{
    [Key]
    public string? userId { get; set; }

    [Required(ErrorMessage = "Username is required"), StringLength(32), Display(Name = "Username")]
    public string? username { get; set; }

    [DataType(DataType.Password)]
    [Required(ErrorMessage = "Password is required"), StringLength(512), Display(Name = "Password")]
    public string? password { get; set; }
    public string? aesKey { get; set; }
    public string? aesIV { get; set; }
    public List<AccountModel>? accounts { get; set; }
    public string currentJwtToken { get; set; } = string.Empty;
    public string? tokenCreated { get; set; }
    public string? tokenExpires { get; set; }

    public override string ToString()
    {
        return $"{nameof(username)}: {username}, {nameof(password)}: {password}, {nameof(aesKey)}: {aesKey}, {nameof(aesIV)}: {aesIV}, {nameof(currentJwtToken)}: {currentJwtToken}, {nameof(tokenCreated)}: {tokenCreated}, {nameof(tokenExpires)}: {tokenExpires}";
    }
}

