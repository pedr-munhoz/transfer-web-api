using Medallion.Threading.Postgres;

namespace api.Services
{
    public class DistributedLockService : ILockService
    {
        private readonly IConfiguration _configuration;

        public DistributedLockService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<T?> ExecuteLockedAsync<T>(string key, Func<Task<T>> method)
        {
            var @lock = GetLock(key);

            T response;
            await using (await @lock.AcquireAsync())
            {
                // I have the lock
                response = await method();
            }

            return response;
        }

        public async Task ExecuteLockedAsync(string key, Func<Task> method)
        {
            var @lock = GetLock(key);

            await using (await @lock.AcquireAsync())
            {
                // I have the lock
                await method();
            }
        }

        public async Task<T?> ExecuteLockedAsync<T>(List<string> keys, Func<Task<T>> method)
        {
            if (!keys.Any())
                return default;

            if (keys.Count == 1)
                return await ExecuteLockedAsync(key: keys.First(), method: method);

            var key = keys.First();
            var remainingKeys = keys.Skip(1).ToList();

            var @lock = GetLock(key);

            await using (await @lock.AcquireAsync())
            {
                // I have the lock
                return await ExecuteLockedAsync(keys: remainingKeys, method: method);
            }
        }

        public async Task ExecuteLockedAsync(List<string> keys, Func<Task> method)
        {
            if (!keys.Any())
                return;

            if (keys.Count == 1)
                await ExecuteLockedAsync(key: keys.First(), method: method);

            var key = keys.First();
            var remainingKeys = keys.Skip(1).ToList();

            var @lock = GetLock(key);

            await using (await @lock.AcquireAsync())
            {
                // I have the lock
                await ExecuteLockedAsync(keys: remainingKeys, method: method);
            }
        }

        private PostgresDistributedLock GetLock(string key)
        {
            var connectionString = _configuration.GetValue<string>("ServerDbConnectionString");
            var postgresAdvisoryLockKey = new PostgresAdvisoryLockKey(key, allowHashing: true);

            var @lock = new PostgresDistributedLock(postgresAdvisoryLockKey, connectionString);

            return @lock;
        }
    }
}