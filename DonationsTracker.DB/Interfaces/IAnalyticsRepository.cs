using DonationsTracker.Core.DTOs;

namespace DonationsTracker.Core.Interfaces.Repositories
{
    public interface IAnalyticsRepository
    {
        Task<DashboardAnalyticsDTO> GetDashboardAnalyticsAsync(string userId);
        Task<AnalyticsDTO> GetFullAnalysisAsync(string userId);
    }
}
