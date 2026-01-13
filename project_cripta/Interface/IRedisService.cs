using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace project_cripta.Services
{
    public interface IRedisService
    {
        void CacheCryptoData(string key, string data, TimeSpan? expiry = null);
        string GetCachedCryptoData(string key);
        void CacheUserBalance(string userId, decimal balance);
        decimal? GetCachedUserBalance(string userId);
        void RemoveUserBalance(string userId);
    }
}
