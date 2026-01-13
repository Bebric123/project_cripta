using Newtonsoft.Json;
using project_cripta.Models;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace project_cripta.Services
{
    public class CryptoService : ICryptoService
    {
        private readonly IRedisService _redisService;

        public CryptoService(IRedisService redisService)
        {
            _redisService = redisService ?? throw new ArgumentNullException(nameof(redisService));
        }

        public async Task<List<CryptoModel>> GetCryptoCurrenciesAsync(int page = 1, int perPage = 20)
        {
            var cacheKey = $"crypto_{page}_{perPage}";

            var cached = _redisService.GetCachedCryptoData(cacheKey);
            if (!string.IsNullOrEmpty(cached))
            {
                return JsonConvert.DeserializeObject<List<CryptoModel>>(cached);
            }

            try
            {
                using (var httpClient = new HttpClient())
                {
                    httpClient.BaseAddress = new Uri("https://api.coingecko.com/api/v3/");
                    httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0");

                    var url = $"coins/markets?vs_currency=usd&order=market_cap_desc&per_page={perPage}&page={page}&sparkline=false";
                    var response = await httpClient.GetAsync(url);

                    if (response.IsSuccessStatusCode)
                    {
                        var json = await response.Content.ReadAsStringAsync();
                        var data = JsonConvert.DeserializeObject<List<CryptoModel>>(json);

                        _redisService.CacheCryptoData(cacheKey, json);
                        return data;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка: {ex.Message}");
            }

            return GetSampleData();
        }

        private List<CryptoModel> GetSampleData()
        {
            return new List<CryptoModel>
            {
                new CryptoModel {
                    Id = "bitcoin", Name = "Bitcoin", Symbol = "btc",
                    CurrentPrice = 45000.50m, PriceChange24h = 1250.25m,
                    PriceChangePercentage24h = 2.85m, MarketCap = 880000000000m,
                    TotalVolume = 25000000000m, LastUpdated = DateTime.Now
                },
                new CryptoModel {
                    Id = "ethereum", Name = "Ethereum", Symbol = "eth",
                    CurrentPrice = 3200.75m, PriceChange24h = 85.50m,
                    PriceChangePercentage24h = 2.74m, MarketCap = 385000000000m,
                    TotalVolume = 15000000000m, LastUpdated = DateTime.Now
                }
            };
        }

        public async Task<decimal?> GetCryptoPriceAsync(string cryptoId)
        {
            try
            {
                var cacheKey = $"crypto_price_{cryptoId}";

                var cached = _redisService.GetCachedCryptoData(cacheKey);
                if (!string.IsNullOrEmpty(cached))
                {
                    return decimal.Parse(cached);
                }

                using (var httpClient = new HttpClient())
                {
                    httpClient.BaseAddress = new Uri("https://api.coingecko.com/api/v3/");
                    httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0");
                    httpClient.Timeout = TimeSpan.FromSeconds(10);

                    var url = $"simple/price?ids={cryptoId}&vs_currencies=usd";
                    var response = await httpClient.GetAsync(url);

                    if (response.IsSuccessStatusCode)
                    {
                        var json = await response.Content.ReadAsStringAsync();
                        var data = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, decimal>>>(json);

                        if (data.ContainsKey(cryptoId) && data[cryptoId].ContainsKey("usd"))
                        {
                            var price = data[cryptoId]["usd"];

                            _redisService.CacheCryptoData(cacheKey, price.ToString());
                            return price;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"GetCryptoPriceAsync error for {cryptoId}: {ex.Message}");
            }

            return null;
        }

        public async Task<CryptoModel> GetCryptoDetailAsync(string cryptoId)
        {
            var cacheKey = $"crypto_detail_{cryptoId}";

            var cached = _redisService.GetCachedCryptoData(cacheKey);
            if (!string.IsNullOrEmpty(cached))
            {
                return JsonConvert.DeserializeObject<CryptoModel>(cached);
            }

            try
            {
                using (var httpClient = new HttpClient())
                {
                    httpClient.BaseAddress = new Uri("https://api.coingecko.com/api/v3/");
                    httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0");
                    httpClient.Timeout = TimeSpan.FromSeconds(10);

                    var url = $"coins/markets?vs_currency=usd&ids={cryptoId}&order=market_cap_desc&per_page=1&page=1&sparkline=false";
                    var response = await httpClient.GetAsync(url);

                    if (response.IsSuccessStatusCode)
                    {
                        var json = await response.Content.ReadAsStringAsync();
                        var data = JsonConvert.DeserializeObject<List<CryptoModel>>(json);

                        if (data != null && data.Count > 0)
                        {
                            var cryptoDetail = data[0];

                            _redisService.CacheCryptoData(cacheKey, JsonConvert.SerializeObject(cryptoDetail), TimeSpan.FromMinutes(2));

                            return cryptoDetail;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"GetCryptoDetailAsync error for {cryptoId}: {ex.Message}");
            }
            return null;
        }
    }
}