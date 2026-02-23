namespace BLL.Services
{
    public interface IAuditService
    {
        Task LogAsync(string actionType, Guid actorUserId, Guid targetId, string? targetType = null);
    }
}
