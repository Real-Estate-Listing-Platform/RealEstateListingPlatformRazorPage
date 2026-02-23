using DAL.Models;
using Microsoft.EntityFrameworkCore;

namespace DAL.Repositories.Implementation
{
    public class LeadRepository : ILeadRepository
    {
        private readonly RealEstateListingPlatformContext _context;

        public LeadRepository(RealEstateListingPlatformContext context)
        {
            _context = context;
        }

        public async Task<Lead?> GetLeadByIdAsync(Guid id)
        {
            return await _context.Leads
                .Include(l => l.Listing)
                .Include(l => l.Seeker)
                .Include(l => l.Lister)
                .FirstOrDefaultAsync(l => l.Id == id);
        }

        public async Task<List<Lead>> GetLeadsByListerIdAsync(Guid listerId)
        {
            return await _context.Leads
                .Include(l => l.Listing)
                .Include(l => l.Seeker)
                .Where(l => l.ListerId == listerId)
                .OrderByDescending(l => l.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<Lead>> GetLeadsBySeekerIdAsync(Guid seekerId)
        {
            return await _context.Leads
                .Include(l => l.Listing)
                .Include(l => l.Lister)
                .Where(l => l.SeekerId == seekerId)
                .OrderByDescending(l => l.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<Lead>> GetLeadsByListingIdAsync(Guid listingId)
        {
            return await _context.Leads
                .Include(l => l.Seeker)
                .Where(l => l.ListingId == listingId)
                .OrderByDescending(l => l.CreatedAt)
                .ToListAsync();
        }

        public async Task<Lead?> GetExistingLeadAsync(Guid listingId, Guid seekerId)
        {
            return await _context.Leads
                .FirstOrDefaultAsync(l => l.ListingId == listingId && l.SeekerId == seekerId);
        }

        public async Task<Lead> CreateLeadAsync(Lead lead)
        {
            _context.Leads.Add(lead);
            await _context.SaveChangesAsync();
            return lead;
        }

        public async Task UpdateLeadAsync(Lead lead)
        {
            _context.Leads.Update(lead);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteLeadAsync(Lead lead)
        {
            _context.Leads.Remove(lead);
            await _context.SaveChangesAsync();
        }

        public async Task<int> GetLeadCountByListerIdAsync(Guid listerId, string? status = null)
        {
            var query = _context.Leads.Where(l => l.ListerId == listerId);
            
            if (!string.IsNullOrWhiteSpace(status))
            {
                query = query.Where(l => l.Status == status);
            }
            
            return await query.CountAsync();
        }

        public async Task<List<Lead>> GetRecentLeadsByListerIdAsync(Guid listerId, int count = 10)
        {
            return await _context.Leads
                .Include(l => l.Listing)
                .Include(l => l.Seeker)
                .Where(l => l.ListerId == listerId)
                .OrderByDescending(l => l.CreatedAt)
                .Take(count)
                .ToListAsync();
        }

        // Statistics Methods for Admin Dashboard
        public async Task<int> GetTotalLeadsCountAsync()
        {
            return await _context.Leads.CountAsync();
        }

        public async Task<int> GetNewLeadsCountAsync(DateTime startDate)
        {
            return await _context.Leads
                .Where(l => l.CreatedAt >= startDate)
                .CountAsync();
        }

        public async Task<int> GetLeadsCountByStatusAsync(string status)
        {
            return await _context.Leads
                .CountAsync(l => l.Status == status);
        }

        public async Task<Dictionary<string, int>> GetLeadsCountByStatusAsync()
        {
            return await _context.Leads
                .GroupBy(l => l.Status ?? "Unknown")
                .Select(g => new { Status = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.Status, x => x.Count);
        }

        public async Task<List<(DateTime Date, int Count)>> GetLeadsGeneratedOverTimeAsync(int days)
        {
            var startDate = DateTime.UtcNow.AddDays(-days).Date;

            var leads = await _context.Leads
                .Where(l => l.CreatedAt >= startDate)
                .GroupBy(l => l.CreatedAt!.Value.Date)
                .Select(g => new { Date = g.Key, Count = g.Count() })
                .OrderBy(x => x.Date)
                .ToListAsync();

            return leads.Select(x => (x.Date, x.Count)).ToList();
        }
    }
}

