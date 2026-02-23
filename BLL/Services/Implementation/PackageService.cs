using BLL.DTOs;
using BLL.Services;
using DAL.Models;
using DAL.Repositories;

namespace BLL.Services.Implementation;

public class PackageService : IPackageService
{
    private readonly IPackageRepository _packageRepository;
    private readonly IUserRepository _userRepository;
    private readonly IListingRepository _listingRepository;
    private readonly IPaymentService _paymentService;

    public PackageService(
        IPackageRepository packageRepository,
        IUserRepository userRepository,
        IListingRepository listingRepository,
        IPaymentService paymentService)
    {
        _packageRepository = packageRepository;
        _userRepository = userRepository;
        _listingRepository = listingRepository;
        _paymentService = paymentService;
    }

    // Package management
    public async Task<ServiceResult<List<PackageDto>>> GetAllPackagesAsync()
    {
        var packages = await _packageRepository.GetAllPackagesAsync();
        var dtos = packages.Select(MapToDto).ToList();
        return ServiceResult<List<PackageDto>>.SuccessResult(dtos);
    }

    public async Task<ServiceResult<List<PackageDto>>> GetActivePackagesAsync()
    {
        var packages = await _packageRepository.GetActivePackagesAsync();
        var dtos = packages.Select(MapToDto).ToList();
        return ServiceResult<List<PackageDto>>.SuccessResult(dtos);
    }

    public async Task<ServiceResult<PackageDto>> GetPackageByIdAsync(Guid id)
    {
        var package = await _packageRepository.GetPackageByIdAsync(id);
        if (package == null)
            return ServiceResult<PackageDto>.FailureResult("Package not found");

        return ServiceResult<PackageDto>.SuccessResult(MapToDto(package));
    }

    public async Task<ServiceResult<List<PackageDto>>> GetPackagesByTypeAsync(string packageType)
    {
        var packages = await _packageRepository.GetPackagesByTypeAsync(packageType);
        var dtos = packages.Select(MapToDto).ToList();
        return ServiceResult<List<PackageDto>>.SuccessResult(dtos);
    }

    public async Task<ServiceResult<PackageDto>> CreatePackageAsync(PackageDto dto)
    {
        var package = new ListingPackage
        {
            Name = dto.Name,
            Description = dto.Description,
            PackageType = dto.PackageType,
            Price = dto.Price,
            DurationDays = dto.DurationDays,
            ListingCount = dto.ListingCount,
            PhotoLimit = dto.PhotoLimit,
            AllowVideo = dto.AllowVideo,
            BoostDays = dto.BoostDays,
            IsActive = dto.IsActive
        };

        var created = await _packageRepository.CreatePackageAsync(package);
        return ServiceResult<PackageDto>.SuccessResult(MapToDto(created), "Package created successfully");
    }

    public async Task<ServiceResult<PackageDto>> UpdatePackageAsync(Guid id, PackageDto dto)
    {
        var package = await _packageRepository.GetPackageByIdAsync(id);
        if (package == null)
            return ServiceResult<PackageDto>.FailureResult("Package not found");

        package.Name = dto.Name;
        package.Description = dto.Description;
        package.PackageType = dto.PackageType;
        package.Price = dto.Price;
        package.DurationDays = dto.DurationDays;
        package.ListingCount = dto.ListingCount;
        package.PhotoLimit = dto.PhotoLimit;
        package.AllowVideo = dto.AllowVideo;
        package.BoostDays = dto.BoostDays;
        package.IsActive = dto.IsActive;

        await _packageRepository.UpdatePackageAsync(package);
        return ServiceResult<PackageDto>.SuccessResult(MapToDto(package), "Package updated successfully");
    }

    public async Task<ServiceResult<bool>> DeletePackageAsync(Guid id)
    {
        var package = await _packageRepository.GetPackageByIdAsync(id);
        if (package == null)
            return ServiceResult<bool>.FailureResult("Package not found");

        await _packageRepository.DeletePackageAsync(id);
        return ServiceResult<bool>.SuccessResult(true, "Package deleted successfully");
    }

    // User package operations
    public async Task<ServiceResult<List<UserPackageDto>>> GetUserPackagesAsync(Guid userId)
    {
        var userPackages = await _packageRepository.GetUserPackagesAsync(userId);
        var dtos = userPackages.Select(MapToUserPackageDto).ToList();
        return ServiceResult<List<UserPackageDto>>.SuccessResult(dtos);
    }

