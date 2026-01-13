using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using project_cripta.Models;
using project_cripta.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace project_cripta.Controllers
{
    public class ProfileController : Controller
    {
        private readonly ICryptoService _cryptoService;
        private readonly IBalanceService _balanceService;
        private readonly IRedisService _redisService;

        public ProfileController(
           ICryptoService cryptoService,
           IBalanceService balanceService,
           IRedisService redisService)
        {
            _cryptoService = cryptoService;
            _balanceService = balanceService;
            _redisService = redisService;
        }

        private readonly Dictionary<string, string> _currencyColors = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            { "btc", "#f7931a" }, { "eth", "#627eea" }, { "usdt", "#26a17b" },
            { "bnb", "#f3ba2f" }, { "ada", "#0033ad" }, { "sol", "#00ffbd" },
            { "xrp", "#23292f" }, { "dot", "#e6007a" }, { "doge", "#c2a633" },
            { "matic", "#8247e5" }, { "sui", "#5f63f2" }, { "avax", "#e84142" },
            { "link", "#2a5ada" }, { "ltc", "#bfbbbb" }, { "bch", "#8dc351" }
        };

        private readonly Dictionary<string, string> _symbolToId = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            { "ETH", "ethereum" }, { "BTC", "bitcoin" }, { "BNB", "binancecoin" },
            { "ADA", "cardano" }, { "SOL", "solana" }, { "XRP", "ripple" },
            { "DOT", "polkadot" }, { "DOGE", "dogecoin" }, { "MATIC", "matic-network" },
            { "SUI", "sui" }, { "AVAX", "avalanche-2" }, { "LINK", "chainlink" },
            { "LTC", "litecoin" }, { "BCH", "bitcoin-cash" }
        };

        private readonly Dictionary<string, string> _currencyNames = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            { "BTC", "Bitcoin" }, { "ETH", "Ethereum" }, { "USDT", "Tether" },
            { "BNB", "Binance Coin" }, { "ADA", "Cardano" }, { "SOL", "Solana" },
            { "XRP", "Ripple" }, { "DOT", "Polkadot" }, { "DOGE", "Dogecoin" },
            { "MATIC", "Polygon" }, { "SUI", "Sui" }, { "AVAX", "Avalanche" },
            { "LINK", "Chainlink" }, { "LTC", "Litecoin" }, { "BCH", "Bitcoin Cash" }
        };

        [HttpGet]
        public async Task<ActionResult> Profile(int page = 1, int perPage = 5)
        {
            var userId = GetCurrentUserId();
            if (userId == 0)
            {
                return RedirectToAction("Login", "Account");
            }

            await _balanceService.InitializeUserBalancesAsync(userId);

            var cryptocurrencies = await _cryptoService.GetCryptoCurrenciesAsync(page, perPage);
            var userBalances = await _balanceService.GetAllUserBalancesAsync(userId);

            ViewBag.CurrentPage = page;
            ViewBag.PerPage = perPage;
            ViewBag.TotalPages = 100;

            var balanceDict = userBalances.ToDictionary(b => b.Currency, b => b.Amount);
            ViewBag.UserBalances = balanceDict;

            return View(cryptocurrencies);
        }

        [HttpPost]
        public async Task<JsonResult> GetAvailableCurrencies(string search = "")
        {
            try
            {
                var allCryptos = await _cryptoService.GetCryptoCurrenciesAsync(1, 100);

                var currencies = allCryptos.Select(c => new
                {
                    id = c.Id,
                    symbol = c.Symbol.ToUpper(),
                    name = c.Name,
                    price = c.CurrentPrice,
                    icon = c.Symbol.Substring(0, 1).ToUpper(),
                    color = GetCurrencyColor(c.Symbol)
                });

                if (!string.IsNullOrEmpty(search))
                {
                    currencies = currencies.Where(c =>
                        c.name.ToLower().Contains(search.ToLower()) ||
                        c.symbol.ToLower().Contains(search.ToLower()));
                }

                return Json(new { success = true, currencies = currencies.Take(20) });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = ex.Message });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Exchange(string toCurrency, decimal? fromAmount)
        {
            var userId = GetCurrentUserId();
            if (userId == 0)
            {
                TempData["Error"] = "Не авторизован";
                return RedirectToAction("Login", "Account");
            }

            if (!fromAmount.HasValue || fromAmount <= 0)
            {
                TempData["Error"] = "Неверная сумма для обмена";
                return RedirectToAction("Profile");
            }

            using (var context = new ApplicationDbContext())
            using (var dbContextTransaction = context.Database.BeginTransaction())
            {
                try
                {
                    const string fromCurrency = "USDT";

                    var exchangeRate = await CalculateExchangeRate(fromCurrency, toCurrency);
                    var toAmount = fromAmount.Value * exchangeRate;
                    var fee = CalculateNetworkFee(toCurrency);
                    var receivedAmount = toAmount - fee;

                    var success = await _balanceService.CreateExchangeTransactionAsync(
                        userId, fromCurrency, toCurrency, fromAmount.Value, toAmount, exchangeRate, fee);

                    if (success)
                    {
                        dbContextTransaction.Commit();
                        TempData["Success"] = $"Успешный обмен! Получено: {receivedAmount:F6} {toCurrency.ToUpper()}";
                    }
                    else
                    {
                        dbContextTransaction.Rollback();
                        TempData["Error"] = "Недостаточно средств для обмена";
                    }
                }
                catch (Exception ex)
                {
                    dbContextTransaction.Rollback();
                    TempData["Error"] = $"Ошибка обмена: {ex.Message}";
                }
            }

            return RedirectToAction("Profile");
        }

        [HttpPost]
        public async Task<JsonResult> CalculateExchange(string toCurrency, decimal amount)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (userId == 0)
                {
                    return Json(new { success = false, error = "Не авторизован" });
                }

                const string fromCurrency = "USDT";

                var usdtBalance = await _balanceService.GetUserBalanceAsync(userId, fromCurrency);
                var availableBalance = usdtBalance?.Amount ?? 0;

                var exchangeRate = await CalculateExchangeRate(fromCurrency, toCurrency);
                var toAmount = amount * exchangeRate;
                var fee = CalculateNetworkFee(toCurrency);
                var receivedAmount = toAmount - fee;

                var result = new
                {
                    success = true,
                    toAmount = toAmount,
                    receivedAmount = receivedAmount,
                    networkFee = fee,
                    exchangeRate = exchangeRate,
                    availableBalance = availableBalance,
                    hasSufficientFunds = amount <= availableBalance
                };

                return Json(result);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = ex.Message });
            }
        }

        [HttpPost]
        public async Task<JsonResult> GetBalanceForCurrency(string currency)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (userId == 0)
                {
                    return Json(new { success = false, error = "Не авторизован" });
                }

                var balance = await _balanceService.GetUserBalanceAsync(userId, currency.ToUpper());
                var balanceAmount = balance?.Amount ?? 0;

                return Json(new { success = true, balance = balanceAmount });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = ex.Message });
            }
        }

        [HttpPost]
        public async Task<JsonResult> GetPortfolioData()
        {
            try
            {
                var userId = GetCurrentUserId();
                if (userId == 0)
                    return Json(new { success = false, error = "Не авторизован" });

                var userBalances = await _balanceService.GetAllUserBalancesAsync(userId);
                var portfolioData = new List<object>();
                decimal totalValue = 0;

                foreach (var balance in userBalances.Where(b => b.Amount > 0))
                {
                    var currency = balance.Currency.ToUpper();
                    var amount = balance.Amount;
                    decimal currentPrice = await GetCurrentPrice(currency);

                    var value = amount * currentPrice;
                    totalValue += value;
                }

                foreach (var balance in userBalances.Where(b => b.Amount > 0))
                {
                    var currency = balance.Currency.ToUpper();
                    var amount = balance.Amount;
                    decimal currentPrice = await GetCurrentPrice(currency);

                    var value = amount * currentPrice;
                    var percentage = totalValue > 0 ? (value / totalValue * 100) : 0;

                    portfolioData.Add(new
                    {
                        symbol = currency,
                        name = GetCurrencyName(currency),
                        amount = amount,
                        price = currentPrice,
                        value = value,
                        color = GetCurrencyColor(currency.ToLower()),
                        change24h = GetRandomChange(),
                        percentage = percentage
                    });
                }

                portfolioData = portfolioData.OrderByDescending(x => ((dynamic)x).value).ToList();

                return Json(new
                {
                    success = true,
                    totalValue = totalValue,
                    totalChange = GetRandomPortfolioChange(),
                    assets = portfolioData
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = ex.Message });
            }
        }

        [HttpPost]
        public async Task<JsonResult> GetCryptoAnalytics(string symbol)
        {
            try
            {
                var cryptoId = GetCryptoIdBySymbol(symbol);
                if (string.IsNullOrEmpty(cryptoId))
                {
                    return Json(new { success = false, error = "Криптовалюта не найдена" });
                }

                var cryptoDetail = await _cryptoService.GetCryptoDetailAsync(cryptoId);
                var historicalData = await GetHistoricalData(cryptoId, cryptoDetail?.CurrentPrice ?? GetFallbackPrice(symbol));

                if (cryptoDetail == null)
                {
                    return Json(new
                    {
                        success = true,
                        symbol = symbol.ToUpper(),
                        currentPrice = GetFallbackPrice(symbol),
                        priceChange24h = GetRandomChange(),
                        marketCap = GetRandomMarketCap(symbol),
                        volume24h = GetRandomVolume(symbol),
                        stats = GetDemoStats(symbol),
                        historicalData = historicalData
                    });
                }

                return Json(new
                {
                    success = true,
                    symbol = symbol.ToUpper(),
                    currentPrice = cryptoDetail.CurrentPrice,
                    priceChange24h = cryptoDetail.PriceChangePercentage24h,
                    marketCap = cryptoDetail.MarketCap,
                    volume24h = cryptoDetail.TotalVolume,
                    stats = GetRealStats(cryptoDetail),
                    historicalData = historicalData
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = ex.Message });
            }
        }

        #region Вспомогательные методы

        private int GetCurrentUserId()
        {
            var userId = Session["UserId"] as int?;
            return userId ?? 0;
        }

        private string GetCurrencyColor(string symbol)
        {
            return _currencyColors.ContainsKey(symbol.ToLower()) ?
                _currencyColors[symbol.ToLower()] : "#7c3aed";
        }

        private string GetCryptoIdBySymbol(string symbol)
        {
            return _symbolToId.ContainsKey(symbol) ? _symbolToId[symbol] : symbol.ToLower();
        }

        private string GetCurrencyName(string symbol)
        {
            return _currencyNames.ContainsKey(symbol) ? _currencyNames[symbol] : symbol;
        }

        private async Task<decimal> CalculateExchangeRate(string fromCurrency, string toCurrency)
        {
            try
            {
                if (fromCurrency.Equals("USDT", StringComparison.OrdinalIgnoreCase))
                {
                    var cryptoId = GetCryptoIdBySymbol(toCurrency);
                    if (!string.IsNullOrEmpty(cryptoId))
                    {
                        var cryptoPrice = await _cryptoService.GetCryptoPriceAsync(cryptoId);
                        if (cryptoPrice.HasValue && cryptoPrice > 0)
                        {
                            return 1 / cryptoPrice.Value;
                        }
                    }
                }
                return GetFallbackExchangeRate(toCurrency);
            }
            catch (Exception)
            {
                return GetFallbackExchangeRate(toCurrency);
            }
        }

        private decimal GetFallbackExchangeRate(string toCurrency)
        {
            var rates = new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase)
            {
                { "ETH", 0.0005m }, { "BTC", 0.000025m }, { "BNB", 0.002m },
                { "ADA", 1.2m }, { "SOL", 0.02m }, { "XRP", 1.5m },
                { "DOT", 0.1m }, { "DOGE", 10m }, { "MATIC", 0.8m },
                { "SUI", 0.9m }
            };

            return rates.ContainsKey(toCurrency) ? rates[toCurrency] : 0.001m;
        }

        private decimal CalculateNetworkFee(string currency)
        {
            switch (currency.ToUpper())
            {
                case "ETH": return 0.001m;
                case "BTC": return 0.0001m;
                case "BNB": return 0.005m;
                default: return 0.1m;
            }
        }

        private async Task<decimal> GetCurrentPrice(string currency)
        {
            if (currency == "USDT") return 1;

            var cryptoId = GetCryptoIdBySymbol(currency);
            if (!string.IsNullOrEmpty(cryptoId))
            {
                var price = await _cryptoService.GetCryptoPriceAsync(cryptoId);
                if (price.HasValue) return price.Value;
            }

            return GetFallbackPrice(currency);
        }

        private decimal GetFallbackPrice(string currency)
        {
            var prices = new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase)
            {
                { "ETH", 2500m }, { "BTC", 50000m }, { "BNB", 600m },
                { "ADA", 0.55m }, { "SOL", 25m }, { "XRP", 0.52m },
                { "DOT", 8.3m }, { "DOGE", 0.07m }, { "MATIC", 0.9m },
                { "SUI", 0.77m }, { "AVAX", 33m }, { "LINK", 12.5m },
                { "LTC", 200m }, { "BCH", 500m }
            };

            return prices.ContainsKey(currency) ? prices[currency] : 1m;
        }

        private decimal GetRandomChange()
        {
            var random = new Random();
            return (decimal)(random.NextDouble() * 20 - 10);
        }

        private decimal GetRandomPortfolioChange()
        {
            var random = new Random();
            return (decimal)(random.NextDouble() * 15 + 5);
        }

        private decimal GetRandomMarketCap(string symbol)
        {
            var random = new Random();
            var caps = new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase)
            {
                { "BTC", 1000000000000m }, { "ETH", 300000000000m }, { "USDT", 80000000000m },
                { "BNB", 40000000000m }, { "ADA", 15000000000m }, { "SOL", 50000000000m },
                { "XRP", 25000000000m }, { "DOT", 8000000000m }, { "DOGE", 10000000000m },
                { "MATIC", 7000000000m }, { "SUI", 2000000000m }
            };

            return caps.ContainsKey(symbol) ? caps[symbol] : 1000000000m;
        }

        private decimal GetRandomVolume(string symbol)
        {
            var random = new Random();
            return GetRandomMarketCap(symbol) * (decimal)(random.NextDouble() * 0.1 + 0.05);
        }

        private object GetDemoStats(string symbol)
        {
            var random = new Random();
            return new[]
            {
                new {
                    value = "$" + FormatLargeNumber(GetRandomMarketCap(symbol) * (decimal)(random.NextDouble() * 0.1 + 0.05)),
                    label = "Stablecoin Market Cap"
                },
                new {
                    value = "$" + FormatLargeNumber(GetRandomMarketCap(symbol) * (decimal)(random.NextDouble() * 0.3 + 0.1)),
                    label = "DeFi Volume"
                },
                new {
                    value = FormatLargeNumber((decimal)(random.Next(1000000, 50000000))),
                    label = "Total Holders"
                }
            };
        }

        private object GetRealStats(CryptoModel crypto)
        {
            return new[]
            {
                new {
                    value = "$" + FormatLargeNumber(crypto.MarketCap),
                    label = "Market Cap"
                },
                new {
                    value = "$" + FormatLargeNumber(crypto.TotalVolume),
                    label = "24h Volume"
                },
                new {
                    value = crypto.CirculatingSupply > 0 ? crypto.CirculatingSupply.ToString("N0") : "N/A",
                    label = "Circulating Supply"
                }
            };
        }

        private string FormatLargeNumber(decimal number)
        {
            if (number >= 1000000000000) return (number / 1000000000000).ToString("F2") + "t";
            if (number >= 1000000000) return (number / 1000000000).ToString("F2") + "b";
            if (number >= 1000000) return (number / 1000000).ToString("F2") + "m";
            if (number >= 1000) return (number / 1000).ToString("F2") + "k";

            return number.ToString("F2");
        }

        private async Task<object> GetHistoricalData(string cryptoId, decimal currentPrice)
        {
            try
            {
                using (var httpClient = new HttpClient())
                {
                    httpClient.BaseAddress = new Uri("https://api.coingecko.com/api/v3/");
                    httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0");

                    var url = $"coins/{cryptoId}/market_chart?vs_currency=usd&days=7&interval=daily";
                    var response = await httpClient.GetAsync(url);

                    if (response.IsSuccessStatusCode)
                    {
                        var json = await response.Content.ReadAsStringAsync();
                        var data = JObject.Parse(json);

                        var prices = data["prices"]?.ToObject<List<List<double>>>() ?? new List<List<double>>();
                        var priceValues = prices.Select(p => (decimal)p[1]).ToList();

                        var labels = new List<string>();
                        for (int i = 6; i >= 0; i--)
                        {
                            labels.Add(DateTime.Now.AddDays(-i).ToString("MMM dd"));
                        }

                        return new
                        {
                            labels = labels,
                            prices = priceValues
                        };
                    }
                }
            }
            catch (Exception ex)
            {
            }

            return GetDemoHistoricalData(currentPrice);
        }

        private object GetDemoHistoricalData(decimal currentPrice)
        {
            var random = new Random();
            var prices = new List<decimal>();

            for (int i = 6; i >= 0; i--)
            {
                var daysAgo = i;
                var volatility = (decimal)(random.NextDouble() * 0.1 - 0.05);
                var trend = (decimal)((6 - daysAgo) * 0.02);

                var historicalPrice = currentPrice * (1 + volatility - trend);
                prices.Add(historicalPrice);
            }

            if (prices.Count > 0)
            {
                prices[prices.Count - 1] = currentPrice;
            }

            var labels = new List<string>();
            for (int i = 6; i >= 0; i--)
            {
                labels.Add(DateTime.Now.AddDays(-i).ToString("MMM dd"));
            }

            return new
            {
                labels = labels,
                prices = prices
            };
        }

        #endregion
    }
}