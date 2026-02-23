using BLL.DTOs;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.Services
{
    public interface IListingService
    {
        public Task<List<ListingDto>> GetListings();
        Task<IEnumerable<ListingDto>> GetPendingListingsAsync();
        Task<IEnumerable<ListingDto>> GetPublishedListingsAsync();
        Task<IEnumerable<ListingDto>> GetPublishedByTypeAsync(string type);
        Task<IEnumerable<ListingDto>> GetByTypeAsync(string type);
        Task<ListingDto> GetByIdAsync(Guid id);
        Task<bool> ApproveListingAsync(Guid id);
        Task<bool> RejectListingAsync(Guid id);

        // Create
        Task<ServiceResult<ListingDto>> CreateListingAsync(ListingCreateDto dto, Guid listerId, List<IFormFile>? mediaFiles = null);

        // Read (Enhanced)
        Task<ServiceResult<ListingDto>> GetListingByIdAsync(Guid id);
        Task<ServiceResult<List<ListingDto>>> GetMyListingsAsync(Guid listerId);
        Task<ServiceResult<PaginatedResult<ListingDto>>> GetMyListingsFilteredAsync(Guid listerId, ListingFilterParameters parameters);
        Task<ServiceResult<ListingDto>> GetListingWithMediaAsync(Guid id);

        // Update
        Task<ServiceResult<ListingDto>> UpdateListingAsync(Guid id, ListingUpdateDto dto, Guid userId, List<IFormFile>? mediaFiles = null);
        Task<ServiceResult<bool>> SubmitForReviewAsync(Guid id, Guid userId);

        // Delete
        Task<ServiceResult<bool>> DeleteListingAsync(Guid id, Guid userId, bool isAdmin = false);

        // Media Management
        Task<ServiceResult<bool>> AddMediaToListingAsync(Guid listingId, IFormFile file, string mediaType);
        Task<ServiceResult<bool>> DeleteMediaAsync(Guid mediaId, Guid userId);
        Task<ServiceResult<List<ListingMediaDto>>> GetListingMediaAsync(Guid listingId);

        // Validation
        Task<ServiceResult<bool>> ValidateListingDataAsync(ListingCreateDto dto);
        Task<bool> CanUserModifyListingAsync(Guid listingId, Guid userId);

        // View Tracking
        Task<ServiceResult<bool>> TrackViewAsync(Guid listingId, Guid? userId, string? ipAddress, string? userAgent);
        Task<ServiceResult<ListingViewStats>> GetListingViewStatsAsync(Guid listingId, int days = 30);
        
        // Snapshot and Comparison for Edit Approvals
        Task<ServiceResult<ListingComparisonDto>> GetListingComparisonAsync(Guid listingId);
        Task<ServiceResult<bool>> ClearPendingSnapshotAsync(Guid listingId);
    }
}

