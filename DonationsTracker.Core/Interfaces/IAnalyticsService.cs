using DonationsTracker.Core.DTOs;

namespace DonationsTracker.Core.Interfaces
{
    public interface IAnalyticsService
    {
        Task<DashboardAnalyticsDTO> GetDashboardAnalyticsAsync();
    }
}
