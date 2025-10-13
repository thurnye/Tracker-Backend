using DonationsTracker.DB.Entities;
using DonationsTracker.Core.Entity;
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
        public DbSet<Budget> Budgets { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Goal> Goals { get; set; }
        public DbSet<Transaction> Transactions { get; set; }
        public DbSet<Wallet> Wallets { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Donation configuration
            modelBuilder.Entity<Donation>(entity =>
            {
                entity.HasKey(d => d.Id);
                entity.Property(d => d.Description).IsRequired();
                entity.Property(d => d.Amount).HasColumnType("decimal(18,2)");
            });

            // RefreshToken configuration
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

            // Category configuration
            modelBuilder.Entity<Category>(entity =>
            {
                entity.HasKey(c => c.Id);
                entity.Property(c => c.Type).IsRequired().HasMaxLength(50);
                entity.Property(c => c.Name).IsRequired().HasMaxLength(100);
                entity.Property(c => c.Icon).IsRequired().HasMaxLength(100);
                entity.Property(c => c.Color).HasMaxLength(20);

                entity.HasOne(c => c.User)
                      .WithMany()
                      .HasForeignKey(c => c.UserId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // Budget configuration
            modelBuilder.Entity<Budget>(entity =>
            {
                entity.HasKey(b => b.Id);
                entity.Property(b => b.SpendingType).IsRequired().HasMaxLength(50);
                entity.Property(b => b.BudgetAmount).HasColumnType("decimal(18,2)");
                entity.Property(b => b.AmountSpent).HasColumnType("decimal(18,2)");
                entity.Property(b => b.PaymentMethod).HasMaxLength(100);
                entity.Property(b => b.Frequency).HasMaxLength(50);
                entity.Property(b => b.BudgetType).HasMaxLength(50);
                entity.Property(b => b.IncomeSource).HasMaxLength(100);
                entity.Property(b => b.Currency).HasMaxLength(10);
                entity.Property(b => b.Status).HasMaxLength(50);

                entity.HasOne(b => b.User)
                      .WithMany()
                      .HasForeignKey(b => b.UserId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(b => b.Category)
                      .WithMany(c => c.Budgets)
                      .HasForeignKey(b => b.CategoryId)
                      .OnDelete(DeleteBehavior.NoAction);
            });

            // Goal configuration
            modelBuilder.Entity<Goal>(entity =>
            {
                entity.HasKey(g => g.Id);
                entity.Property(g => g.GoalName).IsRequired().HasMaxLength(200);
                entity.Property(g => g.Priority).HasMaxLength(50);
                entity.Property(g => g.TargetMetric).HasMaxLength(100);
                entity.Property(g => g.TargetValue).HasColumnType("decimal(18,2)");
                entity.Property(g => g.SuccessCriteria).HasMaxLength(500);

                entity.HasOne(g => g.User)
                      .WithMany()
                      .HasForeignKey(g => g.UserId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(g => g.Category)
                      .WithMany(c => c.Goals)
                      .HasForeignKey(g => g.CategoryId)
                      .OnDelete(DeleteBehavior.NoAction);
            });

            // Wallet configuration
            modelBuilder.Entity<Wallet>(entity =>
            {
                entity.HasKey(w => w.Id);
                entity.Property(w => w.WalletName).IsRequired().HasMaxLength(100);
                entity.Property(w => w.WalletType).IsRequired().HasMaxLength(50);
                entity.Property(w => w.AccountNumber).IsRequired().HasMaxLength(50);
                entity.Property(w => w.BankName).IsRequired().HasMaxLength(100);
                entity.Property(w => w.Currency).HasMaxLength(10);
                entity.Property(w => w.Balance).HasColumnType("decimal(18,2)");
                entity.Property(w => w.CreditLimit).HasColumnType("decimal(18,2)");
                entity.Property(w => w.InterestRate).HasColumnType("decimal(5,2)");
                entity.Property(w => w.CardType).HasMaxLength(50);

                entity.HasOne(w => w.User)
                      .WithMany()
                      .HasForeignKey(w => w.UserId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(w => w.Category)
                      .WithMany()
                      .HasForeignKey(w => w.CategoryId)
                      .OnDelete(DeleteBehavior.NoAction);
            });

            // Transaction configuration
            modelBuilder.Entity<Transaction>(entity =>
            {
                entity.HasKey(t => t.Id);
                entity.Property(t => t.TransactionAmount).HasColumnType("decimal(18,2)");
                entity.Property(t => t.TransactionType).IsRequired().HasMaxLength(50);
                entity.Property(t => t.MerchantName).HasMaxLength(200);
                entity.Property(t => t.Status).HasMaxLength(50);
                entity.Property(t => t.Method).HasMaxLength(50);
                entity.Property(t => t.Location).HasMaxLength(200);
                entity.Property(t => t.Reference).HasMaxLength(100);
                entity.Property(t => t.CurrencyCode).HasMaxLength(10);
                entity.Property(t => t.TransactionFee).HasColumnType("decimal(18,2)");

                entity.HasOne(t => t.User)
                      .WithMany()
                      .HasForeignKey(t => t.UserId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(t => t.Wallet)
                      .WithMany(w => w.Transactions)
                      .HasForeignKey(t => t.WalletId)
                      .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(t => t.Category)
                      .WithMany(c => c.Transactions)
                      .HasForeignKey(t => t.CategoryId)
                      .OnDelete(DeleteBehavior.NoAction);
            });
        }
    }
}
