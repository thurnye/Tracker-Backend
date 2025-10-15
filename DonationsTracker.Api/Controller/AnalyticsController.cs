using DonationsTracker.Api.Models;
using DonationsTracker.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DonationsTracker.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class AnalyticsController : ControllerBase
    {
        private readonly IAnalyticsService _analyticsService;

        public AnalyticsController(IAnalyticsService analyticsService)
        {
            _analyticsService = analyticsService;
        }

        [HttpGet("dashboard")]
        public async Task<IActionResult> GetDashboard()
        {
            try
            {
                var analytics = await _analyticsService.GetDashboardAnalyticsAsync();

                return Ok(new ApiResponse<object>
                {
                    Data = analytics,
                    Meta = new ApiMeta
                    {
                        RequestId = Guid.NewGuid().ToString(),
                        Timestamp = DateTime.UtcNow
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<object>
                {
                    Errors = new List<ApiError>
                    {
                        new ApiError
                        {
                            Code = ErrorCode.INTERNAL_ERROR,
                            Message = ex.Message
                        }
                    },
                    Meta = new ApiMeta { Timestamp = DateTime.UtcNow }
                });
            }
        }
    }
}
