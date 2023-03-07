namespace password_manager.Services;

public interface IDataAccess<T>
{
    Task<IResult> Post(T model, string userId);
    Task<IResult> PostMany(List<T> models);
    Task<IEnumerable<T>> Get();
    Task<IEnumerable<T>> Get(string id);
    Task<IResult> Put(T model);
    Task<T> Delete(string id);
    Task<int> Commit();
    Task<T?> GetOne(string accountId);
    Task<IEnumerable<T>> FilterBy(string userId, string title);
}
