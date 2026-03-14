using BLL.DTOs;

namespace BLL.Services
{
    public interface IValuationReportService
    {
        Task<ValuationReportDto> SaveAsync(Guid userId, SaveReportDto dto);
        Task<List<ValuationReportDto>> GetMyReportsAsync(Guid userId);
        Task<ValuationReportDto?> GetByIdAsync(Guid id);
        Task<List<ValuationReportDto>> GetForComparisonAsync(IEnumerable<Guid> ids, Guid userId);
        Task<bool> DeleteAsync(Guid id, Guid userId);
        Task UpdateNameAsync(Guid id, Guid userId, string newName);
    }
}
