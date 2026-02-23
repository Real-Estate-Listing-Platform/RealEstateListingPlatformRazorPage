using BLL.DTOs;
using DAL.Models;

namespace BLL.Services

{
    public interface INotificationService
    {
        Task<ServiceResult<Notification>> CreateNotificationAsync(Guid userId, string title, string message, string type, string? relatedLink = null);
        Task<ServiceResult<List<Notification>>> GetMyNotificationsAsync(Guid userId, bool unreadOnly = false);
        Task<ServiceResult<bool>> MarkAsReadAsync(Guid notificationId, Guid userId);
        Task<ServiceResult<int>> GetUnreadCountAsync(Guid userId);
        Task NotifyNewLeadAsync(Guid listerId, Guid leadId, string listingTitle, string seekerName);
    }
}
