using DAL.Models;
using Microsoft.EntityFrameworkCore;

namespace DAL.Repositories.Implementation;

public class PackageRepository : IPackageRepository
{
    private readonly RealEstateListingPlatformContext _context;

    public PackageRepository(RealEstateListingPlatformContext context)
    {
        _context = context;
    }

    // Package operations
    public async Task<List<ListingPackage>> GetAllPackagesAsync()
    {
        return await _context.ListingPackages
            .OrderBy(p => p.Price)
            .ToListAsync();
    }

    public async Task<List<ListingPackage>> GetActivePackagesAsync()
    {
        return await _context.ListingPackages
            .Where(p => p.IsActive)
            .OrderBy(p => p.Price)
            .ToListAsync();
    }

    public async Task<ListingPackage?> GetPackageByIdAsync(Guid id)
    {
        return await _context.ListingPackages.FindAsync(id);
    }

    public async Task<List<ListingPackage>> GetPackagesByTypeAsync(string packageType)
    {
        return await _context.ListingPackages
            .Where(p => p.PackageType == packageType && p.IsActive)
            .OrderBy(p => p.Price)
            .ToListAsync();
    }

    public async Task<ListingPackage> CreatePackageAsync(ListingPackage package)
    {
        package.Id = Guid.NewGuid();
        package.CreatedAt = DateTime.UtcNow;
        package.UpdatedAt = DateTime.UtcNow;

        await _context.ListingPackages.AddAsync(package);
        await _context.SaveChangesAsync();
        return package;
    }

    public async Task UpdatePackageAsync(ListingPackage package)
    {
        package.UpdatedAt = DateTime.UtcNow;
        _context.ListingPackages.Update(package);
        await _context.SaveChangesAsync();
    }

    public async Task DeletePackageAsync(Guid id)
    {
        var package = await GetPackageByIdAsync(id);
        if (package != null)
        {
            package.IsActive = false;
            await UpdatePackageAsync(package);
        }
    }

    // User package operations
    public async Task<List<UserPackage>> GetUserPackagesAsync(Guid userId)
    {
        return await _context.UserPackages
            .Include(up => up.Package)
            .Include(up => up.Transaction)
            .Where(up => up.UserId == userId)
            .OrderByDescending(up => up.PurchasedAt)
            .ToListAsync();
    }

    public async Task<List<UserPackage>> GetActiveUserPackagesAsync(Guid userId)
    {
        var now = DateTime.UtcNow;
        return await _context.UserPackages
            .Include(up => up.Package)
            .Where(up => up.UserId == userId 
                      && up.IsActive 
                      && up.Status == "Active"
                      && (up.ExpiresAt == null || up.ExpiresAt > now))
            .OrderByDescending(up => up.PurchasedAt)
            .ToListAsync();
    }

    public async Task<(List<UserPackage> Items, int TotalCount)> GetUserPackagesFilteredAsync(
        Guid userId,
        string? searchTerm,
        string? status,
        string? packageType,
        DateTime? purchasedAfter,
        DateTime? purchasedBefore,
        string sortBy,
        string sortOrder,
        int pageNumber,
        int pageSize)
    {
        var query = _context.UserPackages
            .Include(up => up.Package)
            .Include(up => up.Transaction)
            .Where(up => up.UserId == userId)
            .AsQueryable();

        // Apply search filter
        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            searchTerm = searchTerm.ToLower();
            query = query.Where(up =>
                up.Package.Name.ToLower().Contains(searchTerm) ||
                (up.Package.Description != null && up.Package.Description.ToLower().Contains(searchTerm)));
        }

        // Apply status filter
        if (!string.IsNullOrWhiteSpace(status))
        {
            query = query.Where(up => up.Status == status);
        }

        // Apply package type filter
        if (!string.IsNullOrWhiteSpace(packageType))
        {
            query = query.Where(up => up.Package.PackageType == packageType);
        }

        // Apply date range filter
        if (purchasedAfter.HasValue)
        {
            query = query.Where(up => up.PurchasedAt >= purchasedAfter.Value);
        }
        if (purchasedBefore.HasValue)
        {
            query = query.Where(up => up.PurchasedAt <= purchasedBefore.Value);
        }

        // Get total count before pagination
        int totalCount = await query.CountAsync();

        // Apply sorting
        query = sortBy.ToLower() switch
        {
            "name" => sortOrder.ToLower() == "asc" 
                ? query.OrderBy(up => up.Package.Name) 
                : query.OrderByDescending(up => up.Package.Name),
            "price" => sortOrder.ToLower() == "asc" 
                ? query.OrderBy(up => up.Package.Price) 
                : query.OrderByDescending(up => up.Package.Price),
            "status" => sortOrder.ToLower() == "asc" 
                ? query.OrderBy(up => up.Status) 
                : query.OrderByDescending(up => up.Status),
            "expiresat" => sortOrder.ToLower() == "asc" 
                ? query.OrderBy(up => up.ExpiresAt) 
                : query.OrderByDescending(up => up.ExpiresAt),
            _ => sortOrder.ToLower() == "asc" 
                ? query.OrderBy(up => up.PurchasedAt) 
                : query.OrderByDescending(up => up.PurchasedAt)
        };

