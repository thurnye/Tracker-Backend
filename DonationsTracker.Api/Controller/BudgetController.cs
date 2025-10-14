using DonationsTracker.Core.Entity;
using DonationsTracker.Core.Interfaces;
using DonationsTracker.Api.Models;
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

            return Ok(new ApiResponse<Budget>
            {
                Data = result,
                Meta = new ApiMeta
                {
                    RequestId = Guid.NewGuid().ToString(),
                    Timestamp = DateTime.UtcNow
                }
            });
        }

        [HttpGet]
        public async Task<IActionResult> GetUserBudgets()
        {
            var budgets = await _budgetService.GetUserBudgetsAsync();

            return Ok(new ApiResponse<List<Budget>>
            {
                Data = budgets.ToList(),
                Meta = new ApiMeta
                {
                    RequestId = Guid.NewGuid().ToString(),
                    Timestamp = DateTime.UtcNow
                }
            });
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetBudgetById(string id)
        {
            var budget = await _budgetService.GetBudgetAsync(id);

            if (budget == null)
            {
                return NotFound(new ApiResponse<object>
                {
                    Data = null,
                    Errors = new List<ApiError>
                    {
                        new ApiError
                        {
                            Code = ErrorCode.NOT_FOUND,
                            Message = $"Budget with ID {id} was not found."
                        }
                    },
                    Meta = new ApiMeta
                    {
                        RequestId = Guid.NewGuid().ToString(),
                        Timestamp = DateTime.UtcNow
                    }
                });
            }

            return Ok(new ApiResponse<Budget>
            {
                Data = budget,
                Meta = new ApiMeta
                {
                    RequestId = Guid.NewGuid().ToString(),
                    Timestamp = DateTime.UtcNow
                }
            });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteBudget(string id)
        {
            var deleted = await _budgetService.DeleteBudgetAsync(id);

            if (!deleted)
            {
                return NotFound(new ApiResponse<object>
                {
                    Data = null,
                    Errors = new List<ApiError>
                    {
                        new ApiError
                        {
                            Code = ErrorCode.NOT_FOUND,
                            Message = $"Budget with ID {id} was not found."
                        }
                    },
                    Meta = new ApiMeta
                    {
                        RequestId = Guid.NewGuid().ToString(),
                        Timestamp = DateTime.UtcNow
                    }
                });
            }

            return Ok(new ApiResponse<object>
            {
                Data = $"Budget with ID {id} deleted successfully.",
                Meta = new ApiMeta
                {
                    RequestId = Guid.NewGuid().ToString(),
                    Timestamp = DateTime.UtcNow
                }
            });
        }
    }
}
