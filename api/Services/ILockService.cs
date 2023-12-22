using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace api.Services
{
    public interface ILockService
    {
        Task ExecuteLockedAsync(string key, Func<Task> method);
        Task<T> ExecuteLockedAsync<T>(string key, Func<Task<T>> method);
    }
}