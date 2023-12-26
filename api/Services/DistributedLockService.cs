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

        public async Task<T> ExecuteLockedAsync<T>(string key, Func<Task<T>> method)
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

        public async Task<T> ExecuteLockedAsync<T>(List<string> keys, Func<Task<T>> method)
        {
            var locks = keys.Select(GetLock).ToList();

            // Acquire all locks
            foreach (var @lock in locks)
            {
                await @lock.AcquireAsync();
            }

            T response;
            try
            {
                // All locks acquired
                response = await method();
            }
            finally
            {
                // Release all locks by allowing them to be disposed
                foreach (var @lock in locks)
                {
                    // The lock will be disposed, and the associated resources will be released
                }
            }

            return response;
        }

        public async Task ExecuteLockedAsync(List<string> keys, Func<Task> method)
        {
            var locks = keys.Select(GetLock).ToList();

            // Acquire all locks
            foreach (var @lock in locks)
            {
                await @lock.AcquireAsync();
            }

            try
            {
                // All locks acquired
                await method();
            }
            finally
            {
                // Release all locks by allowing them to be disposed
                foreach (var @lock in locks)
                {
                    // The lock will be disposed, and the associated resources will be released
                }
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