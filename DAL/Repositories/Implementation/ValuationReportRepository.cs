using DAL.Models;
using Microsoft.EntityFrameworkCore;

namespace DAL.Repositories.Implementation
{
    public class ValuationReportRepository : IValuationReportRepository
    {
        private readonly RealEstateListingPlatformContext _context;

        public ValuationReportRepository(RealEstateListingPlatformContext context)
        {
            _context = context;
        }

        public async Task<ValuationReport> CreateAsync(ValuationReport report)
        {
            report.Id = Guid.NewGuid();
            report.CreatedAt = DateTime.UtcNow;
            await _context.ValuationReports.AddAsync(report);
            await _context.SaveChangesAsync();
            return report;
        }

        public async Task<List<ValuationReport>> GetByUserIdAsync(Guid userId) =>
            await _context.ValuationReports
                .Where(r => r.UserId == userId)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();

        public async Task<ValuationReport?> GetByIdAsync(Guid id) =>
            await _context.ValuationReports
                .Include(r => r.User)
                .FirstOrDefaultAsync(r => r.Id == id);

        public async Task<List<ValuationReport>> GetByIdsAsync(IEnumerable<Guid> ids) =>
            await _context.ValuationReports
                .Where(r => ids.Contains(r.Id))
                .OrderBy(r => r.CreatedAt)
                .ToListAsync();

        public async Task<bool> DeleteAsync(Guid id, Guid userId)
        {
            var report = await _context.ValuationReports
                .FirstOrDefaultAsync(r => r.Id == id && r.UserId == userId);

            if (report == null) return false;

            _context.ValuationReports.Remove(report);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task UpdateNameAsync(Guid id, Guid userId, string newName)
        {
            var report = await _context.ValuationReports
                .FirstOrDefaultAsync(r => r.Id == id && r.UserId == userId);

            if (report == null) return;

            report.ReportName = newName;
            await _context.SaveChangesAsync();
        }
    }
}
