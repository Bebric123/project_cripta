using project_cripta.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace project_cripta.Services
{
    public interface IBalanceService
    {
        Task<UserBalance> GetUserBalanceAsync(int userId, string currency);
        Task<List<UserBalance>> GetAllUserBalancesAsync(int userId);
        Task<bool> UpdateBalanceAsync(int userId, string currency, decimal amountChange);
        Task<bool> CreateExchangeTransactionAsync(int userId, string fromCurrency, string toCurrency,
            decimal fromAmount, decimal toAmount, decimal exchangeRate, decimal fee);
        Task InitializeUserBalancesAsync(int userId);
    }
}