    public async Task<ServiceResult<List<UserPackageDto>>> GetActiveUserPackagesAsync(Guid userId)
    {
        var userPackages = await _packageRepository.GetActiveUserPackagesAsync(userId);
        var dtos = userPackages.Select(MapToUserPackageDto).ToList();
        return ServiceResult<List<UserPackageDto>>.SuccessResult(dtos);
    }

    public async Task<ServiceResult<PaginatedResult<UserPackageDto>>> GetUserPackagesFilteredAsync(Guid userId, PackageFilterParameters parameters)
    {
        var (items, totalCount) = await _packageRepository.GetUserPackagesFilteredAsync(
            userId,
            parameters.SearchTerm,
            parameters.Status,
            parameters.PackageType,
            parameters.PurchasedAfter,
            parameters.PurchasedBefore,
            parameters.SortBy,
            parameters.SortOrder,
            parameters.PageNumber,
            parameters.PageSize);

        var dtos = items.Select(MapToUserPackageDto).ToList();

        var paginatedResult = new PaginatedResult<UserPackageDto>
        {
            Items = dtos,
            PageNumber = parameters.PageNumber,
            PageSize = parameters.PageSize,
            TotalCount = totalCount
        };

        return ServiceResult<PaginatedResult<UserPackageDto>>.SuccessResult(paginatedResult);
    }

    public async Task<ServiceResult<UserPackageDto>> GetUserPackageByIdAsync(Guid id)
    {
        var userPackage = await _packageRepository.GetUserPackageWithDetailsAsync(id);
        if (userPackage == null)
            return ServiceResult<UserPackageDto>.FailureResult("User package not found");

        return ServiceResult<UserPackageDto>.SuccessResult(MapToUserPackageDto(userPackage));
    }

    public async Task<ServiceResult<UserPackageDto>> PurchasePackageAsync(Guid userId, PurchasePackageDto dto)
    {
        var package = await _packageRepository.GetPackageByIdAsync(dto.PackageId);
        if (package == null)
            return ServiceResult<UserPackageDto>.FailureResult("Package not found");

        if (!package.IsActive)
            return ServiceResult<UserPackageDto>.FailureResult("Package is not available");

        // Create transaction
        var transactionDto = new CreateTransactionDto
        {
            UserId = userId,
            PackageId = package.Id,
            TransactionType = "PACKAGE_PURCHASE",
            Amount = package.Price,
            PaymentMethod = dto.PaymentMethod,
            Notes = dto.Notes
        };

        var transactionResult = await _paymentService.CreateTransactionAsync(transactionDto);
        if (!transactionResult.Success || transactionResult.Data == null)
            return ServiceResult<UserPackageDto>.FailureResult("Failed to create transaction");

        // Create user package with Pending status (activated after payment)
        var userPackage = new UserPackage
        {
            UserId = userId,
            PackageId = package.Id,
            TransactionId = transactionResult.Data.Id,
            RemainingListings = package.ListingCount,
            RemainingPhotos = package.PhotoLimit,
            VideoAvailable = package.AllowVideo,
            RemainingBoosts = package.PackageType == "BOOST_LISTING" ? 1 : null,
            ExpiresAt = package.DurationDays.HasValue 
                ? DateTime.UtcNow.AddDays(package.DurationDays.Value) 
                : null,
            Status = "Pending" // Changed from "Active" - will be activated after payment
        };

        var created = await _packageRepository.CreateUserPackageAsync(userPackage);
        var detailedPackage = await _packageRepository.GetUserPackageWithDetailsAsync(created.Id);

        return ServiceResult<UserPackageDto>.SuccessResult(
            MapToUserPackageDto(detailedPackage!), 
            "Package reserved. Please complete payment to activate.");
    }

    public async Task<ServiceResult<bool>> ActivateUserPackageAsync(Guid transactionId)
    {
        try
        {
            // Find user package by transaction ID
            var userPackages = await _packageRepository.GetUserPackagesByTransactionIdAsync(transactionId);
            
            if (userPackages == null || !userPackages.Any())
                return ServiceResult<bool>.FailureResult("No packages found for this transaction");

            foreach (var userPackage in userPackages)
            {
                // Only activate if currently pending
                if (userPackage.Status == "Pending")
                {
                    userPackage.Status = "Active";
                    userPackage.PurchasedAt = DateTime.UtcNow;
                    
                    await _packageRepository.UpdateUserPackageAsync(userPackage);
                }
            }

            return ServiceResult<bool>.SuccessResult(true, "Package(s) activated successfully");
        }
        catch (Exception ex)
        {
            return ServiceResult<bool>.FailureResult($"Failed to activate package: {ex.Message}");
        }
    }

