using DAL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Repositories
{
    public interface IListingRepository
    {
        // Read operations
        Task<List<Listing>> GetListings();
        Task<IEnumerable<Listing>> GetPendingListingsAsync();
        Task<IEnumerable<Listing>> GetPublishedListingsAsync();
        Task<Listing?> GetByIdAsync(Guid id);
        Task<Listing?> GetListingWithMediaAsync(Guid id);
        Task<List<Listing>> GetListingsByListerIdAsync(Guid listerId);
        Task<(List<Listing> Items, int TotalCount)> GetListingsFilteredAsync(Guid listerId, string? searchTerm, string? status, string? transactionType, string? propertyType, string? city, string? district, decimal? minPrice, decimal? maxPrice, string sortBy, string sortOrder, int pageNumber, int pageSize);
        
        // Create
        Task<Listing> CreateAsync(Listing listing);
        
        // Update
        Task UpdateAsync(Listing listing);
        
        // Delete (Hard delete)
        Task<bool> DeleteAsync(Guid id);
        
    // Media Management
    Task AddMediaAsync(Guid listingId, ListingMedia media);
    Task<List<ListingMedia>> GetMediaByListingIdAsync(Guid listingId);
    Task DeleteMediaAsync(Guid mediaId);
        
        // Validation Helpers
        Task<bool> ExistsAsync(Guid id);
        Task<bool> IsOwnerAsync(Guid listingId, Guid userId);

        // Statistics Methods for Admin Dashboard
        Task<int> GetTotalListingsCountAsync();
        Task<int> GetListingsCountByStatusAsync(string status);
        Task<decimal> GetAverageListingPriceAsync();
        Task<int> GetBoostedListingsCountAsync();
        Task<Dictionary<string, int>> GetListingsCountByStatusAsync();
        Task<List<(DateTime Date, int Count)>> GetListingsCreatedOverTimeAsync(int days);
        Task<List<(Guid ListingId, string Title, int LeadCount, decimal Price, string ListerName)>> GetTopPerformingListingsAsync(int topCount);

        // Valuation estimation – returns Published listings matching type/txType/location
        Task<List<Listing>> GetListingsForValuationAsync(string propertyType, string transactionType, string city, string? district);

        // Market analytics – returns Published listings for a city within the given month window
        Task<List<Listing>> GetListingsForMarketAnalyticsAsync(string city, string? propertyType, string? transactionType, int months);

        // City comparison – returns Published listings for multiple cities
        Task<List<Listing>> GetListingsForCityComparisonAsync(IEnumerable<string> cities, string? propertyType, string? transactionType, int months);
    }
}
