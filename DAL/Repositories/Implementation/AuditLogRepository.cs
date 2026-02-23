using DAL.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DAL.Repositories.Implementation
{
    public class AuditLogRepository : IAuditLogRepository
    {
        private readonly RealEstateListingPlatformContext _context;

        public AuditLogRepository(RealEstateListingPlatformContext context)
        {
            _context = context;
        }

        public async Task<List<AuditLog>> GetRecentAuditLogsAsync(int count = 50)
        {
            return await _context.AuditLogs
                .Include(a => a.ActorUser)
                .OrderByDescending(a => a.CreatedAt)
                .Take(count)
                .ToListAsync();
        }

        public async Task<(List<AuditLog> Logs, int TotalCount)> GetAuditLogsPaginatedAsync(int pageNumber, int pageSize)
        {
            var query = _context.AuditLogs
                .Include(a => a.ActorUser)
                .OrderByDescending(a => a.CreatedAt);

            var totalCount = await query.CountAsync();

            var logs = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (logs, totalCount);
        }

        public async Task<List<AuditLog>> GetAuditLogsByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            return await _context.AuditLogs
                .Include(a => a.ActorUser)
                .Where(a => a.CreatedAt >= startDate && a.CreatedAt <= endDate)
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<AuditLog>> GetAuditLogsByUserAsync(Guid userId, int count = 50)
        {
            return await _context.AuditLogs
                .Include(a => a.ActorUser)
                .Where(a => a.ActorUserId == userId)
                .OrderByDescending(a => a.CreatedAt)
                .Take(count)
                .ToListAsync();
        }

        public async Task<List<AuditLog>> GetAuditLogsByActionTypeAsync(string actionType, int count = 50)
        {
            return await _context.AuditLogs
                .Include(a => a.ActorUser)
                .Where(a => a.ActionType == actionType)
                .OrderByDescending(a => a.CreatedAt)
                .Take(count)
                .ToListAsync();
        }

        public async Task<List<AuditLog>> GetAuditLogsByTargetAsync(string targetType, Guid targetId)
        {
            return await _context.AuditLogs
                .Include(a => a.ActorUser)
                .Where(a => a.TargetType == targetType && a.TargetId == targetId)
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync();
        }

        public async Task<AuditLog?> GetAuditLogByIdAsync(Guid id)
        {
            return await _context.AuditLogs
                .Include(a => a.ActorUser)
                .FirstOrDefaultAsync(a => a.Id == id);
        }

        public async Task<AuditLog> CreateAuditLogAsync(AuditLog auditLog)
        {
            auditLog.Id = Guid.NewGuid();
            auditLog.CreatedAt = DateTime.UtcNow;
            
            await _context.AuditLogs.AddAsync(auditLog);
            await _context.SaveChangesAsync();
            
            return auditLog;
        }

        public async Task<int> GetTotalAuditLogsCountAsync()
        {
            return await _context.AuditLogs.CountAsync();
        }
    }
}
