using DAL.Models;

namespace DAL.Repositories
{
    public interface IUserRepository
    {
        Task<List<User>> GetUsers();
        Task<User?> GetUserByEmail(string email);
        Task<User?> GetUserById(Guid id);
        Task AddUser(User user);
        Task UpdateUser(User user);
        Task DeleteUser(User user);
        Task<bool> UserExists(string email);
        IQueryable<User> GetUsersQueryable();
        Task<int> DeleteUnverifiedUsersOlderThanAsync(DateTime threshold, CancellationToken cancellationToken = default);

        // Statistics Methods for Admin Dashboard
        Task<int> GetTotalUsersCountAsync();
        Task<int> GetNewUsersCountAsync(DateTime startDate);
        Task<int> GetActiveListersCountAsync();
        Task<int> GetActiveSeekersCountAsync();
        Task<int> GetVerifiedUsersCountAsync();
        Task<int> GetUnverifiedUsersCountAsync();
        Task<List<(DateTime Date, int Count)>> GetUserRegistrationsOverTimeAsync(int days);
    }
}