    public async Task<ServiceResult<bool>> ApplyPackageToListingAsync(Guid userId, ApplyPackageDto dto)
    {
        var userPackage = await _packageRepository.GetUserPackageWithDetailsAsync(dto.UserPackageId);
        if (userPackage == null)
            return ServiceResult<bool>.FailureResult("User package not found");

        if (userPackage.UserId != userId)
            return ServiceResult<bool>.FailureResult("Unauthorized");

        if (!userPackage.IsActive || userPackage.Status != "Active")
            return ServiceResult<bool>.FailureResult("Package is not active");

        var listing = await _listingRepository.GetByIdAsync(dto.ListingId);
        if (listing == null)
            return ServiceResult<bool>.FailureResult("Listing not found");

        if (listing.ListerId != userId)
            return ServiceResult<bool>.FailureResult("Unauthorized");

        // Determine package type and apply benefits accordingly
        var packageType = userPackage.Package.PackageType;
        
        switch (packageType)
        {
            case "PHOTO_PACK":
                // Apply photo package - upgrade photo limit
                if (userPackage.Package.PhotoLimit.HasValue)
                {
                    listing.MaxPhotos = Math.Max(listing.MaxPhotos, userPackage.Package.PhotoLimit.Value);
                }
                
                // Decrease remaining photos if tracked
                if (userPackage.RemainingPhotos.HasValue && userPackage.RemainingPhotos > 0)
                {
                    userPackage.RemainingPhotos--;
                    if (userPackage.RemainingPhotos <= 0)
                        userPackage.Status = "Used";
                    
                    await _packageRepository.UpdateUserPackageAsync(userPackage);
                }
                break;

            case "VIDEO_UPLOAD":
                // Apply video package - enable video
                listing.AllowVideo = true;
                
                // Mark video as consumed
                userPackage.VideoAvailable = false;
                userPackage.Status = "Used";
                await _packageRepository.UpdateUserPackageAsync(userPackage);
                break;

            case "ADDITIONAL_LISTING":
                // Apply listing package - this is typically done during listing creation
                if (userPackage.RemainingListings.HasValue && userPackage.RemainingListings > 0)
                {
                    listing.UserPackageId = userPackage.Id;
                    listing.IsFreeListingorder = false;
                    
                    // Apply any photo/video benefits from the listing package
                    if (userPackage.Package.PhotoLimit.HasValue)
                    {
                        listing.MaxPhotos = Math.Max(listing.MaxPhotos, userPackage.Package.PhotoLimit.Value);
                    }
                    if (userPackage.VideoAvailable)
                    {
                        listing.AllowVideo = true;
                    }
                    
                    // Decrease remaining listings
                    userPackage.RemainingListings--;
                    if (userPackage.RemainingListings <= 0)
                        userPackage.Status = "Used";
                    
                    await _packageRepository.UpdateUserPackageAsync(userPackage);
                }
                else
                {
                    return ServiceResult<bool>.FailureResult("No remaining listings in package");
                }
                break;

            default:
                return ServiceResult<bool>.FailureResult($"Package type '{packageType}' cannot be applied to listings");
        }

        // Update the listing with merged benefits
        await _listingRepository.UpdateAsync(listing);

        return ServiceResult<bool>.SuccessResult(true, $"{packageType} package applied successfully");
    }

    public async Task<ServiceResult<bool>> CanUserCreateListingAsync(Guid userId)
    {
        var user = await _userRepository.GetUserById(userId);
        if (user == null)
            return ServiceResult<bool>.FailureResult("User not found");

        // Check active free listings
        var listings = await _listingRepository.GetListingsByListerIdAsync(userId);
        var activeFreeListings = listings.Count(l => 
            l.IsFreeListingorder && 
            (l.Status == "Published" || l.Status == "PendingReview") &&
            (!l.ExpirationDate.HasValue || l.ExpirationDate > DateTime.UtcNow));

        // Check if user has available free listing slot
        if (activeFreeListings < user.MaxFreeListings)
            return ServiceResult<bool>.SuccessResult(true, "Can create free listing");

        // Check for active packages with remaining listings
        var activePackages = await _packageRepository.GetActiveUserPackagesAsync(userId);
        var hasAvailablePackage = activePackages.Any(up => 
            up.Package.PackageType == "ADDITIONAL_LISTING" && 
            up.RemainingListings.HasValue && 
            up.RemainingListings > 0);

        if (hasAvailablePackage)
            return ServiceResult<bool>.SuccessResult(true, "Has available package");

        return ServiceResult<bool>.FailureResult(
            "No available listing slots. Please purchase an additional listing package.");
    }

