using DAL.Models;

namespace BLL.Services
{
    public interface IUserService
    {
        Task<List<User>> GetUsers();
        Task<User?> Login(string email, string password);
        Task<User> Register(string fullName, string email, string password, string phone);
        Task<bool> VerifyEmail(string email);
        Task<bool> ResetPassword(string email, string newPassword);
        Task<User?> GetUserByEmail(string email);
        Task<bool> IsEmailRegistered(string email);
        Task UpdateUser(User user);
        Task DeleteUser(User user);
        Task<int> CleanupUnverifiedUsersOlderThanAsync(DateTime threshold, CancellationToken cancellationToken = default);
        string HashPassword(string password);
        bool VerifyPassword(string password, string hash);
    }
}