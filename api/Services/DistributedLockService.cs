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

        private PostgresDistributedLock GetLock(string key)
        {
            var connectionString = _configuration.GetValue<string>("ServerDbConnectionString");
            var postgresAdvisoryLockKey = new PostgresAdvisoryLockKey(key, allowHashing: true);

            var @lock = new PostgresDistributedLock(postgresAdvisoryLockKey, connectionString);

            return @lock;
        }
    }
}