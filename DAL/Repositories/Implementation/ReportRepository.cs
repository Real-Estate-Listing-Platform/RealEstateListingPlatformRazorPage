using DAL.Models;
using Microsoft.EntityFrameworkCore;

namespace DAL.Repositories.Implementation
{
    public class ReportRepository : IReportRepository
    {
        private readonly RealEstateListingPlatformContext _context;

        public ReportRepository(RealEstateListingPlatformContext context)
        {
            _context = context;
        }

        public async Task<int> GetTotalReportsCountAsync()
        {
            return await _context.Reports.CountAsync();
        }

        public async Task<int> GetReportsCountByStatusAsync(string status)
        {
            return await _context.Reports
                .CountAsync(r => r.Status == status);
        }

        public async Task<int> GetUrgentReportsCountAsync()
        {
            // Consider reports as urgent if they're pending and created within last 24 hours
            var urgentThreshold = DateTime.UtcNow.AddHours(-24);
            return await _context.Reports
                .CountAsync(r => r.Status == "Pending" && r.CreatedAt >= urgentThreshold);
        }
    }
}
