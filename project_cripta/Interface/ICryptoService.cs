using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using project_cripta.Models;

namespace project_cripta.Services
{
    public interface ICryptoService
    {
        Task<List<CryptoModel>> GetCryptoCurrenciesAsync(int page = 1, int perPage = 20);
        Task<decimal?> GetCryptoPriceAsync(string cryptoId);
        Task<CryptoModel> GetCryptoDetailAsync(string cryptoId);
    }
}