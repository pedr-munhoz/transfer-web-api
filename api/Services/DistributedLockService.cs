using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Medallion.Threading.Redis;
using StackExchange.Redis;

namespace api.Services
{
    public class DistributedLockService
    {
        public async Task Lock(string key)
        {
            var connection = await ConnectionMultiplexer.ConnectAsync("connectionString"); // uses StackExchange.Redis
            var @lock = new RedisDistributedLock(key, connection.GetDatabase());
            await using var handle = await @lock.TryAcquireAsync();
            if (handle != null) { /* I have the lock */ }
        }
    }
}