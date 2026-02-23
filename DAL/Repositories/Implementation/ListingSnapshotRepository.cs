using DAL.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace DAL.Repositories.Implementation
{
    public class ListingSnapshotRepository : IListingSnapshotRepository
    {
        private readonly RealEstateListingPlatformContext _context;

        public ListingSnapshotRepository(RealEstateListingPlatformContext context)
        {
            _context = context;
        }

        public async Task<ListingSnapshot> CreateSnapshotAsync(Listing listing)
        {
            // Serialize media URLs
            var mediaUrls = listing.ListingMedia?
                .OrderBy(m => m.SortOrder ?? int.MaxValue)
                .Select(m => m.Url)
                .ToList() ?? new List<string?>();
            
            var snapshot = new ListingSnapshot
            {
                Id = Guid.NewGuid(),
                ListingId = listing.Id,
                CreatedAt = DateTime.UtcNow,
                Title = listing.Title,
                Description = listing.Description,
                TransactionType = listing.TransactionType,
                PropertyType = listing.PropertyType,
                Price = listing.Price,
                StreetName = listing.StreetName,
                Ward = listing.Ward,
                District = listing.District,
                City = listing.City,
                Area = listing.Area,
                HouseNumber = listing.HouseNumber,
                Latitude = listing.Latitude,
                Longitude = listing.Longitude,
                Bedrooms = listing.Bedrooms,
                Bathrooms = listing.Bathrooms,
                Floors = listing.Floors,
                LegalStatus = listing.LegalStatus,
                FurnitureStatus = listing.FurnitureStatus,
                Direction = listing.Direction,
                MediaUrlsJson = JsonSerializer.Serialize(mediaUrls)
            };

            await _context.ListingSnapshots.AddAsync(snapshot);
            await _context.SaveChangesAsync();

            return snapshot;
        }

        public async Task<ListingSnapshot?> GetSnapshotByIdAsync(Guid snapshotId)
        {
            return await _context.ListingSnapshots
                .Include(s => s.Listing)
                    .ThenInclude(l => l.ListingMedia)
                .FirstOrDefaultAsync(s => s.Id == snapshotId);
        }

        public async Task<ListingSnapshot?> GetPendingSnapshotForListingAsync(Guid listingId)
        {
            var listing = await _context.Listings
                .Include(l => l.PendingSnapshot)
                .FirstOrDefaultAsync(l => l.Id == listingId);

            return listing?.PendingSnapshot;
        }

        public async Task DeleteSnapshotAsync(Guid snapshotId)
        {
            var snapshot = await _context.ListingSnapshots.FindAsync(snapshotId);
            if (snapshot != null)
            {
                _context.ListingSnapshots.Remove(snapshot);
                await _context.SaveChangesAsync();
            }
        }
    }
}
