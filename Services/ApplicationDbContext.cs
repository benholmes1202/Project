using Microsoft.EntityFrameworkCore;
using Project.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;


namespace Project.Services
{
    public class ApplicationDbContext : IdentityDbContext<IdentityUser, IdentityRole, string>
    {
        public ApplicationDbContext(DbContextOptions options) : base(options)
        {

        }
        public DbSet<Models.ApplicationUser> AppUsers { get; set; }
        public DbSet<Models.Bet> Bets { get; set; }
        public DbSet<Models.AccountTransaction> AccountTransactions { get; set; }
        public DbSet<Models.BettingAccount> BettingAccounts { get; set; }
        public DbSet<Models.TransactionType> TransactionTypes { get; set; }
        public DbSet<Models.BetSettlement> BetSettlements { get; set; }
        public DbSet<Models.ContactMessage> ContactMessages { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<ApplicationUser>(entity =>
            {
                entity.HasKey(u => u.UserId);

                entity.Property(u => u.IdNumber)
                    .IsRequired()
                    .HasMaxLength(13);

                entity.Property(u => u.FirstName)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(u => u.Surname)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(u => u.Email)
                    .HasMaxLength(150);

                entity.Property(u => u.PhoneNumber)
                    .HasMaxLength(20);

                // Enforces unique ID numbers
                entity.HasIndex(u => u.IdNumber)
                    .IsUnique();

                // Helps searching/filtering users by surname
                entity.HasIndex(u => u.Surname);


                entity.HasIndex(u => u.IdentityUserId)
                                    .IsUnique()
                                    .HasFilter("[IdentityUserId] IS NOT NULL");

                entity.HasOne(u => u.IdentityUser)
                    .WithOne()
                    .HasForeignKey<ApplicationUser>(u => u.IdentityUserId)
                    .OnDelete(DeleteBehavior.Cascade);


                entity.HasMany(u => u.BettingAccounts)
                    .WithOne(a => a.User)
                    .HasForeignKey(a => a.UserId)
                    .OnDelete(DeleteBehavior.Restrict);
            });


            modelBuilder.Entity<BettingAccount>(entity =>
            {
                entity.HasKey(a => a.AccountId);

                entity.Property(a => a.AccountNumber)
                    .IsRequired()
                    .HasMaxLength(30);

                entity.Property(a => a.CurrencyCode)
                    .IsRequired()
                    .HasMaxLength(3)
                    .HasDefaultValue("ZAR");

                entity.Property(a => a.Balance)
                    .HasColumnType("decimal(18,2)")
                    .HasDefaultValue(0m);

                entity.Property(a => a.Status)
                    .IsRequired()
                    .HasMaxLength(20)
                    .HasDefaultValue("Open");

                entity.Property(a => a.CreatedAt)
                    .HasDefaultValueSql("GETDATE()");

                entity.Property(a => a.UpdatedAt)
                    .HasDefaultValueSql("GETDATE()");

                entity.Property(a => a.ClosedAt)
                    .IsRequired(false);

                entity.Property(a => a.RowVersion)
                    .IsRowVersion();

                entity.HasIndex(a => a.AccountNumber)
                    .IsUnique();

                entity.HasIndex(a => a.UserId);

                entity.HasIndex(a => a.Status);

                entity.HasMany(a => a.AccountTransactions)
                    .WithOne(t => t.BettingAccount)
                    .HasForeignKey(t => t.AccountId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasMany(a => a.Bets)
                    .WithOne(b => b.BettingAccount)
                    .HasForeignKey(b => b.AccountId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<TransactionType>(entity =>
            {
                entity.HasKey(t => t.TransactionTypeId);

                entity.Property(t => t.Name)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.HasIndex(t => t.Name)
                    .IsUnique();

                entity.HasMany(t => t.AccountTransactions)
                    .WithOne(at => at.TransactionType)
                    .HasForeignKey(at => at.TransactionTypeId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<AccountTransaction>(entity =>
            {
                entity.HasKey(t => t.TransactionId);

                entity.Property(t => t.Amount)
                    .HasColumnType("decimal(18,2)");

                entity.Property(t => t.TransactionDate)
                    .IsRequired();

                entity.Property(t => t.CaptureDate)
                    .HasDefaultValueSql("GETDATE()");


                entity.HasIndex(t => t.AccountId);
                entity.HasIndex(t => t.TransactionTypeId);
                entity.HasIndex(t => t.TransactionDate);
            });


            modelBuilder.Entity<Bet>(entity =>
            {
                entity.HasKey(b => b.BetId);

                entity.Property(b => b.AccountId)
                    .IsRequired();

                entity.Property(b => b.Category)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(b => b.Amount)
                    .HasColumnType("decimal(16,2)")
                    .IsRequired();

                entity.Property(b => b.CreatedAt)
                    .HasDefaultValueSql("GETDATE()");

                entity.HasIndex(b => b.AccountId);

                entity.HasIndex(b => b.Category);

                entity.HasOne(b => b.BettingAccount)
                    .WithMany(a => a.Bets)
                    .HasForeignKey(b => b.AccountId)
                    .OnDelete(DeleteBehavior.Restrict);
            });


            modelBuilder.Entity<BetSettlement>(entity =>
            {
                entity.HasKey(s => s.SettlementId);

                entity.Property(s => s.PayoutAmount)
                    .HasColumnType("decimal(18,2)");

                entity.Property(s => s.SettledAt)
                    .HasDefaultValueSql("GETDATE()");

                // Ensures one settlement per bet
                entity.HasIndex(s => s.BetId)
                    .IsUnique();
            });
        
            modelBuilder.Entity<ContactMessage>(entity =>
            {
                entity.HasKey(c => c.ContactMessageId);

                entity.Property(c => c.Name)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(c => c.Email)
                    .IsRequired()
                    .HasMaxLength(150);

                entity.Property(c => c.Message)
                    .IsRequired()
                    .HasMaxLength(1000);

                entity.Property(c => c.SubmittedAt)
                    .HasDefaultValueSql("GETDATE()");

                entity.HasIndex(c => c.Email);
                entity.HasIndex(c => c.SubmittedAt);
            });
        }
    }
}
