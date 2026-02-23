using DAL.Models;
using Microsoft.EntityFrameworkCore;

namespace DAL.Repositories.Implementation
{
    public class UserRepository : IUserRepository
    {
        private readonly RealEstateListingPlatformContext _context;

        public UserRepository(RealEstateListingPlatformContext context)
        {
            _context = context;
        }

        public async Task<List<User>> GetUsers()
        {
            return await _context.Users.ToListAsync();
        }

        public async Task<User?> GetUserByEmail(string email)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
        }

        public async Task<User?> GetUserById(Guid id)
        {
            return await _context.Users.FindAsync(id);
        }

        public async Task AddUser(User user)
        {
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateUser(User user)
        {
            _context.Users.Update(user);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteUser(User user)
        {
            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> UserExists(string email)
        {
            return await _context.Users.AnyAsync(u => u.Email == email);
        }

        public IQueryable<User> GetUsersQueryable()
        {
            return _context.Users.AsQueryable();
        }

        public async Task<int> DeleteUnverifiedUsersOlderThanAsync(DateTime threshold, CancellationToken cancellationToken = default)
        {
            return await _context.Users
                .Where(u => !u.IsEmailVerified && u.CreatedAt < threshold)
                .ExecuteDeleteAsync(cancellationToken);
        }

        // Statistics Methods for Admin Dashboard
        public async Task<int> GetTotalUsersCountAsync()
        {
            return await _context.Users.CountAsync(u => u.Role != "Admin");
        }

        public async Task<int> GetNewUsersCountAsync(DateTime startDate)
        {
            return await _context.Users
                .Where(u => u.CreatedAt >= startDate && u.Role != "Admin")
                .CountAsync();
        }

        public async Task<int> GetActiveListersCountAsync()
        {
            return await _context.Users
                .Where(u => u.Role == "Lister" && u.IsActive)
                .CountAsync();
        }

        public async Task<int> GetActiveSeekersCountAsync()
        {
            return await _context.Users
                .Where(u => u.Role == "Seeker" && u.IsActive)
                .CountAsync();
        }

        public async Task<int> GetVerifiedUsersCountAsync()
        {
            return await _context.Users
                .Where(u => u.IsEmailVerified && u.Role != "Admin")
                .CountAsync();
        }

        public async Task<int> GetUnverifiedUsersCountAsync()
        {
            return await _context.Users
                .Where(u => !u.IsEmailVerified && u.Role != "Admin")
                .CountAsync();
        }

        public async Task<List<(DateTime Date, int Count)>> GetUserRegistrationsOverTimeAsync(int days)
        {
            var startDate = DateTime.UtcNow.AddDays(-days).Date;

            var registrations = await _context.Users
                .Where(u => u.CreatedAt >= startDate && u.Role != "Admin")
                .GroupBy(u => u.CreatedAt.Date)
                .Select(g => new { Date = g.Key, Count = g.Count() })
                .OrderBy(x => x.Date)
                .ToListAsync();

            return registrations.Select(x => (x.Date, x.Count)).ToList();
        }
    }
}

