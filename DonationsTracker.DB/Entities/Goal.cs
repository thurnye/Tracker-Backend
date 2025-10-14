
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using DonationsTracker.DB;

namespace DonationsTracker.Core.Entity
{
    public class Goal
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

        [MaxLength(1000)]
        [Column(TypeName = "nvarchar(1000)")]
        public string? SuccessCriteria { get; set; }


        [MaxLength(1000)]
        [Column(TypeName = "nvarchar(MAX)")]
        public string? Obstacles { get; set; }
        public List<string>? Actions { get; set; }
       public List<string>? ResourcesNeeded { get; set; } = new();
        public List<string>? Milestones { get; set; } = new();

        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation
        public ApplicationUser User { get; set; } = null!;
        public Category? Category { get; set; }

    }
}
