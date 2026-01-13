using project_cripta.Models;
using System;
using System.Data.Entity;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace project_cripta.Services
{
    public interface IUserService
    {
        Task<bool> RegisterUserAsync(RegisterViewModel model);
        Task<User> AuthenticateUserAsync(LoginViewModel model);
        Task<bool> UserExistsAsync(string email);
        Task<User> GetUserByIdAsync(int userId);
        Task<decimal> GetUserBalanceAsync(int userId);
        Task<bool> AddMoneyAsync(int userId, decimal amount);
    }

    public class UserService : IUserService
    {
        private readonly ApplicationDbContext _context;

        public UserService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<bool> RegisterUserAsync(RegisterViewModel model)
        {
            if (await UserExistsAsync(model.Email))
                return false;

            var user = new User
            {
                FirstName = model.FirstName,
                Email = model.Email,
                PasswordHash = HashPassword(model.Password),
                CreatedAt = DateTime.Now
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<User> AuthenticateUserAsync(LoginViewModel model)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == model.Email);

            if (user == null || !VerifyPassword(model.Password, user.PasswordHash))
                return null;

            return user;
        }

        public async Task<bool> UserExistsAsync(string email)
        {
            return await _context.Users.AnyAsync(u => u.Email == email);
        }

        public async Task<User> GetUserByIdAsync(int userId)
        {
            return await _context.Users.FindAsync(userId);
        }

        private string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var bytes = Encoding.UTF8.GetBytes(password);
                var hash = sha256.ComputeHash(bytes);
                return Convert.ToBase64String(hash);
            }
        }

        private bool VerifyPassword(string password, string storedHash)
        {
            var hash = HashPassword(password);
            return hash == storedHash;
        }

        public async Task<decimal> GetUserBalanceAsync(int userId)
        {
            var user = await _context.Users.FindAsync(userId);
            return user?.Balance ?? 0;
        }

        public async Task<bool> AddMoneyAsync(int userId, decimal amount)
        {
            try
            {
                var user = await _context.Users.FindAsync(userId);
                if (user == null) return false;

                user.Balance += amount;
                user.UpdatedAt = DateTime.Now;

                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"AddMoney error: {ex}");
                return false;
            }
        }
    }
}