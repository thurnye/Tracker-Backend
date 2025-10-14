
using DonationsTracker.DB;

namespace DonationsTracker.Core.RequestModel
{
    public class CategoryRequest
    {
        public string? Id { get; set; }
        public string Type { get; set; } = null!;

        public string Name { get; set; } = null!;
        public string Icon { get; set; } = null!;
        public string Color { get; set; } = "#000000";  // Hex or color string


    }
}
