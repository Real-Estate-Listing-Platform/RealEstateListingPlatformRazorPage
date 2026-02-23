using DAL.Models;

namespace BLL.Services.Implementation
{
    public class AuditService : IAuditService
    {
        private readonly RealEstateListingPlatformContext _context;

        public AuditService(RealEstateListingPlatformContext context)
        {
            _context = context;
        }

        public async Task LogAsync(string actionType, Guid actorUserId, Guid targetId, string? targetType = null)
        {
            var auditLog = new AuditLog
            {
                Id = Guid.NewGuid(),
                ActorUserId = actorUserId,
                ActionType = actionType,
                TargetType = targetType ?? "Listing",
                TargetId = targetId,
                CreatedAt = DateTime.UtcNow
            };

            await _context.AuditLogs.AddAsync(auditLog);
            await _context.SaveChangesAsync();
        }
    }
}
