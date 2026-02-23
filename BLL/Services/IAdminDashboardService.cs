using BLL.DTOs;

namespace BLL.Services
{
    public interface IAdminDashboardService
    {
        Task<AdminDashboardStatsDto> GetDashboardStatsAsync();
    }
}
