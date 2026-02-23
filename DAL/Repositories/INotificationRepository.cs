using DAL.Models;

namespace DAL.Repositories
{
    public interface INotificationRepository
    {
        Task<Notification> CreateNotificationAsync(Notification notification);
        Task<List<Notification>> GetNotificationsByUserIdAsync(Guid userId, bool unreadOnly = false);
        Task<Notification?> GetNotificationByIdAsync(Guid id);
        Task MarkAsReadAsync(Guid notificationId);
        Task<int> GetUnreadCountAsync(Guid userId);
    }
}
