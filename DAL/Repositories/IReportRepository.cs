using DAL.Models;

namespace DAL.Repositories
{
    public interface IReportRepository
    {
        Task<int> GetTotalReportsCountAsync();
        Task<int> GetReportsCountByStatusAsync(string status);
        Task<int> GetUrgentReportsCountAsync();
    }
}
