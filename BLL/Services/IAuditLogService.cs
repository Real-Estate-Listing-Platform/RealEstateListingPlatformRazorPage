using BLL.DTOs;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BLL.Services
{
    public interface IAuditLogService
    {
        Task<List<AuditLogDto>> GetRecentAuditLogsAsync(int count = 50);
        Task<(List<AuditLogDto> Logs, int TotalCount)> GetAuditLogsPaginatedAsync(int pageNumber, int pageSize);
        Task<List<AuditLogDto>> GetAuditLogsByDateRangeAsync(DateTime startDate, DateTime endDate);
        Task<List<AuditLogDto>> GetAuditLogsByUserAsync(Guid userId, int count = 50);
        Task<List<AuditLogDto>> GetAuditLogsByActionTypeAsync(string actionType, int count = 50);
        Task<List<AuditLogDto>> GetAuditLogsByTargetAsync(string targetType, Guid targetId);
        Task<AuditLogDto?> GetAuditLogByIdAsync(Guid id);
        Task LogActionAsync(string actionType, Guid? actorUserId, string? targetType, Guid? targetId, string? oldValues, string? newValues, string? ipAddress);
        Task<int> GetTotalAuditLogsCountAsync();
    }
}