        // Apply pagination
        var items = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, totalCount);
    }

    public async Task<UserPackage?> GetUserPackageByIdAsync(Guid id)
    {
        return await _context.UserPackages.FindAsync(id);
    }

    public async Task<UserPackage?> GetUserPackageWithDetailsAsync(Guid id)
    {
        return await _context.UserPackages
            .Include(up => up.Package)
            .Include(up => up.Transaction)
            .Include(up => up.User)
            .FirstOrDefaultAsync(up => up.Id == id);
    }

    public async Task<UserPackage> CreateUserPackageAsync(UserPackage userPackage)
    {
        userPackage.Id = Guid.NewGuid();
        userPackage.PurchasedAt = DateTime.UtcNow;

        await _context.UserPackages.AddAsync(userPackage);
        await _context.SaveChangesAsync();
        return userPackage;
    }

    public async Task UpdateUserPackageAsync(UserPackage userPackage)
    {
        _context.UserPackages.Update(userPackage);
        await _context.SaveChangesAsync();
    }

    public async Task<bool> HasActivePackageAsync(Guid userId, string packageType)
    {
        var now = DateTime.UtcNow;
        return await _context.UserPackages
            .AnyAsync(up => up.UserId == userId 
                         && up.Package.PackageType == packageType
                         && up.IsActive
                         && up.Status == "Active"
                         && (up.ExpiresAt == null || up.ExpiresAt > now));
    }

    public async Task<List<UserPackage>> GetUserPackagesByTransactionIdAsync(Guid transactionId)
    {
        return await _context.UserPackages
            .Include(up => up.Package)
            .Where(up => up.TransactionId == transactionId)
            .ToListAsync();
    }

    // Boost operations
    public async Task<List<ListingBoost>> GetActiveBoostsAsync()
    {
        var now = DateTime.UtcNow;
        return await _context.ListingBoosts
            .Include(b => b.Listing)
            .Where(b => b.IsActive && b.Status == "Active" && b.EndDate > now)
            .OrderByDescending(b => b.StartDate)
            .ToListAsync();
    }

    public async Task<List<ListingBoost>> GetBoostsByListingIdAsync(Guid listingId)
    {
        return await _context.ListingBoosts
            .Include(b => b.UserPackage)
            .Where(b => b.ListingId == listingId)
            .OrderByDescending(b => b.StartDate)
            .ToListAsync();
    }

    public async Task<ListingBoost?> GetActiveBoostByListingIdAsync(Guid listingId)
    {
        var now = DateTime.UtcNow;
        return await _context.ListingBoosts
            .Where(b => b.ListingId == listingId 
                     && b.IsActive 
                     && b.Status == "Active" 
                     && b.EndDate > now)
            .OrderByDescending(b => b.StartDate)
            .FirstOrDefaultAsync();
    }

    public async Task<ListingBoost> CreateBoostAsync(ListingBoost boost)
    {
        boost.Id = Guid.NewGuid();
        boost.CreatedAt = DateTime.UtcNow;

        await _context.ListingBoosts.AddAsync(boost);
        await _context.SaveChangesAsync();
        return boost;
    }

    public async Task UpdateBoostAsync(ListingBoost boost)
    {
        _context.ListingBoosts.Update(boost);
        await _context.SaveChangesAsync();
    }

    public async Task ExpireOldBoostsAsync()
    {
        var now = DateTime.UtcNow;
        var expiredBoosts = await _context.ListingBoosts
            .Where(b => b.IsActive && b.EndDate <= now && b.Status == "Active")
            .ToListAsync();

        foreach (var boost in expiredBoosts)
        {
            boost.Status = "Expired";
            boost.IsActive = false;
        }

        await _context.SaveChangesAsync();
    }

    // Statistics Methods for Admin Dashboard
    public async Task<PackageStatistics> GetPackageStatisticsAsync()
    {
        var allUserPackages = await _context.UserPackages
            .Include(up => up.Package)
            .Include(up => up.Transaction)
            .ToListAsync();

        var stats = new PackageStatistics
        {
            TotalPurchased = allUserPackages.Count,
            BoostPackages = allUserPackages.Count(up => up.Package.PackageType == "BOOST_LISTING"),
            PhotoPacks = allUserPackages.Count(up => up.Package.PackageType == "PHOTO_PACK"),
            AdditionalListings = allUserPackages.Count(up => up.Package.PackageType == "ADDITIONAL_LISTING"),
            TotalRevenue = allUserPackages
                .Where(up => up.Transaction != null && up.Transaction.Status == "Completed")
                .Sum(up => up.Transaction!.Amount)
        };

        return stats;
    }
}