    public async Task<ServiceResult<bool>> ConsumeListingSlotAsync(Guid userPackageId)
    {
        var userPackage = await _packageRepository.GetUserPackageWithDetailsAsync(userPackageId);
        if (userPackage == null)
            return ServiceResult<bool>.FailureResult("User package not found");

        if (!userPackage.IsActive || userPackage.Status != "Active")
            return ServiceResult<bool>.FailureResult("Package is not active");

        if (!userPackage.RemainingListings.HasValue || userPackage.RemainingListings <= 0)
            return ServiceResult<bool>.FailureResult("No remaining listings in package");

        // Decrease remaining listings
        userPackage.RemainingListings--;
        
        // If no more listings remaining, mark as used
        if (userPackage.RemainingListings <= 0)
            userPackage.Status = "Used";

        await _packageRepository.UpdateUserPackageAsync(userPackage);

        return ServiceResult<bool>.SuccessResult(true, "Listing slot consumed successfully");
    }

    public async Task<ServiceResult<bool>> RefundListingSlotAsync(Guid userPackageId)
    {
        var userPackage = await _packageRepository.GetUserPackageWithDetailsAsync(userPackageId);
        if (userPackage == null)
            return ServiceResult<bool>.FailureResult("User package not found");

        // Check if package is expired
        if (userPackage.ExpiresAt.HasValue && userPackage.ExpiresAt < DateTime.UtcNow)
            return ServiceResult<bool>.FailureResult("Cannot refund - package has expired");

        // Restore listing slot
        if (userPackage.RemainingListings.HasValue)
        {
            userPackage.RemainingListings++;
        }
        else
        {
            // If RemainingListings was null, initialize it to 1
            userPackage.RemainingListings = 1;
        }

        // If package was marked as "Used", reactivate it if still valid
        if (userPackage.Status == "Used" && userPackage.RemainingListings > 0)
        {
            userPackage.Status = "Active";
        }

        await _packageRepository.UpdateUserPackageAsync(userPackage);

        return ServiceResult<bool>.SuccessResult(true, "Listing slot refunded successfully");
    }

    public async Task<ServiceResult<int>> GetAvailablePhotosForListingAsync(Guid userId, Guid? listingId = null)
    {
        if (listingId.HasValue)
        {
            var listing = await _listingRepository.GetByIdAsync(listingId.Value);
            if (listing != null)
                return ServiceResult<int>.SuccessResult(listing.MaxPhotos);
        }

        // Check for photo pack packages
        var activePackages = await _packageRepository.GetActiveUserPackagesAsync(userId);
        var photoPackage = activePackages
            .FirstOrDefault(up => up.Package.PackageType == "PHOTO_PACK" && up.RemainingPhotos > 0);

        if (photoPackage != null)
            return ServiceResult<int>.SuccessResult(photoPackage.RemainingPhotos ?? 15);

        return ServiceResult<int>.SuccessResult(5); // Default free tier
    }

