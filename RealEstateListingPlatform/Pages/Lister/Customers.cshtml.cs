using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace RealEstateListingPlatform.Pages.Lister
{
    [Authorize(Roles = "Lister,Seeker,Admin")]
    public class CustomersModel : PageModel
    {
        // Lead data is loaded via AJAX from LeadsController API
        public void OnGet()
        {
        }
    }
}
