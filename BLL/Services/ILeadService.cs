using BLL.DTOs;
using DAL.Models;

namespace BLL.Services
{
    public interface ILeadService

    {
        Task<ServiceResult<Lead>> CreateLeadAsync(Guid listingId, Guid seekerId, string? message, DateTime? appointmentDate = null);
        Task<ServiceResult<Lead>> GetLeadByIdAsync(Guid id, Guid userId);
        Task<ServiceResult<List<Lead>>> GetMyLeadsAsListerAsync(Guid listerId, string? statusFilter = null);
        Task<ServiceResult<List<Lead>>> GetMyLeadsAsSeekerAsync(Guid seekerId);
        Task<ServiceResult<List<Lead>>> GetLeadsByListingIdAsync(Guid listingId, Guid listerId);
        Task<ServiceResult<bool>> UpdateLeadStatusAsync(Guid leadId, Guid listerId, string newStatus, string? listerNote = null);
        Task<ServiceResult<bool>> UpdateAppointmentAsync(Guid leadId, Guid listerId, DateTime appointmentDate);
        Task<ServiceResult<bool>> DeleteLeadAsync(Guid leadId, Guid userId);
        Task<ServiceResult<LeadStatistics>> GetLeadStatisticsAsync(Guid listerId);
        Task<ServiceResult<DashboardStatsDto>> GetDashboardStatsAsync(Guid listerId);
    }

    public class LeadStatistics
    {
        public int TotalLeads { get; set; }
        public int NewLeads { get; set; }
        public int ContactedLeads { get; set; }
        public int ClosedLeads { get; set; }
        public List<Lead>? RecentLeads { get; set; }
    }
}
