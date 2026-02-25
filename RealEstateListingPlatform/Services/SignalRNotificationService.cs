using Microsoft.AspNetCore.SignalR;
using RealEstateListingPlatform.Hubs;

namespace RealEstateListingPlatform.Services;

public interface ISignalRNotificationService
{
    Task SendToUserAsync(Guid userId, string title, string message, string type = "info", string? url = null);
    Task SendToRoleAsync(string role, string title, string message, string type = "info", string? url = null);
    Task SendToAllAsync(string title, string message, string type = "info", string? url = null);
    Task NotifyNewLeadAsync(Guid listerId, string propertyTitle, string seekerName);
    Task NotifyListingApprovedAsync(Guid listerId, string propertyTitle);
    Task NotifyListingRejectedAsync(Guid listerId, string propertyTitle, string reason);
    Task NotifyPackageExpiringAsync(Guid userId, string packageName, int daysRemaining);
}

public class SignalRNotificationService : ISignalRNotificationService
{
    private readonly IHubContext<NotificationHub> _hubContext;

    public SignalRNotificationService(IHubContext<NotificationHub> hubContext)
    {
        _hubContext = hubContext;
    }

    public async Task SendToUserAsync(Guid userId, string title, string message, string type = "info", string? url = null)
    {
        await _hubContext.Clients.Group($"User_{userId}").SendAsync("ReceiveNotification", new
        {
            id = Guid.NewGuid().ToString(),
            title,
            message,
            type,
            url,
            timestamp = DateTime.UtcNow
        });
    }

    public async Task SendToRoleAsync(string role, string title, string message, string type = "info", string? url = null)
    {
        await _hubContext.Clients.Group($"Role_{role}").SendAsync("ReceiveNotification", new
        {
            id = Guid.NewGuid().ToString(),
            title,
            message,
            type,
            url,
            timestamp = DateTime.UtcNow
        });
    }

    public async Task SendToAllAsync(string title, string message, string type = "info", string? url = null)
    {
        await _hubContext.Clients.All.SendAsync("ReceiveNotification", new
        {
            id = Guid.NewGuid().ToString(),
            title,
            message,
            type,
            url,
            timestamp = DateTime.UtcNow
        });
    }

    public async Task NotifyNewLeadAsync(Guid listerId, string propertyTitle, string seekerName)
    {
        await SendToUserAsync(
            listerId,
            "New Lead Received! 🎉",
            $"{seekerName} is interested in your property: {propertyTitle}",
            "success",
            "/Lister/Customers"
        );
    }

    public async Task NotifyListingApprovedAsync(Guid listerId, string propertyTitle)
    {
        await SendToUserAsync(
            listerId,
            "Listing Approved! ✅",
            $"Your property listing '{propertyTitle}' has been approved and is now live!",
            "success",
            "/Lister/Listings"
        );
    }

    public async Task NotifyListingRejectedAsync(Guid listerId, string propertyTitle, string reason)
    {
        await SendToUserAsync(
            listerId,
            "Listing Needs Review ⚠️",
            $"Your property listing '{propertyTitle}' was rejected. Reason: {reason}",
            "warning",
            "/Lister/Listings"
        );
    }

    public async Task NotifyPackageExpiringAsync(Guid userId, string packageName, int daysRemaining)
    {
        await SendToUserAsync(
            userId,
            "Package Expiring Soon! ⏰",
            $"Your {packageName} package will expire in {daysRemaining} day(s). Renew now to keep your benefits!",
            "warning",
            "/Package/MyPackages"
        );
    }
}
