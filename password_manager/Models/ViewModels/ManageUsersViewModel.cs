
namespace PasswordManager.Models;

public class ManageUsersViewModel<T>
{
    public IEnumerable<T> Users { get; set; } = null!;
    public List<IEnumerable<string>> Roles { get; set; } = null!;
}