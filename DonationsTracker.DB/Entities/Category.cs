
using DonationsTracker.DB;

namespace DonationsTracker.Core.Entity
{
    public class Category
    {
        public string? Id { get; set; }
        public string? UserId { get; set; }

        /// <summary>
        /// Either "Income" or "Expense"
        /// </summary>
        public string Type { get; set; } = null!;

        public string Name { get; set; } = null!;
        public string Icon { get; set; } = null!;
        public string Color { get; set; } = "#000000";  // Hex or color string

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public bool IsActive { get; set; }

        // Navigation
        public ApplicationUser User { get; set; } = null!;
        public ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
       public ICollection<Budget> Budgets { get; set; } = new List<Budget>();
        public ICollection<Goal> Goals { get; set; } = new List<Goal>();

    }
}
