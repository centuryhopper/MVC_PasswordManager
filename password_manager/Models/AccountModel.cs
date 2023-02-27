
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PasswordManager.Models;

public class AccountModel
{
    [Key]
    public string? accountId { get; set; }

    // each account will be associated with only one user
    [Required, ForeignKey("ApplicationUser")]
    public string? userId {get; set;}
    public ApplicationUser user {get; set;} = null!;

    [Required(ErrorMessage="Title is required"), StringLength(32), Display(Name="Title: ")]
    public string? title { get; set; }

    [Required(ErrorMessage="Username is required"), StringLength(32), Display(Name="Username: ")]
    public string? username { get; set; }

    [Required(ErrorMessage="Password is required"), StringLength(512), Display(Name="Password: ")]
    public string? password { get; set; }
    public string? aesKey { get; set; }
    public string? aesIV { get; set; }
    public string? insertedDateTime { get; set; }
    public string? lastModifiedDateTime { get; set; }


    public override string ToString()
    {
        return $"id: {accountId}, title: {title}, username: {username}, password: {password}, aesKey: {aesKey}, aesIV: {aesIV}, insertedDateTime: {insertedDateTime}, lastModifiedDateTime: {lastModifiedDateTime}";
    }
}

