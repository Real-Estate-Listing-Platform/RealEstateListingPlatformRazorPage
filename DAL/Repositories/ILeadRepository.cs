using DAL.Models;

namespace DAL.Repositories
{
    public interface ILeadRepository
    {
        Task<Lead?> GetLeadByIdAsync(Guid id);
        Task<List<Lead>> GetLeadsByListerIdAsync(Guid listerId);
        Task<List<Lead>> GetLeadsBySeekerIdAsync(Guid seekerId);
        Task<List<Lead>> GetLeadsByListingIdAsync(Guid listingId);
        Task<Lead?> GetExistingLeadAsync(Guid listingId, Guid seekerId);
        Task<Lead> CreateLeadAsync(Lead lead);
        Task UpdateLeadAsync(Lead lead);
        Task DeleteLeadAsync(Lead lead);
        Task<int> GetLeadCountByListerIdAsync(Guid listerId, string? status = null);
        Task<List<Lead>> GetRecentLeadsByListerIdAsync(Guid listerId, int count = 10);

        // Statistics Methods for Admin Dashboard
        Task<int> GetTotalLeadsCountAsync();
        Task<int> GetNewLeadsCountAsync(DateTime startDate);
        Task<int> GetLeadsCountByStatusAsync(string status);
        Task<Dictionary<string, int>> GetLeadsCountByStatusAsync();
        Task<List<(DateTime Date, int Count)>> GetLeadsGeneratedOverTimeAsync(int days);
    }
}
