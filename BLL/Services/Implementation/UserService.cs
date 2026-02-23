using BLL.Services;
using DAL.Models;
using DAL.Repositories;
using System.Security.Cryptography;
using System.Text;

namespace BLL.Services.Implementation
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;

        public UserService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<List<User>> GetUsers()
        {
            return await _userRepository.GetUsers();
        }

        public async Task<User?> Login(string email, string password)
        {
            var user = await _userRepository.GetUserByEmail(email);
            if (user == null || string.IsNullOrEmpty(user.PasswordHash) || !VerifyPassword(password, user.PasswordHash))
            {
                return null;
            }
            return user;
        }

        public async Task<User> Register(string fullName, string email, string password, string phone)
        {
            var user = new User
            {
                DisplayName = fullName,
                Email = email,
                PasswordHash = HashPassword(password),
                Phone = phone,
                Role = "Seeker",
                IsEmailVerified = false,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _userRepository.AddUser(user);
            return user;
        }

        public async Task<bool> VerifyEmail(string email)
        {
            var user = await _userRepository.GetUserByEmail(email);
            if (user == null) return false;

            user.IsEmailVerified = true;
            await _userRepository.UpdateUser(user);
            return true;
        }

        public async Task<bool> ResetPassword(string email, string newPassword)
        {
            var user = await _userRepository.GetUserByEmail(email);
            if (user == null) return false;

            // Note: Password reuse check should be done before calling this if needed
            
            user.PasswordHash = HashPassword(newPassword);
            await _userRepository.UpdateUser(user);
            return true;
        }

        public async Task<User?> GetUserByEmail(string email)
        {
            return await _userRepository.GetUserByEmail(email);
        }

        public async Task<bool> IsEmailRegistered(string email)
        {
            return await _userRepository.UserExists(email);
        }

        public async Task UpdateUser(User user)
        {
            await _userRepository.UpdateUser(user);
        }

        public async Task DeleteUser(User user)
        {
            await _userRepository.DeleteUser(user);
        }

        public async Task<int> CleanupUnverifiedUsersOlderThanAsync(DateTime threshold, CancellationToken cancellationToken = default)
        {
            return await _userRepository.DeleteUnverifiedUsersOlderThanAsync(threshold, cancellationToken);
        }

        public string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return Convert.ToBase64String(hashedBytes);
            }
        }

        public bool VerifyPassword(string password, string hash)
        {
            if (string.IsNullOrEmpty(hash)) return false;
            return HashPassword(password) == hash;
        }
    }
}