
using System.ComponentModel.DataAnnotations;

namespace PasswordManager.Models;

public class AccountModel
{
    [Key]
    public string? accountId { get; set; }

    // each account will be associated with only one user
    [Required]
    public UserModel user {get; set;}

    [Required, StringLength(32)]
    public string? title { get; set; }

    [Required, StringLength(32)]
    public string? username { get; set; }

    [Required, StringLength(512)]
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

