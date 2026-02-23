using BLL.DTOs;
using DAL.Models;

namespace BLL.Services;

public interface IPackageService
{
    // Package management
    Task<ServiceResult<List<PackageDto>>> GetAllPackagesAsync();
    Task<ServiceResult<List<PackageDto>>> GetActivePackagesAsync();
    Task<ServiceResult<PackageDto>> GetPackageByIdAsync(Guid id);
    Task<ServiceResult<List<PackageDto>>> GetPackagesByTypeAsync(string packageType);
    Task<ServiceResult<PackageDto>> CreatePackageAsync(PackageDto dto);
    Task<ServiceResult<PackageDto>> UpdatePackageAsync(Guid id, PackageDto dto);
    Task<ServiceResult<bool>> DeletePackageAsync(Guid id);

    // User package operations
    Task<ServiceResult<List<UserPackageDto>>> GetUserPackagesAsync(Guid userId);
    Task<ServiceResult<List<UserPackageDto>>> GetActiveUserPackagesAsync(Guid userId);
    Task<ServiceResult<PaginatedResult<UserPackageDto>>> GetUserPackagesFilteredAsync(Guid userId, PackageFilterParameters parameters);
    Task<ServiceResult<UserPackageDto>> GetUserPackageByIdAsync(Guid id);
    Task<ServiceResult<UserPackageDto>> PurchasePackageAsync(Guid userId, PurchasePackageDto dto);
    Task<ServiceResult<bool>> ActivateUserPackageAsync(Guid transactionId);
    Task<ServiceResult<bool>> ApplyPackageToListingAsync(Guid userId, ApplyPackageDto dto);
    Task<ServiceResult<bool>> CanUserCreateListingAsync(Guid userId);
    Task<ServiceResult<int>> GetAvailablePhotosForListingAsync(Guid userId, Guid? listingId = null);
    Task<ServiceResult<bool>> ConsumeListingSlotAsync(Guid userPackageId);
    Task<ServiceResult<bool>> RefundListingSlotAsync(Guid userPackageId);

    // Boost operations
    Task<ServiceResult<ListingBoost>> BoostListingAsync(Guid userId, BoostListingDto dto);
    Task<ServiceResult<List<ListingBoost>>> GetActiveBoostsAsync();
    Task<ServiceResult<ListingBoost?>> GetActiveBoostForListingAsync(Guid listingId);
    Task ExpireOldBoostsAsync();

    // Validation
    Task<ServiceResult<bool>> ValidatePackageLimitsAsync(Guid userId, Guid listingId);
    Task<ServiceResult<bool>> HasActiveBoostAsync(Guid listingId);
}
