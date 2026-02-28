using BLL.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using RealEstateListingPlatform.Models;

namespace RealEstateListingPlatform.Pages.Admin
{
    [Authorize(Roles = "Admin,Lister,Seeker")]
    public class GetComparisonModel : PageModel
    {
        private readonly IListingService _listingService;

        public GetComparisonModel(IListingService listingService)
        {
            _listingService = listingService;
        }

        public ListingComparisonViewModel ComparisonViewModel { get; set; } = new();

        public async Task<IActionResult> OnGetAsync(Guid id)
        {
            var result = await _listingService.GetListingComparisonAsync(id);
            if (!result.Success || result.Data == null)
            {
                return NotFound();
            }

            ComparisonViewModel = MapToComparisonViewModel(result.Data);
            return Partial("_ListingComparison", ComparisonViewModel);
        }

        private ListingComparisonViewModel MapToComparisonViewModel(BLL.DTOs.ListingComparisonDto dto)
        {
            var viewModel = new ListingComparisonViewModel
            {
                ListingId = dto.ListingId,
                ListerName = dto.ListerName,
                SubmittedAt = dto.SubmittedAt,
                IsUpdate = dto.IsUpdate,
                Current = MapToListingDataViewModel(dto.Current)
            };

            if (dto.Original != null)
            {
                viewModel.Original = MapToListingDataViewModel(dto.Original);
            }

            return viewModel;
        }

        private ListingDataViewModel MapToListingDataViewModel(BLL.DTOs.ListingDataDto dto)
        {
            return new ListingDataViewModel
            {
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
                Bedrooms = dto.Bedrooms,
                Bathrooms = dto.Bathrooms,
                Floors = dto.Floors,
                LegalStatus = dto.LegalStatus,
                FurnitureStatus = dto.FurnitureStatus,
                Direction = dto.Direction,
                MediaUrls = dto.MediaUrls
            };
        }
    }
}
