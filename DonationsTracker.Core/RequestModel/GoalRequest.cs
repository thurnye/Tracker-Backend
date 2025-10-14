


namespace DonationsTracker.Core.RequestModel
{
    public class GoalRequest
    {
       public string? Id { get; set; }
        public string CategoryId { get; set; } 
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
        public string? Actions { get; set; }
        public string? ResourcesNeeded { get; set; }
        public string? Obstacles { get; set; }
        public string? Milestones { get; set; }

    }
}
