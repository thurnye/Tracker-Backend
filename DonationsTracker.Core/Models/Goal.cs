using System;

namespace FinancialTracker.Core.Entities
{
    public class Goal
    {
        public int GoalId { get; set; }
        public int UserId { get; set; }
        public string GoalName { get; set; } = null!;
        public string? GoalDescription { get; set; }
        public string? Category { get; set; }
        public string? Priority { get; set; }
        public int? Progress { get; set; }
        public DateTime? Deadline { get; set; }
        public DateTime? StartDate { get; set; }
        public string? TargetMetric { get; set; }
        public decimal? TargetValue { get; set; }
        public DateTime? TargetDate { get; set; }
        public string? SuccessCriteria { get; set; }
        public string? Actions { get; set; }
        public string? ResourcesNeeded { get; set; }
        public string? Obstacles { get; set; }
        public string? Milestones { get; set; }

        // Navigation
        public User User { get; set; } = null!;
    }
}
