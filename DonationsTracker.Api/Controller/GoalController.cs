using DonationsTracker.Core.Entity;
using DonationsTracker.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DonationsTracker.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class GoalController : ControllerBase
    {
        private readonly IGoalService _goalService;

        public GoalController(IGoalService goalService)
        {
            _goalService = goalService;
        }

        
        [HttpPost("create-update")]
        public async Task<IActionResult> CreateUpdateGoal([FromBody] Goal goal)
        {
            var result = await _goalService.CreateUpdateGoalAsync(goal);
            return Ok(result);
        }

        [HttpGet]
        public async Task<IActionResult> GetUserGoals()
        {
            var goals = await _goalService.GetUserGoalsAsync();
            return Ok(goals);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetGoalById(string id)
        {
            var goal = await _goalService.GetGoalAsync(id);
            if (goal == null) return NotFound();
            return Ok(goal);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteGoal(string id)
        {
            var deleted = await _goalService.DeleteGoalAsync(id);
            return deleted ? NoContent() : NotFound();
        }
    }
}
