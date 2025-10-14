
using DonationsTracker.Core.DTOs.Shared;
using DonationsTracker.DB;

namespace DonationsTracker.Core.Entity
{
    public class GoalDTO
    {
        public string Id { get; set; }
        public string UserId { get; set; }
        public string? CategoryId { get; set; }
        public string GoalName { get; set; }
        public string? GoalDescription { get; set; }
        public string? Priority { get; set; }
        public int? Progress { get; set; }
        public DateTime? Deadline { get; set; }
        public DateTime? StartDate { get; set; }
        public string? TargetMetric { get; set; }
        public decimal? TargetValue { get; set; }
        public DateTime? TargetDate { get; set; }
        public string? SuccessCriteria { get; set; }
        public List<string>? Actions { get; set; }
        public List<string>? ResourcesNeeded { get; set; }
        public List<string>? Milestones { get; set; } 

        public string? Obstacles { get; set; }

        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public UserLiteDTO? User { get; set; }
        public CategoryLiteDTO? Category { get; set; }

    }
}
