using BLL.DTOs;
using BLL.Services;
using DAL.Models;
using DAL.Repositories;

namespace BLL.Services.Implementation

{
    public class NotificationService : INotificationService
    {
        private readonly INotificationRepository _notificationRepository;

        public NotificationService(INotificationRepository notificationRepository)
        {
            _notificationRepository = notificationRepository;
        }

        public async Task<ServiceResult<Notification>> CreateNotificationAsync(Guid userId, string title, string message, string type, string? relatedLink = null)
        {
            try
            {
                var notification = new Notification
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    Title = title,
                    Message = message,
                    Type = type,
                    RelatedLink = relatedLink,
                    IsRead = false,
                    CreatedAt = DateTime.UtcNow
                };

                var created = await _notificationRepository.CreateNotificationAsync(notification);

                return new ServiceResult<Notification>
                {
                    Success = true,
                    Data = created
                };
            }
            catch (Exception ex)
            {
                return new ServiceResult<Notification>
                {
                    Success = false,
                    Message = $"Failed to create notification: {ex.Message}"
                };
            }
        }

        public async Task<ServiceResult<List<Notification>>> GetMyNotificationsAsync(Guid userId, bool unreadOnly = false)
        {
            try
            {
                var notifications = await _notificationRepository.GetNotificationsByUserIdAsync(userId, unreadOnly);
                return new ServiceResult<List<Notification>>
                {
                    Success = true,
                    Data = notifications
                };
            }
            catch (Exception ex)
            {
                return new ServiceResult<List<Notification>>
                {
                    Success = false,
                    Message = $"Failed to retrieve notifications: {ex.Message}"
                };
            }
        }

        public async Task<ServiceResult<bool>> MarkAsReadAsync(Guid notificationId, Guid userId)
        {
            try
            {
                var notification = await _notificationRepository.GetNotificationByIdAsync(notificationId);
                if (notification == null || notification.UserId != userId)
                {
                    return new ServiceResult<bool>
                    {
                        Success = false,
                        Message = "Notification not found or access denied."
                    };
                }

                await _notificationRepository.MarkAsReadAsync(notificationId);

                return new ServiceResult<bool>
                {
                    Success = true,
                    Data = true
                };
            }
            catch (Exception ex)
            {
                return new ServiceResult<bool>
                {
                    Success = false,
                    Message = $"Failed to mark notification as read: {ex.Message}"
                };
            }
        }

        public async Task<ServiceResult<int>> GetUnreadCountAsync(Guid userId)
        {
            try
            {
                var count = await _notificationRepository.GetUnreadCountAsync(userId);
                return new ServiceResult<int>
                {
                    Success = true,
                    Data = count
                };
            }
            catch (Exception ex)
            {
                return new ServiceResult<int>
                {
                    Success = false,
                    Message = $"Failed to get unread count: {ex.Message}"
                };
            }
        }

        public async Task NotifyNewLeadAsync(Guid listerId, Guid leadId, string listingTitle, string seekerName)
        {
            try
            {
                var title = "New Lead Received";
                var message = $"{seekerName} has expressed interest in your property: {listingTitle}";
                var type = "Lead";
                var relatedLink = $"/Lister/Leads?highlight={leadId}";

                await CreateNotificationAsync(listerId, title, message, type, relatedLink);
            }
            catch (Exception)
            {
                // Log error but don't throw - notification failure shouldn't break lead creation
            }
        }
    }
}
