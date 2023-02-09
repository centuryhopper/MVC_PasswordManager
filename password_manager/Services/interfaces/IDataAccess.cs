namespace PasswordManager.Services;

public interface IDataAccess<T>
{
    Task<IResult> Post(T model);
    Task<IResult> PostMany(List<T> models);
    Task<IResult> Get();
    Task<IResult> Get(string id);
    Task<IResult> Put(T model);
    Task<IResult> Delete(string id);
    Task<int> Commit();
}
