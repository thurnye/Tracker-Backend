using DonationsTracker.Core.Entity;
using DonationsTracker.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DonationsTracker.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class BudgetController : ControllerBase
    {
        private readonly IBudgetService _budgetService;

        public BudgetController(IBudgetService budgetService)
        {
            _budgetService = budgetService;
        }

        // ✅ Combined create + update
        [HttpPost("create-update")]
        public async Task<IActionResult> CreateUpdateBudget([FromBody] Budget budget)
        {
            var result = await _budgetService.CreateUpdateBudgetAsync(budget);
            return Ok(result);
        }

        [HttpGet]
        public async Task<IActionResult> GetUserBudgets()
        {
            var budgets = await _budgetService.GetUserBudgetsAsync();
            return Ok(budgets);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetBudgetById(string id)
        {
            var budget = await _budgetService.GetBudgetAsync(id);
            if (budget == null) return NotFound();
            return Ok(budget);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteBudget(string id)
        {
            var deleted = await _budgetService.DeleteBudgetAsync(id);
            return deleted ? NoContent() : NotFound();
        }
    }
}
