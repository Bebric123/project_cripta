using StackExchange.Redis;
using System;
using System.Configuration;

namespace project_cripta.Services
{
    public class RedisService
    {
        private readonly ConnectionMultiplexer _redis;
        private readonly IDatabase _db;

        public RedisService()
        {
            _redis = ConnectionMultiplexer.Connect("localhost:6379");
            _db = _redis.GetDatabase();
        }

        public void CacheCryptoData(string key, string data)
        {
            _db.StringSet(key, data, TimeSpan.FromMinutes(2));
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