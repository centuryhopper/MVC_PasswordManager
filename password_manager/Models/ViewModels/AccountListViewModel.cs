namespace PasswordManager.Models;

public class AccountListViewModel
{
    public List<AccountModel> accountModels { get; set; } = null!;
    public string filterTerm { get; set; } = null!;
}

