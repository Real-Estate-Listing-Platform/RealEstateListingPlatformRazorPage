using DAL.Models;

namespace DAL.Repositories;

public interface IPackageRepository
{
    // Package operations
    Task<List<ListingPackage>> GetAllPackagesAsync();
    Task<List<ListingPackage>> GetActivePackagesAsync();
    Task<ListingPackage?> GetPackageByIdAsync(Guid id);
    Task<List<ListingPackage>> GetPackagesByTypeAsync(string packageType);
    Task<ListingPackage> CreatePackageAsync(ListingPackage package);
    Task UpdatePackageAsync(ListingPackage package);
    Task DeletePackageAsync(Guid id);

    // User package operations
    Task<List<UserPackage>> GetUserPackagesAsync(Guid userId);
    Task<List<UserPackage>> GetActiveUserPackagesAsync(Guid userId);
    Task<(List<UserPackage> Items, int TotalCount)> GetUserPackagesFilteredAsync(Guid userId, string? searchTerm, string? status, string? packageType, DateTime? purchasedAfter, DateTime? purchasedBefore, string sortBy, string sortOrder, int pageNumber, int pageSize);
    Task<UserPackage?> GetUserPackageByIdAsync(Guid id);
    Task<UserPackage> CreateUserPackageAsync(UserPackage userPackage);
    Task UpdateUserPackageAsync(UserPackage userPackage);
    Task<bool> HasActivePackageAsync(Guid userId, string packageType);
    Task<UserPackage?> GetUserPackageWithDetailsAsync(Guid id);
    Task<List<UserPackage>> GetUserPackagesByTransactionIdAsync(Guid transactionId);

    // Boost operations
    Task<List<ListingBoost>> GetActiveBoostsAsync();
    Task<List<ListingBoost>> GetBoostsByListingIdAsync(Guid listingId);
    Task<ListingBoost?> GetActiveBoostByListingIdAsync(Guid listingId);
    Task<ListingBoost> CreateBoostAsync(ListingBoost boost);
    Task UpdateBoostAsync(ListingBoost boost);
    Task ExpireOldBoostsAsync();

    // Statistics Methods for Admin Dashboard
    Task<PackageStatistics> GetPackageStatisticsAsync();
}

public class PackageStatistics
{
    public int TotalPurchased { get; set; }
    public int BoostPackages { get; set; }
    public int PhotoPacks { get; set; }
    public int AdditionalListings { get; set; }
    public decimal TotalRevenue { get; set; }
}

