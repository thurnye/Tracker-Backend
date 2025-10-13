using DonationsTracker.DB.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client;


namespace DonationsTracker.DB
{
    public class DonationDbContext : IdentityDbContext<ApplicationUser>
    {
        public DonationDbContext(DbContextOptions<DonationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Donation> Donations { get; set; }
          public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Donation>(entity =>
            {
                entity.HasKey(d => d.Id);
                entity.Property(d => d.Description).IsRequired();
                entity.Property(d => d.Amount).HasColumnType("decimal(18,2)");
            });

            modelBuilder.Entity<RefreshToken>(entity =>
            {
                entity.HasKey(rt => rt.Id);
                entity.Property(rt => rt.Token).IsRequired().HasMaxLength(256);
                entity.HasIndex(rt => rt.Token).IsUnique();

                // Configure the relationship to ApplicationUser
                entity.HasOne(rt => rt.User)
                      .WithMany()
                      .HasForeignKey(rt => rt.UserId)
                      .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}
