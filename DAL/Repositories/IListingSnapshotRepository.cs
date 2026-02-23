using DAL.Models;
using System;
using System.Threading.Tasks;

namespace DAL.Repositories
{
    public interface IListingSnapshotRepository
    {
        Task<ListingSnapshot> CreateSnapshotAsync(Listing listing);
        Task<ListingSnapshot?> GetSnapshotByIdAsync(Guid snapshotId);
        Task<ListingSnapshot?> GetPendingSnapshotForListingAsync(Guid listingId);
        Task DeleteSnapshotAsync(Guid snapshotId);
    }
}
