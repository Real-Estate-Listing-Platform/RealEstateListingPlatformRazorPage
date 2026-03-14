using BLL.DTOs;
using DAL.Models;
using DAL.Repositories;

namespace BLL.Services.Implementation
{
    public class ValuationReportService : IValuationReportService
    {
        private readonly IValuationReportRepository _repo;

        public ValuationReportService(IValuationReportRepository repo)
        {
            _repo = repo;
        }

        public async Task<ValuationReportDto> SaveAsync(Guid userId, SaveReportDto dto)
        {
            var entity = new ValuationReport
            {
                UserId           = userId,
                ReportName       = dto.ReportName,
                PropertyType     = dto.PropertyType,
                TransactionType  = dto.TransactionType,
                AreaSqm          = dto.AreaSqm,
                City             = dto.City,
                District         = dto.District,
                Ward             = dto.Ward,
                AddressLine      = dto.AddressLine,
                Notes            = dto.Notes,
                EstimatedMinPrice = dto.EstimatedMinPrice,
                EstimatedAvgPrice = dto.EstimatedAvgPrice,
                EstimatedMaxPrice = dto.EstimatedMaxPrice,
                AvgPricePerSqm   = dto.AvgPricePerSqm,
                SampleCount      = dto.SampleCount,
                IsFallbackToCity = dto.IsFallbackToCity,
                MarketInsight    = dto.MarketInsight
            };

            var saved = await _repo.CreateAsync(entity);
            return ToDto(saved);
        }

        public async Task<List<ValuationReportDto>> GetMyReportsAsync(Guid userId)
        {
            var list = await _repo.GetByUserIdAsync(userId);
            return list.Select(ToDto).ToList();
        }

        public async Task<ValuationReportDto?> GetByIdAsync(Guid id)
        {
            var entity = await _repo.GetByIdAsync(id);
            return entity == null ? null : ToDto(entity);
        }

        public async Task<List<ValuationReportDto>> GetForComparisonAsync(
            IEnumerable<Guid> ids, Guid userId)
        {
            var reports = await _repo.GetByIdsAsync(ids);
            // Only return reports that belong to the requesting user
            return reports
                .Where(r => r.UserId == userId)
                .Select(ToDto)
                .ToList();
        }

        public Task<bool> DeleteAsync(Guid id, Guid userId) =>
            _repo.DeleteAsync(id, userId);

        public Task UpdateNameAsync(Guid id, Guid userId, string newName) =>
            _repo.UpdateNameAsync(id, userId, newName);

        // ── mapper ────────────────────────────────────────────────────────────
        private static ValuationReportDto ToDto(ValuationReport e) => new()
        {
            Id               = e.Id,
            UserId           = e.UserId,
            ReportName       = e.ReportName,
            PropertyType     = e.PropertyType,
            TransactionType  = e.TransactionType,
            AreaSqm          = e.AreaSqm,
            City             = e.City,
            District         = e.District,
            Ward             = e.Ward,
            AddressLine      = e.AddressLine,
            Notes            = e.Notes,
            EstimatedMinPrice = e.EstimatedMinPrice,
            EstimatedAvgPrice = e.EstimatedAvgPrice,
            EstimatedMaxPrice = e.EstimatedMaxPrice,
            AvgPricePerSqm   = e.AvgPricePerSqm,
            SampleCount      = e.SampleCount,
            IsFallbackToCity = e.IsFallbackToCity,
            MarketInsight    = e.MarketInsight,
            CreatedAt        = e.CreatedAt
        };
    }
}
