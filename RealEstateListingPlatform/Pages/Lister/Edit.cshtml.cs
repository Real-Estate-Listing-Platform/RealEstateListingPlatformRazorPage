using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using BLL.DTOs;
using BLL.Services;
using RealEstateListingPlatform.Models;

namespace RealEstateListingPlatform.Pages.Lister
{
    [Authorize(Roles = "Lister,Seeker,Admin")]
    public class EditModel : PageModel
    {
        private readonly IListingService _listingService;
        private readonly IPackageService _packageService;

        public EditModel(IListingService listingService, IPackageService packageService)
        {
            _listingService = listingService;
            _packageService = packageService;
        }

        [BindProperty]
        public ListingEditViewModel Input { get; set; } = new ListingEditViewModel();

        public SelectList TransactionTypes { get; set; } = null!;
        public SelectList PropertyTypes { get; set; } = null!;
        public SelectList LegalStatuses { get; set; } = null!;
        public SelectList FurnitureStatuses { get; set; } = null!;
        public SelectList Directions { get; set; } = null!;
        public List<UserPackageDto> AvailablePackages { get; set; } = new List<UserPackageDto>();

        // Proxy properties to expose Input properties directly for view access
        public Guid Id { get => Input.Id; set => Input.Id = value; }
        public string Title { get => Input.Title; set => Input.Title = value; }
        public string? Description { get => Input.Description; set => Input.Description = value; }
        public string TransactionType { get => Input.TransactionType; set => Input.TransactionType = value; }
        public string PropertyType { get => Input.PropertyType; set => Input.PropertyType = value; }
        public decimal Price { get => Input.Price; set => Input.Price = value; }
        public string? StreetName { get => Input.StreetName; set => Input.StreetName = value; }
        public string? Ward { get => Input.Ward; set => Input.Ward = value; }
        public string? District { get => Input.District; set => Input.District = value; }
        public string? City { get => Input.City; set => Input.City = value; }
        public string? HouseNumber { get => Input.HouseNumber; set => Input.HouseNumber = value; }
        public string? Area { get => Input.Area; set => Input.Area = value; }
        public decimal? Latitude { get => Input.Latitude; set => Input.Latitude = value; }
        public decimal? Longitude { get => Input.Longitude; set => Input.Longitude = value; }
        public int? Bedrooms { get => Input.Bedrooms; set => Input.Bedrooms = value; }
        public int? Bathrooms { get => Input.Bathrooms; set => Input.Bathrooms = value; }
        public int? Floors { get => Input.Floors; set => Input.Floors = value; }
        public string? LegalStatus { get => Input.LegalStatus; set => Input.LegalStatus = value; }
        public string? FurnitureStatus { get => Input.FurnitureStatus; set => Input.FurnitureStatus = value; }
        public string? Direction { get => Input.Direction; set => Input.Direction = value; }
        public string? Status { get => Input.Status; set => Input.Status = value; }
        public DateTime? CreatedAt { get => Input.CreatedAt; set => Input.CreatedAt = value; }
        public DateTime? UpdatedAt { get => Input.UpdatedAt; set => Input.UpdatedAt = value; }
        public List<ListingMediaDto>? ExistingMedia { get => Input.ExistingMedia; set => Input.ExistingMedia = value; }
        public bool IsBoosted { get => Input.IsBoosted; set => Input.IsBoosted = value; }
        public bool IsFreeListingSlot { get => Input.IsFreeListingSlot; set => Input.IsFreeListingSlot = value; }
        public int MaxPhotos { get => Input.MaxPhotos; set => Input.MaxPhotos = value; }
        public bool AllowVideo { get => Input.AllowVideo; set => Input.AllowVideo = value; }

