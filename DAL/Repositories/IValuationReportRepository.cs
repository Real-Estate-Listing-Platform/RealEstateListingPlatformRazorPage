using DAL.Models;

namespace DAL.Repositories
{
    public interface IValuationReportRepository
    {
        Task<ValuationReport> CreateAsync(ValuationReport report);
        Task<List<ValuationReport>> GetByUserIdAsync(Guid userId);
        Task<ValuationReport?> GetByIdAsync(Guid id);
        Task<List<ValuationReport>> GetByIdsAsync(IEnumerable<Guid> ids);
        Task<bool> DeleteAsync(Guid id, Guid userId);
        Task UpdateNameAsync(Guid id, Guid userId, string newName);
    }
}
