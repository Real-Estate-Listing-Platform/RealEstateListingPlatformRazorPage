using BLL.DTOs;
using DAL.Models;
using DAL.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BLL.Services.Implementation
{
    public class AuditLogService : IAuditLogService
    {
        private readonly IAuditLogRepository _auditLogRepository;

        public AuditLogService(IAuditLogRepository auditLogRepository)
        {
            _auditLogRepository = auditLogRepository;
        }

        public async Task<List<AuditLogDto>> GetRecentAuditLogsAsync(int count = 50)
        {
            var logs = await _auditLogRepository.GetRecentAuditLogsAsync(count);
            return logs.Select(MapToDto).ToList();
        }

        public async Task<(List<AuditLogDto> Logs, int TotalCount)> GetAuditLogsPaginatedAsync(int pageNumber, int pageSize)
        {
            var (logs, totalCount) = await _auditLogRepository.GetAuditLogsPaginatedAsync(pageNumber, pageSize);
            var dtos = logs.Select(MapToDto).ToList();
            return (dtos, totalCount);
        }

        public async Task<List<AuditLogDto>> GetAuditLogsByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            var logs = await _auditLogRepository.GetAuditLogsByDateRangeAsync(startDate, endDate);
            return logs.Select(MapToDto).ToList();
        }

        public async Task<List<AuditLogDto>> GetAuditLogsByUserAsync(Guid userId, int count = 50)
        {
            var logs = await _auditLogRepository.GetAuditLogsByUserAsync(userId, count);
            return logs.Select(MapToDto).ToList();
        }

        public async Task<List<AuditLogDto>> GetAuditLogsByActionTypeAsync(string actionType, int count = 50)
        {
            var logs = await _auditLogRepository.GetAuditLogsByActionTypeAsync(actionType, count);
            return logs.Select(MapToDto).ToList();
        }

        public async Task<List<AuditLogDto>> GetAuditLogsByTargetAsync(string targetType, Guid targetId)
        {
            var logs = await _auditLogRepository.GetAuditLogsByTargetAsync(targetType, targetId);
            return logs.Select(MapToDto).ToList();
        }

        public async Task<AuditLogDto?> GetAuditLogByIdAsync(Guid id)
        {
            var log = await _auditLogRepository.GetAuditLogByIdAsync(id);
            return log != null ? MapToDto(log) : null;
        }

        public async Task LogActionAsync(string actionType, Guid? actorUserId, string? targetType, Guid? targetId, string? oldValues, string? newValues, string? ipAddress)
        {
            var auditLog = new AuditLog
            {
                ActionType = actionType,
                ActorUserId = actorUserId,
                TargetType = targetType,
                TargetId = targetId,
                OldValues = oldValues,
                NewValues = newValues,
                Ipaddress = ipAddress
            };

            await _auditLogRepository.CreateAuditLogAsync(auditLog);
        }

        public async Task<int> GetTotalAuditLogsCountAsync()
        {
            return await _auditLogRepository.GetTotalAuditLogsCountAsync();
        }

        private AuditLogDto MapToDto(AuditLog log)
        {
            return new AuditLogDto
            {
                Id = log.Id,
                ActorUserId = log.ActorUserId,
                ActorUserName = log.ActorUser?.DisplayName ?? "System",
                ActionType = log.ActionType ?? "Unknown",
                TargetType = log.TargetType,
                TargetId = log.TargetId,
                OldValues = log.OldValues,
                NewValues = log.NewValues,
                IpAddress = log.Ipaddress,
                CreatedAt = log.CreatedAt ?? DateTime.UtcNow
            };
        }
    }
}
