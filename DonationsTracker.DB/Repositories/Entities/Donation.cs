using System.ComponentModel.DataAnnotations;

namespace DonationsTracker.DB;

public class Donation
{
    [Key]
    public int Id { get; set; }
    public string Description { get; set; }
    public double Amount { get; set; }

        public Boolean? IsActive { get; set; }
    
}
