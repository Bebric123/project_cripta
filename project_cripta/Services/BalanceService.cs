using project_cripta.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;

namespace project_cripta.Services
{
    public class BalanceService : IBalanceService
    {
        private readonly ApplicationDbContext _context;

        public BalanceService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<UserBalance> GetUserBalanceAsync(int userId, string currency)
        {
            return await _context.UserBalances
                .FirstOrDefaultAsync(b => b.UserId == userId && b.Currency == currency);
        }

        public async Task<List<UserBalance>> GetAllUserBalancesAsync(int userId)
        {
            return await _context.UserBalances
                .Where(b => b.UserId == userId)
                .ToListAsync();
        }

        public async Task<bool> UpdateBalanceAsync(int userId, string currency, decimal amountChange)
        {
            var balance = await GetUserBalanceAsync(userId, currency);

            if (balance == null)
            {
                balance = new UserBalance
                {
                    UserId = userId,
                    Currency = currency,
                    Amount = amountChange,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                };
                _context.UserBalances.Add(balance);
            }
            else
            {
                if (amountChange < 0 && balance.Amount < Math.Abs(amountChange))
                {
                    return false;
                }

                balance.Amount += amountChange;
                balance.UpdatedAt = DateTime.Now;
            }

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> CreateExchangeTransactionAsync(int userId, string fromCurrency, string toCurrency,
            decimal fromAmount, decimal toAmount, decimal exchangeRate, decimal fee)
        {
            var fromSuccess = await UpdateBalanceAsync(userId, fromCurrency, -fromAmount);
            if (!fromSuccess) return false;

            var toSuccess = await UpdateBalanceAsync(userId, toCurrency, toAmount - fee);
            if (!toSuccess)
            {
                await UpdateBalanceAsync(userId, fromCurrency, fromAmount);
                return false;
            }

            var transactionRecord = new Transaction
            {
                UserId = userId,
                Type = "EXCHANGE",
                FromCurrency = fromCurrency,
                ToCurrency = toCurrency,
                FromAmount = fromAmount,
                ToAmount = toAmount,
                ExchangeRate = exchangeRate,
                Fee = fee,
                CreatedAt = DateTime.Now
            };

            _context.Transactions.Add(transactionRecord);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task InitializeUserBalancesAsync(int userId)
        {
            var existingBalances = await _context.UserBalances
                .Where(b => b.UserId == userId)
                .ToListAsync();

            if (existingBalances.Any())
            {
                return;
            }

            var defaultBalances = new[]
            {
                new { Currency = "USDT", Amount = 0.00m },
                new { Currency = "ETH", Amount = 0.00m },
                new { Currency = "BTC", Amount = 0.00m },
                new { Currency = "SUI", Amount = 0.00m }
            };

            foreach (var balance in defaultBalances)
            {
                var newBalance = new UserBalance
                {
                    UserId = userId,
                    Currency = balance.Currency,
                    Amount = balance.Amount,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                };
                _context.UserBalances.Add(newBalance);
            }

            await _context.SaveChangesAsync();
        }
    }
}