    // Boost operations
    public async Task<ServiceResult<ListingBoost>> BoostListingAsync(Guid userId, BoostListingDto dto)
    {
        var listing = await _listingRepository.GetByIdAsync(dto.ListingId);
        if (listing == null)
            return ServiceResult<ListingBoost>.FailureResult("Listing not found");

        if (listing.ListerId != userId)
            return ServiceResult<ListingBoost>.FailureResult("Unauthorized");

        // Validate listing status - only Published listings can be boosted
        if (listing.Status != "Published")
            return ServiceResult<ListingBoost>.FailureResult($"Only published listings can be boosted. Current status: {listing.Status}");

        // Check if already boosted
        var existingBoost = await _packageRepository.GetActiveBoostByListingIdAsync(dto.ListingId);
        if (existingBoost != null)
            return ServiceResult<ListingBoost>.FailureResult("Listing is already boosted");

        UserPackage? userPackage = null;
        if (dto.UserPackageId.HasValue)
        {
            userPackage = await _packageRepository.GetUserPackageWithDetailsAsync(dto.UserPackageId.Value);
            if (userPackage == null || userPackage.UserId != userId)
                return ServiceResult<ListingBoost>.FailureResult("Invalid package");

            if (!userPackage.RemainingBoosts.HasValue || userPackage.RemainingBoosts <= 0)
                return ServiceResult<ListingBoost>.FailureResult("No boost credits remaining");
        }

        var boost = new ListingBoost
        {
            ListingId = dto.ListingId,
            UserId = userId,
            UserPackageId = dto.UserPackageId,
            StartDate = DateTime.UtcNow,
            EndDate = DateTime.UtcNow.AddDays(dto.BoostDays),
            BoostDays = dto.BoostDays,
            IsActive = true,
            Status = "Active"
        };

        var created = await _packageRepository.CreateBoostAsync(boost);

        // Update listing
        listing.IsBoosted = true;
        await _listingRepository.UpdateAsync(listing);

        // Decrease boost credits
        if (userPackage != null)
        {
            userPackage.RemainingBoosts--;
            await _packageRepository.UpdateUserPackageAsync(userPackage);
        }

        return ServiceResult<ListingBoost>.SuccessResult(created, "Listing boosted successfully");
    }

    public async Task<ServiceResult<List<ListingBoost>>> GetActiveBoostsAsync()
    {
        var boosts = await _packageRepository.GetActiveBoostsAsync();
        return ServiceResult<List<ListingBoost>>.SuccessResult(boosts);
    }

    public async Task<ServiceResult<ListingBoost?>> GetActiveBoostForListingAsync(Guid listingId)
    {
        var boost = await _packageRepository.GetActiveBoostByListingIdAsync(listingId);
        return ServiceResult<ListingBoost?>.SuccessResult(boost);
    }

    public async Task ExpireOldBoostsAsync()
    {
        await _packageRepository.ExpireOldBoostsAsync();
    }

    // Validation
    public async Task<ServiceResult<bool>> ValidatePackageLimitsAsync(Guid userId, Guid listingId)
    {
        var listing = await _listingRepository.GetByIdAsync(listingId);
        if (listing == null)
            return ServiceResult<bool>.FailureResult("Listing not found");

        var media = await _listingRepository.GetMediaByListingIdAsync(listingId);
        var photoCount = media.Count(m => m.MediaType == "image");
        var hasVideo = media.Any(m => m.MediaType == "video");

        if (photoCount > listing.MaxPhotos)
            return ServiceResult<bool>.FailureResult($"Photo limit exceeded. Maximum {listing.MaxPhotos} photos allowed.");

        if (hasVideo && !listing.AllowVideo)
            return ServiceResult<bool>.FailureResult("Video upload not allowed for this listing. Purchase video package.");

        return ServiceResult<bool>.SuccessResult(true);
    }

    public async Task<ServiceResult<bool>> HasActiveBoostAsync(Guid listingId)
    {
        var boost = await _packageRepository.GetActiveBoostByListingIdAsync(listingId);
        return ServiceResult<bool>.SuccessResult(boost != null);
    }

    // Helper methods
    private PackageDto MapToDto(ListingPackage package)
    {
        return new PackageDto
        {
            Id = package.Id,
            Name = package.Name,
            Description = package.Description,
            PackageType = package.PackageType,
            Price = package.Price,
            DurationDays = package.DurationDays,
            ListingCount = package.ListingCount,
            PhotoLimit = package.PhotoLimit,
            AllowVideo = package.AllowVideo,
            BoostDays = package.BoostDays,
            IsActive = package.IsActive
        };
    }

    private UserPackageDto MapToUserPackageDto(UserPackage userPackage)
    {
        return new UserPackageDto
        {
            Id = userPackage.Id,
            UserId = userPackage.UserId,
            TransactionId = userPackage.TransactionId,
            Package = MapToDto(userPackage.Package),
            RemainingListings = userPackage.RemainingListings,
            RemainingPhotos = userPackage.RemainingPhotos,
            VideoAvailable = userPackage.VideoAvailable,
            RemainingBoosts = userPackage.RemainingBoosts,
            PurchasedAt = userPackage.PurchasedAt,
            ExpiresAt = userPackage.ExpiresAt,
            Status = userPackage.Status
        };
    }
}
