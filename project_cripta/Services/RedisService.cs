using StackExchange.Redis;
using System;

namespace project_cripta.Services
{
    public class RedisService : IRedisService
    {
        private readonly ConnectionMultiplexer _redis;
        private readonly IDatabase _db;

        public RedisService()
        {
            _redis = ConnectionMultiplexer.Connect("localhost:6379");
            _db = _redis.GetDatabase();
        }

        public void CacheCryptoData(string key, string data, TimeSpan? expiry = null)
        {
            try
            {
                var database = _redis.GetDatabase();
                database.StringSet(key, data, expiry ?? TimeSpan.FromMinutes(1));
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Redis cache error: {ex.Message}");
            }
        }

        public string GetCachedCryptoData(string key)
        {
            return _db.StringGet(key);
        }

        public void CacheUserBalance(string userId, decimal balance)
        {
            _db.StringSet($"balance_{userId}", balance.ToString(), TimeSpan.FromMinutes(1));
        }

        public decimal? GetCachedUserBalance(string userId)
        {
            var cached = _db.StringGet($"balance_{userId}");
            if (cached.HasValue && decimal.TryParse(cached, out var balance))
                return balance;
            return null;
        }

        public void RemoveUserBalance(string userId)
        {
            _db.KeyDelete($"balance_{userId}");
        }
    }
}