namespace PasswordManager.Services;

public interface IAuthenticationService<T>
{
    Task<string?> Login(T model);
    Task<bool> Register(T model);
    Task<int> Commit();
}
