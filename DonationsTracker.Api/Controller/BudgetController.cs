using DonationsTracker.Core.Entity;
using DonationsTracker.Core.Interfaces;
using DonationsTracker.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using DonationsTracker.Core.DTOs;

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

        // create + update
        [HttpPost("create-update")]
        public async Task<IActionResult> CreateUpdateBudget([FromBody] BudgetRequest budget)
        {
            var result = await _budgetService.CreateUpdateBudgetAsync(budget);

            return Ok(new ApiResponse<BudgetDTO>
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

            return Ok(new ApiResponse<List<BudgetDTO>>
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

            return Ok(new ApiResponse<BudgetDTO>
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
