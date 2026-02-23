using DAL.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DAL.Repositories
{
    public interface IAuditLogRepository
    {
        Task<List<AuditLog>> GetRecentAuditLogsAsync(int count = 50);
        Task<(List<AuditLog> Logs, int TotalCount)> GetAuditLogsPaginatedAsync(int pageNumber, int pageSize);
        Task<List<AuditLog>> GetAuditLogsByDateRangeAsync(DateTime startDate, DateTime endDate);
        Task<List<AuditLog>> GetAuditLogsByUserAsync(Guid userId, int count = 50);
        Task<List<AuditLog>> GetAuditLogsByActionTypeAsync(string actionType, int count = 50);
        Task<List<AuditLog>> GetAuditLogsByTargetAsync(string targetType, Guid targetId);
        Task<AuditLog?> GetAuditLogByIdAsync(Guid id);
        Task<AuditLog> CreateAuditLogAsync(AuditLog auditLog);
        Task<int> GetTotalAuditLogsCountAsync();
    }
}
