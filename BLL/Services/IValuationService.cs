using BLL.DTOs;

namespace BLL.Services
{
    public interface IValuationService
    {
        /// <summary>
        /// Estimates property value based on average price/m² of comparable
        /// Published listings in the same district (falls back to city if
        /// fewer than 3 district-level samples are available).
        /// </summary>
        Task<ValuationResultDto> EstimateAsync(
            string propertyType,
            decimal areaSqm,
            string city,
            string district,
            string transactionType);
    }
}
