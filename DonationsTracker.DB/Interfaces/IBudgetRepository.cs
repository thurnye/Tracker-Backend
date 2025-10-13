using DonationsTracker.Core.Entity;

namespace DonationsTracker.Core.Interfaces.Repositories
{
    public interface IBudgetRepository
    {
        Task<IEnumerable<Budget>> GetBudgetsByUserAsync(string userId);
        Task<Budget?> GetBudgetByIdAsync(string id);
        Task<Budget> CreateBudgetAsync(Budget budget);
        Task<Budget?> UpdateBudgetAsync(Budget budget);
        Task<bool> DeleteBudgetAsync(string id);
    }
}
