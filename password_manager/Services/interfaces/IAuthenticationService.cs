using password_manager.Models;

namespace password_manager.Services;

public interface IAuthenticationService<T>
{
    Task<T?> Login(LoginViewModel model);
    Task<T?> Register(RegisterViewModel model);
    Task<T?> Delete(string userId);
    Task<int> Commit();
}
