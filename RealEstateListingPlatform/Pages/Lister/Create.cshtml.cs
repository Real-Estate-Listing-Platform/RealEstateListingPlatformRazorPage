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
    public class CreateModel : PageModel
    {
        private readonly IListingService _listingService;

        public CreateModel(IListingService listingService)
        {
            _listingService = listingService;
        }

        [BindProperty]
        public ListingCreateViewModel Input { get; set; } = new ListingCreateViewModel();

        public SelectList TransactionTypes { get; set; } = null!;
        public SelectList PropertyTypes { get; set; } = null!;
        public SelectList LegalStatuses { get; set; } = null!;
        public SelectList FurnitureStatuses { get; set; } = null!;
        public SelectList Directions { get; set; } = null!;

        // Proxy properties to expose Input properties directly for view access
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

        public IActionResult OnGet()
        {
            PopulateDropDowns();
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(List<IFormFile>? mediaFiles, string action)
        {
            if (!ModelState.IsValid)
            {
                PopulateDropDowns();
                return Page();
            }

            var userId = GetCurrentUserId();
            var dto = MapToCreateDto(Input);
            var result = await _listingService.CreateListingAsync(dto, userId, mediaFiles);

            if (!result.Success)
            {
                ModelState.AddModelError("", result.Message ?? "Failed to create listing");
                PopulateDropDowns();
                return Page();
            }

            // Check if user wants to submit for review
            if (action == "submit")
            {
                var submitResult = await _listingService.SubmitForReviewAsync(result.Data!.Id, userId);
                if (submitResult.Success)
                {
                    TempData["Success"] = "Listing created and submitted for review successfully.";
                    return RedirectToPage("/Lister/Listings");
                }
                else
                {
                    TempData["Warning"] = "Listing created as draft, but submission failed: " + submitResult.Message;
                    return RedirectToPage("/Lister/Edit", new { id = result.Data!.Id });
                }
            }

            // Default: save as draft
            TempData["Success"] = "Listing created as draft successfully.";
            return RedirectToPage("/Lister/Edit", new { id = result.Data!.Id });
        }

        private void PopulateDropDowns()
        {
            TransactionTypes = new SelectList(new[]
            {
                new { Value = "Sell", Text = "Bán" },
                new { Value = "Rent", Text = "Cho thuê" }
            }, "Value", "Text");

            PropertyTypes = new SelectList(new[]
            {
                new { Value = "Apartment", Text = "Căn hộ" },
                new { Value = "House", Text = "Nhà phố" },
                new { Value = "Villa", Text = "Biệt thự" },
                new { Value = "Land", Text = "Đất" },
                new { Value = "Commercial", Text = "Thương mại" }
            }, "Value", "Text");

            LegalStatuses = new SelectList(new[]
            {
                new { Value = "RedBook", Text = "Sổ đỏ" },
                new { Value = "PinkBook", Text = "Sổ hồng" },
                new { Value = "SaleContract", Text = "Hợp đồng mua bán" },
                new { Value = "Waiting", Text = "Đang chờ cấp sổ" }
            }, "Value", "Text");

            FurnitureStatuses = new SelectList(new[]
            {
                new { Value = "FullyFurnished", Text = "Đầy đủ nội thất" },
                new { Value = "PartiallyFurnished", Text = "Nội thất cơ bản" },
                new { Value = "Unfurnished", Text = "Không nội thất" }
            }, "Value", "Text");

            Directions = new SelectList(new[]
            {
                new { Value = "North", Text = "Bắc" },
                new { Value = "South", Text = "Nam" },
                new { Value = "East", Text = "Đông" },
                new { Value = "West", Text = "Tây" },
                new { Value = "Northeast", Text = "Đông Bắc" },
                new { Value = "Northwest", Text = "Tây Bắc" },
                new { Value = "Southeast", Text = "Đông Nam" },
                new { Value = "Southwest", Text = "Tây Nam" }
            }, "Value", "Text");
        }

        private ListingCreateDto MapToCreateDto(ListingCreateViewModel model)
        {
            return new ListingCreateDto
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

        private Guid GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim))
                throw new UnauthorizedAccessException("User ID not found in claims");

            return Guid.Parse(userIdClaim);
        }
    }
}
