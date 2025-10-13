using DonationsTracker.Core.Entity;

namespace DonationsTracker.Core.Interfaces
{
    public interface IBudgetService
    {
        Task<IEnumerable<Budget>> GetUserBudgetsAsync();
        Task<Budget?> GetBudgetAsync(string id);
        Task<Budget> CreateUpdateBudgetAsync(Budget budget);
        Task<bool> DeleteBudgetAsync(string id);
    }
}
