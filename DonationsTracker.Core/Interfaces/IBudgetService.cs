using DonationsTracker.Core.DTOs;
using DonationsTracker.Core.Entity;

namespace DonationsTracker.Core.Interfaces
{
    public interface IBudgetService
    {
        Task<IEnumerable<BudgetDTO>> GetUserBudgetsAsync();
        Task<BudgetDTO?> GetBudgetAsync(string id);
        Task<BudgetDTO> CreateUpdateBudgetAsync(BudgetRequest budget);
        Task<bool> DeleteBudgetAsync(string id);
    }
}
