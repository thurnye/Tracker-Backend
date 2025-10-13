using DonationsTracker.Core.Entity;
using DonationsTracker.Core.Interfaces;
using DonationsTracker.Core.Interfaces.Repositories;
using DonationsTracker.Core.Helpers;

namespace DonationsTracker.Core.Services
{
    public class BudgetService : IBudgetService
    {
        private readonly IBudgetRepository _budgetRepository;
        private readonly IUserContextService _userContext;

        public BudgetService(IBudgetRepository budgetRepository, IUserContextService userContext)
        {
            _budgetRepository = budgetRepository;
            _userContext = userContext;
        }

        public Task<IEnumerable<Budget>> GetUserBudgetsAsync()
        {
            var userId = _userContext.GetUserId();
            return _budgetRepository.GetBudgetsByUserAsync(userId);
        }

        public Task<Budget?> GetBudgetAsync(string id)
        {
            return _budgetRepository.GetBudgetByIdAsync(id);
        }

        public async Task<Budget> CreateUpdateBudgetAsync(Budget budget)
        {
            var userId = _userContext.GetUserId();
            budget.UserId = userId;

            if (!string.IsNullOrEmpty(budget.Id))
            {
                var existing = await _budgetRepository.GetBudgetByIdAsync(budget.Id);
                if (existing == null)
                    throw new KeyNotFoundException("Budget not found.");

                if (existing.UserId != userId)
                    throw new UnauthorizedAccessException("You cannot modify another user’s budget.");

                existing.BudgetAmount = budget.BudgetAmount;
                existing.CategoryId = budget.CategoryId;
                existing.Currency = budget.Currency;
                existing.Status = budget.Status;
                existing.SpendingType = budget.SpendingType;
                existing.StartDate = budget.StartDate;
                existing.EndDate = budget.EndDate;
                existing.Frequency = budget.Frequency;
                existing.Notes = budget.Notes;
                existing.UpdatedAt = DateTime.UtcNow;
                existing.IsActive = true;

                return await _budgetRepository.UpdateBudgetAsync(existing);
            }

            budget.CreatedAt = DateTime.UtcNow;
            budget.IsActive = true;
            return await _budgetRepository.CreateBudgetAsync(budget);
        }

        public async Task<bool> DeleteBudgetAsync(string id)
        {
            return await _budgetRepository.DeleteBudgetAsync(id);
        }
    }
}
