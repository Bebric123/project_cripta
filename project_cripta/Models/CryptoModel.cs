using Newtonsoft.Json;
using System;

namespace project_cripta.Models
{
    public class CryptoModel
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("symbol")]
        public string Symbol { get; set; }

        [JsonProperty("current_price")]
        public decimal CurrentPrice { get; set; }

        [JsonProperty("price_change_24h")]
        public decimal? PriceChange24h { get; set; }

        [JsonProperty("price_change_percentage_24h")]
        public decimal? PriceChangePercentage24h { get; set; }

        [JsonProperty("market_cap")]
        public decimal MarketCap { get; set; }

        [JsonProperty("total_volume")]
        public decimal TotalVolume { get; set; }
        public decimal CirculatingSupply { get; set; }
        public decimal TotalSupply { get; set; }
        [JsonProperty("last_updated")]
        public DateTime LastUpdated { get; set; }
    }
}