using Microsoft.AspNetCore.SignalR;

namespace BLL.Hubs
{
    public interface IDashboardClient
    {
        Task ReceiveDashboardUpdate(string eventType, object data);
    }

    public class DashboardHub : Hub<IDashboardClient>
    {
        public const string PublicListingsGroup = "public-listings";

        public async Task JoinListerGroup(string listerId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"lister-{listerId}");
        }

        public async Task LeaveListerGroup(string listerId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"lister-{listerId}");
        }

        public async Task JoinListingGroup(string listingId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"listing-{listingId}");
        }

        public async Task LeaveListingGroup(string listingId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"listing-{listingId}");
        }

        public async Task JoinPublicListingsGroup()
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, PublicListingsGroup);
        }

        public async Task LeavePublicListingsGroup()
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, PublicListingsGroup);
        }
    }
}
