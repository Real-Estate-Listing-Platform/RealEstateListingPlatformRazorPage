using Microsoft.AspNetCore.Mvc;

namespace RealEstateListingPlatform.Models
{
    public class PropertyViewModel 
    {
       
            public int Id { get; set; }
            public string Title { get; set; } = string.Empty;
            public string Description { get; set; } = string.Empty;
            public decimal Price { get; set; }
            public string Currency { get; set; } = "VND";
            public string Location { get; set; } = string.Empty;
            public string ImageUrl { get; set; } = "/images/default-property.jpg";
            public int Bedrooms { get; set; }
            public int Bathrooms { get; set; }
            public double Area { get; set; }
            public string Status { get; set; } = string.Empty;
            public DateTime CreatedDate { get; set; } = DateTime.Now;
            public string FormattedPrice
            {
                get
                {
                    if (Price >= 1000000000)
                        return $"{Price/1000000000:N1} B";
                    if (Price >= 1000000)
                        return $"{Price/1000000:N0} M";
                    return Price.ToString("N0") + " VNĐ";
                }
            }
    }
    
}
