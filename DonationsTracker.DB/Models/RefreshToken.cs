using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DonationsTracker.DB.Entities
{
    public class RefreshToken
    {
        [Key]
        public int Id { get; set; }

        // The actual opaque token value (unique)
        [Required]
        [MaxLength(256)]
        public string Token { get; set; } = default!;

        // FK to AspNetUsers
        [Required]
        public string UserId { get; set; } = default!;

        public DateTime ExpiresAt { get; set; }
        public bool Revoked { get; set; } = false;

        // Optional: track rotation chain
        [MaxLength(256)]
        public string? ReplacedByToken { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public string? IpAddress { get; set; }

        // Nav property
        [ForeignKey(nameof(UserId))]
        public ApplicationUser User { get; set; } = default!;
    }
}
