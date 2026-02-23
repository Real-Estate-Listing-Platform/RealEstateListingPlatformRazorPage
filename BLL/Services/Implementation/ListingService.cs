using BLL.DTOs;
using DAL.Models;
using DAL.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.Services.Implementation
{
    public class ListingService : IListingService
    {
        private readonly IListingRepository _listingRepository;
        private readonly IPriceHistoryService _priceHistoryService;
        private readonly IAuditService _auditService;
        private readonly IPackageService _packageService;
        private readonly IUserRepository _userRepository;
        private readonly IListingViewRepository _listingViewRepository;
        private readonly IListingSnapshotRepository _listingSnapshotRepository;

        public ListingService(
            IListingRepository listingRepository,
            IPriceHistoryService priceHistoryService,
            IAuditService auditService,
            IPackageService packageService,
            IUserRepository userRepository,
            IListingViewRepository listingViewRepository,
            IListingSnapshotRepository listingSnapshotRepository)
        { 
            _listingRepository = listingRepository;
            _priceHistoryService = priceHistoryService;
            _auditService = auditService;
            _packageService = packageService;
            _userRepository = userRepository;
            _listingViewRepository = listingViewRepository;
            _listingSnapshotRepository = listingSnapshotRepository;
        }

        // Existing methods
        public async Task<List<ListingDto>> GetListings()
        {
            var listings = await _listingRepository.GetListings();
            return listings.Select(MapToDto).ToList();
        }

        public async Task<IEnumerable<ListingDto>> GetPendingListingsAsync()
        {
            var result = await _listingRepository.GetPendingListingsAsync();
            if (result == null)
            {
                return Enumerable.Empty<ListingDto>();
            }
            return result.Select(MapToDto);
        }

        public async Task<IEnumerable<ListingDto>> GetPublishedListingsAsync()
        {
            var result = await _listingRepository.GetPublishedListingsAsync();
            return result?.Select(MapToDto) ?? Enumerable.Empty<ListingDto>();
        }

        public async Task<IEnumerable<ListingDto>> GetPublishedByTypeAsync(string type)
        {
            var listings = await _listingRepository.GetPublishedListingsAsync();
            if (listings == null) return Enumerable.Empty<ListingDto>();
            
            // Filter by transaction type while maintaining boost ordering from repository
            return listings.Where(l => l.TransactionType == type).Select(MapToDto).ToList();
        }

        public async Task<IEnumerable<ListingDto>> GetByTypeAsync(String type)
        {
            var listings = await _listingRepository.GetPendingListingsAsync();
            if (listings == null)
            {
                return Enumerable.Empty<ListingDto>();
            }
            var filteredListings = listings.Where(l => l.TransactionType == type);
            return filteredListings.Select(MapToDto);
        }

        public async Task<ListingDto> GetByIdAsync(Guid id)
        {
            var listing = await _listingRepository.GetByIdAsync(id);
            return MapToDto(listing!);
        }
        
        public async Task<bool> ApproveListingAsync(Guid id)
        {
            var listing = await _listingRepository.GetByIdAsync(id);
            if (listing == null) return false;

            if (listing.Status != "PendingReview")
            {
                return false;
            }

            listing.Status = "Published";
            
            // Clear pending snapshot if exists (changes are now approved)
            if (listing.PendingSnapshotId.HasValue)
            {
                await _listingSnapshotRepository.DeleteSnapshotAsync(listing.PendingSnapshotId.Value);
                listing.PendingSnapshotId = null;
            }
            
            await _listingRepository.UpdateAsync(listing);
            return true;
        }

        public async Task<bool> RejectListingAsync(Guid id)
        {
            var listing = await _listingRepository.GetByIdAsync(id);
            if (listing == null) return false;
            
            if (listing.Status != "PendingReview")
            {
                return false;
            }

            // If this is an edit rejection and we have a snapshot, restore original data
            bool wasEdit = listing.PendingSnapshotId.HasValue;
            if (wasEdit)
            {
                var snapshot = await _listingSnapshotRepository.GetSnapshotByIdAsync(listing.PendingSnapshotId.Value);
                if (snapshot != null)
                {
                    // Restore original values
                    listing.Title = snapshot.Title;
                    listing.Description = snapshot.Description;
                    listing.TransactionType = snapshot.TransactionType;
                    listing.PropertyType = snapshot.PropertyType;
                    listing.Price = snapshot.Price;
                    listing.StreetName = snapshot.StreetName;
                    listing.Ward = snapshot.Ward;
                    listing.District = snapshot.District;
                    listing.City = snapshot.City;
                    listing.Area = snapshot.Area;
                    listing.HouseNumber = snapshot.HouseNumber;
                    listing.Latitude = snapshot.Latitude;
                    listing.Longitude = snapshot.Longitude;
                    listing.Bedrooms = snapshot.Bedrooms;
                    listing.Bathrooms = snapshot.Bathrooms;
                    listing.Floors = snapshot.Floors;
                    listing.LegalStatus = snapshot.LegalStatus;
                    listing.FurnitureStatus = snapshot.FurnitureStatus;
                    listing.Direction = snapshot.Direction;
                    
                    // Restore to Published status since this was an edit
                    listing.Status = "Published";
                    
                    // Clean up snapshot
                    await _listingSnapshotRepository.DeleteSnapshotAsync(listing.PendingSnapshotId.Value);
                    listing.PendingSnapshotId = null;
                }
            }
            else
            {
                // This is a new listing rejection
                // Refund package slot if listing was using a paid package
                if (!listing.IsFreeListingorder && listing.UserPackageId.HasValue)
                {
                    var refundResult = await _packageService.RefundListingSlotAsync(listing.UserPackageId.Value);
                    if (refundResult.Success)
                    {
                        // Log successful refund
                        await _auditService.LogAsync("PackageRefunded", listing.ListerId, listing.Id, "Listing");
                    }
                }

                listing.Status = "Rejected";
            }
            
            await _listingRepository.UpdateAsync(listing);
            
            // Log rejection audit
            await _auditService.LogAsync("ListingRejected", listing.ListerId, listing.Id, "Listing");
            
            return true;
        }

        // Create
        public async Task<ServiceResult<ListingDto>> CreateListingAsync(ListingCreateDto dto, Guid listerId, List<IFormFile>? mediaFiles = null)
        {
            // Validate input
            var validationResult = await ValidateListingDataAsync(dto);
            if (!validationResult.Success)
                return ServiceResult<ListingDto>.FailureResult(validationResult.Message ?? "Validation failed", validationResult.Errors);

            // Check if user can create a listing
            var canCreateResult = await _packageService.CanUserCreateListingAsync(listerId);
            if (!canCreateResult.Success)
                return ServiceResult<ListingDto>.FailureResult(canCreateResult.Message ?? "Cannot create listing");

            // Get user to check free listing availability
            var user = await _userRepository.GetUserById(listerId);
            if (user == null)
                return ServiceResult<ListingDto>.FailureResult("User not found");

            // Check for active free listings
            var userListings = await _listingRepository.GetListingsByListerIdAsync(listerId);
            var activeFreeListings = userListings.Count(l => 
                l.IsFreeListingorder && 
                (l.Status == "Published" || l.Status == "PendingReview") &&
                (!l.ExpirationDate.HasValue || l.ExpirationDate > DateTime.UtcNow));

            bool isFreeListingorder = activeFreeListings < user.MaxFreeListings;
            Guid? userPackageId = null;
            int maxPhotos = 5;
            bool allowVideo = false;
            DateTime expirationDate = DateTime.UtcNow.AddDays(30);

            // If not a free listing, find and consume a package
            if (!isFreeListingorder)
            {
                var activePackages = await _packageService.GetActiveUserPackagesAsync(listerId);
                if (!activePackages.Success || activePackages.Data == null)
                    return ServiceResult<ListingDto>.FailureResult("No available packages found");

                var availablePackage = activePackages.Data
                    .FirstOrDefault(up => 
                        up.Package.PackageType == "ADDITIONAL_LISTING" &&
                        up.Status == "Active" &&
                        up.RemainingListings.HasValue &&
                        up.RemainingListings > 0);

                if (availablePackage == null)
                    return ServiceResult<ListingDto>.FailureResult("No available listing package found. Please purchase an additional listing package.");

                userPackageId = availablePackage.Id;
                maxPhotos = availablePackage.Package.PhotoLimit ?? 5;
                allowVideo = availablePackage.VideoAvailable;
                if (availablePackage.ExpiresAt.HasValue)
                    expirationDate = availablePackage.ExpiresAt.Value;
            }

            // Map DTO to entity
            var listing = new Listing
            {
                ListerId = listerId,
                Title = dto.Title,
                Description = dto.Description,
                TransactionType = dto.TransactionType,
                PropertyType = dto.PropertyType,
                Price = dto.Price,
                StreetName = dto.StreetName,
                Ward = dto.Ward,
                District = dto.District,
                City = dto.City,
                Area = dto.Area,
                HouseNumber = dto.HouseNumber,
                Latitude = dto.Latitude,
                Longitude = dto.Longitude,
                Bedrooms = dto.Bedrooms,
                Bathrooms = dto.Bathrooms,
                Floors = dto.Floors,
                LegalStatus = dto.LegalStatus,
                FurnitureStatus = dto.FurnitureStatus,
                Direction = dto.Direction,
                UserPackageId = userPackageId,
                IsFreeListingorder = isFreeListingorder,
                MaxPhotos = maxPhotos,
                AllowVideo = allowVideo,
                ExpirationDate = expirationDate,
                IsBoosted = false
            };

            // Save to database
            var created = await _listingRepository.CreateAsync(listing);

            // Consume the package if not a free listing
            if (!isFreeListingorder && userPackageId.HasValue)
            {
                var consumeResult = await _packageService.ConsumeListingSlotAsync(userPackageId.Value);
                if (!consumeResult.Success)
                {
                    // Log warning but don't fail the entire operation since listing is already created
                    // In production, you might want to implement compensation logic here
                    await _auditService.LogAsync("PackageConsumptionFailed", listerId, created.Id, "Listing");
                }
            }

            // Handle media uploads
            if (mediaFiles != null && mediaFiles.Any())
            {
                int sortOrder = 0;
                foreach (var file in mediaFiles)
                {
                    var mediaType = file.ContentType.StartsWith("image") ? "image" : "video";
                    var uploadResult = await SaveMediaFileAsync(created.Id, file, mediaType, sortOrder++);
                    
                    if (!uploadResult.Success)
                    {
                        // Log warning but don't fail the entire operation
                        continue;
                    }
                }
            }

            // Log audit trail
            await _auditService.LogAsync("ListingCreated", listerId, listing.Id, "Listing");

            return ServiceResult<ListingDto>.SuccessResult(MapToDto(created), "Listing created successfully");
        }

        // Read (Enhanced)
        public async Task<ServiceResult<ListingDto>> GetListingByIdAsync(Guid id)
        {
            var listing = await _listingRepository.GetByIdAsync(id);
            if (listing == null)
                return ServiceResult<ListingDto>.FailureResult("Listing not found");

            return ServiceResult<ListingDto>.SuccessResult(MapToDto(listing));
        }

        public async Task<ServiceResult<List<ListingDto>>> GetMyListingsAsync(Guid listerId)
        {
            var listings = await _listingRepository.GetListingsByListerIdAsync(listerId);
            return ServiceResult<List<ListingDto>>.SuccessResult(listings.Select(MapToDto).ToList());
        }

        public async Task<ServiceResult<PaginatedResult<ListingDto>>> GetMyListingsFilteredAsync(Guid listerId, ListingFilterParameters parameters)
        {
            var (items, totalCount) = await _listingRepository.GetListingsFilteredAsync(
                listerId,
                parameters.SearchTerm,
                parameters.Status,
                parameters.TransactionType,
                parameters.PropertyType,
                parameters.City,
                parameters.District,
                parameters.MinPrice,
                parameters.MaxPrice,
                parameters.SortBy,
                parameters.SortOrder,
                parameters.PageNumber,
                parameters.PageSize);

            var paginatedResult = new PaginatedResult<ListingDto>
            {
                Items = items.Select(MapToDto).ToList(),
                PageNumber = parameters.PageNumber,
                PageSize = parameters.PageSize,
                TotalCount = totalCount
            };

            return ServiceResult<PaginatedResult<ListingDto>>.SuccessResult(paginatedResult);
        }

        public async Task<ServiceResult<ListingDto>> GetListingWithMediaAsync(Guid id)
        {
            var listing = await _listingRepository.GetListingWithMediaAsync(id);
            if (listing == null)
                return ServiceResult<ListingDto>.FailureResult("Listing not found");

            return ServiceResult<ListingDto>.SuccessResult(MapToDto(listing));
        }

        // Update
        public async Task<ServiceResult<ListingDto>> UpdateListingAsync(Guid id, ListingUpdateDto dto, Guid userId, List<IFormFile>? mediaFiles = null)
        {
            // Check if listing exists
            var listing = await _listingRepository.GetListingWithMediaAsync(id);
            if (listing == null)
                return ServiceResult<ListingDto>.FailureResult("Listing not found");

            // Verify ownership
            if (!await CanUserModifyListingAsync(id, userId))
                return ServiceResult<ListingDto>.FailureResult("You are not authorized to modify this listing");

            // If listing is already published, create a snapshot and set status to PendingReview
            bool wasPublished = listing.Status == "Published";
            if (wasPublished && listing.PendingSnapshotId == null)
            {
                var snapshot = await _listingSnapshotRepository.CreateSnapshotAsync(listing);
                listing.PendingSnapshotId = snapshot.Id;
                listing.Status = "PendingReview";
            }

            // Track price changes
            if (dto.Price.HasValue && dto.Price.Value != listing.Price)
            {
                await _priceHistoryService.RecordPriceChangeAsync(id, listing.Price, dto.Price.Value, userId);
            }

            // Update fields (only update non-null values from DTO)
            if (!string.IsNullOrEmpty(dto.Title)) listing.Title = dto.Title;
            if (dto.Description != null) listing.Description = dto.Description;
            if (!string.IsNullOrEmpty(dto.TransactionType)) listing.TransactionType = dto.TransactionType;
            if (!string.IsNullOrEmpty(dto.PropertyType)) listing.PropertyType = dto.PropertyType;
            if (dto.Price.HasValue) listing.Price = dto.Price.Value;
            if (dto.StreetName != null) listing.StreetName = dto.StreetName;
            if (dto.Ward != null) listing.Ward = dto.Ward;
            if (dto.District != null) listing.District = dto.District;
            if (dto.City != null) listing.City = dto.City;
            if (dto.Area != null) listing.Area = dto.Area;
            if (dto.HouseNumber != null) listing.HouseNumber = dto.HouseNumber;
            if (dto.Latitude.HasValue) listing.Latitude = dto.Latitude;
            if (dto.Longitude.HasValue) listing.Longitude = dto.Longitude;
            if (dto.Bedrooms.HasValue) listing.Bedrooms = dto.Bedrooms;
            if (dto.Bathrooms.HasValue) listing.Bathrooms = dto.Bathrooms;
            if (dto.Floors.HasValue) listing.Floors = dto.Floors;
            if (dto.LegalStatus != null) listing.LegalStatus = dto.LegalStatus;
            if (dto.FurnitureStatus != null) listing.FurnitureStatus = dto.FurnitureStatus;
            if (dto.Direction != null) listing.Direction = dto.Direction;

            await _listingRepository.UpdateAsync(listing);

            // Handle media uploads
            if (mediaFiles != null && mediaFiles.Any())
            {
                var existingMedia = await _listingRepository.GetMediaByListingIdAsync(id);
                int sortOrder = existingMedia.Count;
                
                foreach (var file in mediaFiles)
                {
                    var mediaType = file.ContentType.StartsWith("image") ? "image" : "video";
                    await SaveMediaFileAsync(id, file, mediaType, sortOrder++);
                }
            }

            // Log audit trail
            var auditAction = wasPublished ? "ListingEditedPendingApproval" : "ListingUpdated";
            await _auditService.LogAsync(auditAction, userId, listing.Id, "Listing");

            var message = wasPublished 
                ? "Listing updated and submitted for review. Changes will be visible after admin approval." 
                : "Listing updated successfully";

            return ServiceResult<ListingDto>.SuccessResult(MapToDto(listing), message);
        }

        public async Task<ServiceResult<bool>> SubmitForReviewAsync(Guid id, Guid userId)
        {
            var listing = await _listingRepository.GetByIdAsync(id);
            if (listing == null)
                return ServiceResult<bool>.FailureResult("Listing not found");

            if (!await CanUserModifyListingAsync(id, userId))
                return ServiceResult<bool>.FailureResult("You are not authorized to modify this listing");

            // Allow submission from Draft or from newly created listings
            if (listing.Status != "Draft" && listing.Status != "PendingReview")
                return ServiceResult<bool>.FailureResult("Only draft listings can be submitted for review");

            // Validate required fields
            if (string.IsNullOrEmpty(listing.Title) || listing.Price <= 0)
                return ServiceResult<bool>.FailureResult("Please complete all required fields before submitting");

            listing.Status = "PendingReview";
            await _listingRepository.UpdateAsync(listing);

            await _auditService.LogAsync("ListingSubmittedForReview", userId, id, "Listing");

            return ServiceResult<bool>.SuccessResult(true, "Listing submitted for review");
        }

        // Delete
        public async Task<ServiceResult<bool>> DeleteListingAsync(Guid id, Guid userId, bool isAdmin = false)
        {
            var listing = await _listingRepository.GetListingWithMediaAsync(id);
            if (listing == null)
                return ServiceResult<bool>.FailureResult("Listing not found");

            // Verify ownership or admin role
            if (!isAdmin && !await CanUserModifyListingAsync(id, userId))
                return ServiceResult<bool>.FailureResult("You are not authorized to delete this listing");

            // Delete physical media files from storage
            if (listing.ListingMedia != null && listing.ListingMedia.Any())
            {
                foreach (var media in listing.ListingMedia)
                {
                    DeletePhysicalFile(media.Url);
                }
            }

            // Log audit trail before deletion
            await _auditService.LogAsync("ListingDeleted", userId, id, "Listing");

            // Hard delete
            var deleted = await _listingRepository.DeleteAsync(id);
            if (!deleted)
                return ServiceResult<bool>.FailureResult("Failed to delete listing");

            return ServiceResult<bool>.SuccessResult(true, "Listing deleted successfully");
        }

        // Media Management
        public async Task<ServiceResult<bool>> AddMediaToListingAsync(Guid listingId, IFormFile file, string mediaType)
        {
            // Verify listing exists
            var listing = await _listingRepository.GetByIdAsync(listingId);
            if (listing == null)
                return ServiceResult<bool>.FailureResult("Listing not found");

            // Check video permission
            if (mediaType.ToLower() == "video" && !listing.AllowVideo)
                return ServiceResult<bool>.FailureResult("Video upload not allowed. Please purchase video upload package.");

            // Check photo limit
            var existingMedia = await _listingRepository.GetMediaByListingIdAsync(listingId);
            if (mediaType.ToLower() == "image")
            {
                var photoCount = existingMedia.Count(m => m.MediaType == "image");
                if (photoCount >= listing.MaxPhotos)
                    return ServiceResult<bool>.FailureResult($"Photo limit reached ({listing.MaxPhotos} photos). Purchase photo pack to add more.");
            }

            // Validate file
            if (!IsValidMediaFile(file, mediaType))
                return ServiceResult<bool>.FailureResult("Invalid file type or size. Max 10MB for images/videos.");

            // Get current media count for sort order
            int sortOrder = existingMedia.Count;

            // Save file
            var result = await SaveMediaFileAsync(listingId, file, mediaType, sortOrder);
            return result;
        }

        public async Task<ServiceResult<bool>> DeleteMediaAsync(Guid mediaId, Guid userId)
        {
            var media = await _listingRepository.GetMediaByListingIdAsync(Guid.Empty);
            var targetMedia = media.FirstOrDefault(m => m.Id == mediaId);
            
            if (targetMedia == null)
                return ServiceResult<bool>.FailureResult("Media not found");

            // Verify ownership
            if (!await CanUserModifyListingAsync(targetMedia.ListingId, userId))
                return ServiceResult<bool>.FailureResult("You are not authorized to delete this media");

            // Delete physical file
            DeletePhysicalFile(targetMedia.Url);

            // Delete from database
            await _listingRepository.DeleteMediaAsync(mediaId);

            return ServiceResult<bool>.SuccessResult(true, "Media deleted successfully");
        }

        public async Task<ServiceResult<List<ListingMediaDto>>> GetListingMediaAsync(Guid listingId)
        {
            var media = await _listingRepository.GetMediaByListingIdAsync(listingId);
            var mediaDtos = media.Select(m => new ListingMediaDto
            {
                Id = m.Id,
                ListingId = m.ListingId,
                MediaType = m.MediaType ?? "image",
                Url = m.Url ?? string.Empty,
                SortOrder = m.SortOrder ?? 0
            }).ToList();
            return ServiceResult<List<ListingMediaDto>>.SuccessResult(mediaDtos);
        }

        // Validation
        public async Task<ServiceResult<bool>> ValidateListingDataAsync(ListingCreateDto dto)
        {
            var errors = new List<string>();

            if (string.IsNullOrWhiteSpace(dto.Title))
                errors.Add("Title is required");

            if (string.IsNullOrWhiteSpace(dto.TransactionType))
                errors.Add("Transaction type is required");

            if (string.IsNullOrWhiteSpace(dto.PropertyType))
                errors.Add("Property type is required");

            if (dto.Price <= 0)
                errors.Add("Price must be greater than 0");

            if (errors.Any())
                return ServiceResult<bool>.FailureResult("Validation failed", errors);

            return ServiceResult<bool>.SuccessResult(true);
        }

        public async Task<bool> CanUserModifyListingAsync(Guid listingId, Guid userId)
        {
            return await _listingRepository.IsOwnerAsync(listingId, userId);
        }

        // Helper Methods
        private bool IsValidMediaFile(IFormFile file, string mediaType)
        {
            const long maxFileSize = 10 * 1024 * 1024; // 10MB
            if (file.Length > maxFileSize) return false;

            var allowedExtensions = mediaType.ToLower() == "image"
                ? new[] { ".jpg", ".jpeg", ".png", ".webp" }
                : new[] { ".mp4", ".avi", ".mov" };

            var extension = Path.GetExtension(file.FileName).ToLower();
            return allowedExtensions.Contains(extension);
        }

        private async Task<ServiceResult<bool>> SaveMediaFileAsync(Guid listingId, IFormFile file, string mediaType, int sortOrder)
        {
            try
            {
                var uploadsFolder = Path.Combine("wwwroot", "uploads", "listings");
                Directory.CreateDirectory(uploadsFolder);

                var fileName = $"{Guid.NewGuid()}_{file.FileName}";
                var filePath = Path.Combine(uploadsFolder, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                var media = new ListingMedia
                {
                    ListingId = listingId,
                    MediaType = mediaType,
                    Url = $"/uploads/listings/{fileName}",
                    SortOrder = sortOrder
                };

                await _listingRepository.AddMediaAsync(listingId, media);

                return ServiceResult<bool>.SuccessResult(true, "Media uploaded successfully");
            }
            catch (Exception ex)
            {
                return ServiceResult<bool>.FailureResult($"Failed to upload media: {ex.Message}");
            }
        }

        private void DeletePhysicalFile(string? url)
        {
            if (string.IsNullOrEmpty(url)) return;

            try
            {
                var filePath = Path.Combine("wwwroot", url.TrimStart('/'));
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }
            }
            catch
            {
                // Log error but don't throw
            }
        }

        // View Tracking
        public async Task<ServiceResult<bool>> TrackViewAsync(Guid listingId, Guid? userId, string? ipAddress, string? userAgent)
        {
            try
            {
                // Check if listing exists
                var listing = await _listingRepository.GetByIdAsync(listingId);
                if (listing == null)
                    return ServiceResult<bool>.FailureResult("Listing not found");

                // Prevent duplicate tracking (same user/IP within 30 minutes)
                var hasRecentView = await _listingViewRepository.HasRecentViewAsync(listingId, userId, ipAddress, 30);
                if (hasRecentView)
                    return ServiceResult<bool>.SuccessResult(true, "View already tracked recently");

                var view = new ListingView
                {
                    ListingId = listingId,
                    UserId = userId,
                    IpAddress = ipAddress,
                    UserAgent = userAgent,
                    ViewedAt = DateTime.UtcNow
                };

                await _listingViewRepository.AddViewAsync(view);

                return ServiceResult<bool>.SuccessResult(true, "View tracked successfully");
            }
            catch (Exception ex)
            {
                return ServiceResult<bool>.FailureResult($"Failed to track view: {ex.Message}");
            }
        }

        public async Task<ServiceResult<ListingViewStats>> GetListingViewStatsAsync(Guid listingId, int days = 30)
        {
            try
            {
                var listing = await _listingRepository.GetByIdAsync(listingId);
                if (listing == null)
                    return ServiceResult<ListingViewStats>.FailureResult("Listing not found");

                var now = DateTime.UtcNow;
                var startDate = now.AddDays(-days).Date;
                var endDate = now.Date;

                // Get total views
                var totalViews = await _listingViewRepository.GetTotalViewsAsync(listingId);

                // Get views for different periods
                var viewsToday = await _listingViewRepository.GetViewCountByDateRangeAsync(
                    listingId, now.Date, now);
                
                var viewsThisWeek = await _listingViewRepository.GetViewCountByDateRangeAsync(
                    listingId, now.AddDays(-7), now);
                
                var viewsThisMonth = await _listingViewRepository.GetViewCountByDateRangeAsync(
                    listingId, now.AddDays(-30), now);

                // Get daily statistics for chart
                var dailyStats = await _listingViewRepository.GetDailyViewStatisticsAsync(
                    listingId, startDate, endDate);

                var stats = new ListingViewStats
                {
                    ListingId = listingId,
                    TotalViews = totalViews,
                    ViewsToday = viewsToday,
                    ViewsThisWeek = viewsThisWeek,
                    ViewsThisMonth = viewsThisMonth,
                    DailyStats = dailyStats.Select(s => new DailyViewStat
                    {
                        Date = s.Date.ToString("yyyy-MM-dd"),
                        Views = s.ViewCount
                    }).ToList()
                };

                return ServiceResult<ListingViewStats>.SuccessResult(stats);
            }
            catch (Exception ex)
            {
                return ServiceResult<ListingViewStats>.FailureResult($"Failed to get view statistics: {ex.Message}");
            }
        }

        // Snapshot and Comparison for Edit Approvals
        public async Task<ServiceResult<ListingComparisonDto>> GetListingComparisonAsync(Guid listingId)
        {
            try
            {
                var listing = await _listingRepository.GetListingWithMediaAsync(listingId);
                if (listing == null)
                    return ServiceResult<ListingComparisonDto>.FailureResult("Listing not found");

                // Get snapshot if exists (for edited listings)
                var snapshot = await _listingSnapshotRepository.GetPendingSnapshotForListingAsync(listingId);
                
                var comparison = new ListingComparisonDto
                {
                    ListingId = listingId,
                    ListerName = listing.Lister?.DisplayName ?? "Unknown User",
                    SubmittedAt = listing.UpdatedAt ?? listing.CreatedAt ?? DateTime.UtcNow,
                    IsUpdate = snapshot != null,
                    Current = MapToListingDataDto(listing)
                };

                if (snapshot != null)
                {
                    comparison.Original = MapSnapshotToListingDataDto(snapshot);
                }

                return ServiceResult<ListingComparisonDto>.SuccessResult(comparison);
            }
            catch (Exception ex)
            {
                return ServiceResult<ListingComparisonDto>.FailureResult($"Failed to get comparison: {ex.Message}");
            }
        }

        public async Task<ServiceResult<bool>> ClearPendingSnapshotAsync(Guid listingId)
        {
            try
            {
                var listing = await _listingRepository.GetByIdAsync(listingId);
                if (listing == null)
                    return ServiceResult<bool>.FailureResult("Listing not found");

                if (listing.PendingSnapshotId.HasValue)
                {
                    await _listingSnapshotRepository.DeleteSnapshotAsync(listing.PendingSnapshotId.Value);
                    listing.PendingSnapshotId = null;
                    await _listingRepository.UpdateAsync(listing);
                }

                return ServiceResult<bool>.SuccessResult(true);
            }
            catch (Exception ex)
            {
                return ServiceResult<bool>.FailureResult($"Failed to clear snapshot: {ex.Message}");
            }
        }

        private ListingDataDto MapToListingDataDto(Listing listing)
        {
            return new ListingDataDto
            {
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
                MediaUrls = listing.ListingMedia?
                    .OrderBy(m => m.SortOrder ?? int.MaxValue)
                    .Select(m => m.Url ?? string.Empty)
                    .ToList() ?? new List<string>()
            };
        }

        private ListingDataDto MapSnapshotToListingDataDto(ListingSnapshot snapshot)
        {
            var mediaUrls = new List<string>();
            if (!string.IsNullOrEmpty(snapshot.MediaUrlsJson))
            {
                try
                {
                    mediaUrls = System.Text.Json.JsonSerializer.Deserialize<List<string>>(snapshot.MediaUrlsJson) ?? new List<string>();
                }
                catch
                {
                    // Ignore deserialization errors
                }
            }

            return new ListingDataDto
            {
                Title = snapshot.Title,
                Description = snapshot.Description,
                TransactionType = snapshot.TransactionType,
                PropertyType = snapshot.PropertyType,
                Price = snapshot.Price,
                StreetName = snapshot.StreetName,
                Ward = snapshot.Ward,
                District = snapshot.District,
                City = snapshot.City,
                Area = snapshot.Area,
                HouseNumber = snapshot.HouseNumber,
                Latitude = snapshot.Latitude,
                Longitude = snapshot.Longitude,
                Bedrooms = snapshot.Bedrooms,
                Bathrooms = snapshot.Bathrooms,
                Floors = snapshot.Floors,
                LegalStatus = snapshot.LegalStatus,
                FurnitureStatus = snapshot.FurnitureStatus,
                Direction = snapshot.Direction,
                MediaUrls = mediaUrls
            };
        }

        private ListingDto MapToDto(Listing listing)
        {
            return new ListingDto
            {
                Id = listing.Id,
                ListerId = listing.ListerId,
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
                Status = listing.Status,
                ExpirationDate = listing.ExpirationDate,
                CreatedAt = listing.CreatedAt,
                UpdatedAt = listing.UpdatedAt,
                UserPackageId = listing.UserPackageId,
                IsFreeListingorder = listing.IsFreeListingorder,
                MaxPhotos = listing.MaxPhotos,
                AllowVideo = listing.AllowVideo,
                IsBoosted = listing.IsBoosted,
                ListerName = listing.Lister?.DisplayName,
                ListerEmail = listing.Lister?.Email,
                ListingMedia = listing.ListingMedia?.Select(m => new ListingMediaDto
                {
                    Id = m.Id,
                    ListingId = m.ListingId,
                    MediaType = m.MediaType ?? "image",
                    Url = m.Url ?? string.Empty,
                    SortOrder = m.SortOrder ?? 0
                }).ToList() ?? new List<ListingMediaDto>(),
                PendingSnapshotId = listing.PendingSnapshotId
            };
        }
    }
}