        public async Task<IActionResult> OnGetAsync(Guid id)
        {
            var userId = GetCurrentUserId();
            
            if (!await _listingService.CanUserModifyListingAsync(id, userId))
            {
                TempData["Error"] = "You are not authorized to edit this listing.";
                return RedirectToPage("/Lister/Listings");
            }

            var result = await _listingService.GetListingWithMediaAsync(id);
            if (!result.Success || result.Data == null)
            {
                TempData["Error"] = "Listing not found.";
                return RedirectToPage("/Lister/Listings");
            }

            // Load available packages for this user
            var activePackagesResult = await _packageService.GetActiveUserPackagesAsync(userId);
            AvailablePackages = activePackagesResult.Success && activePackagesResult.Data != null
                ? activePackagesResult.Data
                    .Where(p => p.Status == "Active" && 
                           (p.Package.PackageType == "PHOTO_PACK" || p.Package.PackageType == "VIDEO_UPLOAD"))
                    .ToList()
                : new List<UserPackageDto>();

            PopulateDropDowns();
            Input = MapToEditViewModel(result.Data);
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(Guid id, List<IFormFile>? mediaFiles, Guid? photoPackageId, Guid? videoPackageId)
        {
            if (!ModelState.IsValid)
            {
                await PopulateEditMetadataAsync(id);
                PopulateDropDowns();
                return Page();
            }

            var userId = GetCurrentUserId();
            var dto = MapToUpdateDto(Input);
            var result = await _listingService.UpdateListingAsync(id, dto, userId, mediaFiles);

            if (!result.Success)
            {
                ModelState.AddModelError("", result.Message ?? "Failed to update listing");
                await PopulateEditMetadataAsync(id);
                PopulateDropDowns();
                return Page();
            }

            // Apply packages if selected
            var packageMessages = new List<string>();
            
            if (photoPackageId.HasValue && photoPackageId.Value != Guid.Empty)
            {
                var photoPackageDto = new ApplyPackageDto
                {
                    ListingId = id,
                    UserPackageId = photoPackageId.Value
                };
                
                var photoResult = await _packageService.ApplyPackageToListingAsync(userId, photoPackageDto);
                if (photoResult.Success)
                {
                    packageMessages.Add("Photo package applied successfully!");
                }
                else
                {
                    packageMessages.Add($"Photo package failed: {photoResult.Message}");
                }
            }
            
            if (videoPackageId.HasValue && videoPackageId.Value != Guid.Empty)
            {
                var videoPackageDto = new ApplyPackageDto
                {
                    ListingId = id,
                    UserPackageId = videoPackageId.Value
                };
                
                var videoResult = await _packageService.ApplyPackageToListingAsync(userId, videoPackageDto);
                if (videoResult.Success)
                {
                    packageMessages.Add("Video package applied successfully!");
                }
                else
                {
                    packageMessages.Add($"Video package failed: {videoResult.Message}");
                }
            }

            // Build success message
            var successMessage = "Listing updated successfully.";
            if (packageMessages.Any())
            {
                successMessage += " " + string.Join(" ", packageMessages);
            }

            TempData["Success"] = successMessage;
            return RedirectToPage("/Lister/Edit", new { id });
        }

        public async Task<IActionResult> OnPostSubmitForReviewAsync(Guid id)
        {
            var userId = GetCurrentUserId();
            var result = await _listingService.SubmitForReviewAsync(id, userId);

            if (!result.Success)
            {
                TempData["Error"] = result.Message;
                return RedirectToPage("/Lister/Edit", new { id });
            }

            TempData["Success"] = "Listing submitted for review.";
            return RedirectToPage("/Lister/Listings");
        }

        public async Task<IActionResult> OnPostUploadMediaAsync(Guid listingId, IFormFile file, string mediaType)
        {
            var userId = GetCurrentUserId();
            
            if (!await _listingService.CanUserModifyListingAsync(listingId, userId))
                return Forbid();

            var result = await _listingService.AddMediaToListingAsync(listingId, file, mediaType);

            if (!result.Success)
                return BadRequest(new { message = result.Message });

            return new JsonResult(new { message = "Media uploaded successfully" });
        }

        public async Task<IActionResult> OnPostDeleteMediaAsync(Guid mediaId)
        {
            var userId = GetCurrentUserId();
            var result = await _listingService.DeleteMediaAsync(mediaId, userId);

            if (!result.Success)
                return BadRequest(new { message = result.Message });

            return new JsonResult(new { message = "Media deleted successfully" });
        }

        public async Task<IActionResult> OnPostDeleteAsync(Guid id)
        {
            var userId = GetCurrentUserId();
            var result = await _listingService.DeleteListingAsync(id, userId);

            if (!result.Success)
            {
                TempData["Error"] = result.Message;
            }
            else
            {
                TempData["Success"] = "Listing permanently deleted.";
            }

            return RedirectToPage("/Lister/Listings");
        }

        private void PopulateDropDowns()
        {
            TransactionTypes = new SelectList(new[]
            {
                new { Value = "Sell", Text = "For Sale" },
                new { Value = "Rent", Text = "For Rent" }
            }, "Value", "Text");

            PropertyTypes = new SelectList(new[]
            {
                new { Value = "Apartment", Text = "Apartment" },
                new { Value = "House", Text = "House" },
                new { Value = "Villa", Text = "Villa" },
                new { Value = "Land", Text = "Land" },
                new { Value = "Commercial", Text = "Commercial" }
            }, "Value", "Text");

            LegalStatuses = new SelectList(new[]
            {
                new { Value = "RedBook", Text = "Red Book" },
                new { Value = "PinkBook", Text = "Pink Book" },
                new { Value = "SaleContract", Text = "Sale Contract" },
                new { Value = "Waiting", Text = "Waiting for Certificate" }
            }, "Value", "Text");

            FurnitureStatuses = new SelectList(new[]
            {
                new { Value = "FullyFurnished", Text = "Fully Furnished" },
                new { Value = "PartiallyFurnished", Text = "Partially Furnished" },
                new { Value = "Unfurnished", Text = "Unfurnished" }
            }, "Value", "Text");

            Directions = new SelectList(new[]
            {
                new { Value = "North", Text = "North" },
                new { Value = "South", Text = "South" },
                new { Value = "East", Text = "East" },
                new { Value = "West", Text = "West" },
                new { Value = "Northeast", Text = "Northeast" },
                new { Value = "Northwest", Text = "Northwest" },
                new { Value = "Southeast", Text = "Southeast" },
                new { Value = "Southwest", Text = "Southwest" }
            }, "Value", "Text");
        }

        private ListingEditViewModel MapToEditViewModel(ListingDto listing)
        {
            return new ListingEditViewModel
            {
                Id = listing.Id,
                Title = listing.Title,
                Description = listing.Description,
                TransactionType = listing.TransactionType ?? string.Empty,
                PropertyType = listing.PropertyType ?? string.Empty,
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
                CreatedAt = listing.CreatedAt,
                UpdatedAt = listing.UpdatedAt,
                ExistingMedia = listing.ListingMedia ?? new List<ListingMediaDto>(),
                IsBoosted = listing.IsBoosted,
                IsFreeListingSlot = listing.IsFreeListingSlot,
                MaxPhotos = listing.MaxPhotos,
                AllowVideo = listing.AllowVideo
            };
        }

        private ListingUpdateDto MapToUpdateDto(ListingEditViewModel model)
        {
            return new ListingUpdateDto
            {
                Title = model.Title,
                Description = model.Description,
                TransactionType = model.TransactionType,
                PropertyType = model.PropertyType,
                Price = model.Price,
                StreetName = model.StreetName,
                Ward = model.Ward,
                District = model.District,
                City = model.City,
                Area = model.Area,
                HouseNumber = model.HouseNumber,
                Latitude = model.Latitude,
                Longitude = model.Longitude,
                Bedrooms = model.Bedrooms,
                Bathrooms = model.Bathrooms,
                Floors = model.Floors,
                LegalStatus = model.LegalStatus,
                FurnitureStatus = model.FurnitureStatus,
                Direction = model.Direction
            };
        }

        private async Task PopulateEditMetadataAsync(Guid id)
        {
            var listingResult = await _listingService.GetListingWithMediaAsync(id);
            if (!listingResult.Success || listingResult.Data == null)
                return;

            Input.Status = listingResult.Data.Status;
            Input.CreatedAt = listingResult.Data.CreatedAt;
            Input.UpdatedAt = listingResult.Data.UpdatedAt;
            Input.ExistingMedia = listingResult.Data.ListingMedia ?? new List<ListingMediaDto>();
            
            // Load available packages
            var userId = GetCurrentUserId();
            var activePackagesResult = await _packageService.GetActiveUserPackagesAsync(userId);
            AvailablePackages = activePackagesResult.Success && activePackagesResult.Data != null
                ? activePackagesResult.Data
                    .Where(p => p.Status == "Active" && 
                           (p.Package.PackageType == "PHOTO_PACK" || p.Package.PackageType == "VIDEO_UPLOAD"))
                    .ToList()
                : new List<UserPackageDto>();
        }

        private Guid GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim))
                throw new UnauthorizedAccessException("User ID not found in claims");

            return Guid.Parse(userIdClaim);
        }
    }
}
