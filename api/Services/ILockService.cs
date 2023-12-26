namespace api.Services
{
    public interface ILockService
    {
        Task ExecuteLockedAsync(string key, Func<Task> method);
        Task<T?> ExecuteLockedAsync<T>(string key, Func<Task<T>> method);
        Task ExecuteLockedAsync(List<string> keys, Func<Task> method);
        Task<T?> ExecuteLockedAsync<T>(List<string> keys, Func<Task<T>> method);
    }
}