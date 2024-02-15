using BalancerKube.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using BalancerKube.Wallet.Domain.Common;

namespace BalancerKube.Wallets.API.Persistence
{
    public sealed class ApplicationDbContext : DbContext
    {
        public DbSet<User> Users { get; init; }
        public DbSet<Domain.Entities.Wallet> Wallets { get; init; }
        public DbSet<Transaction> Transactions { get; init; }

        public ApplicationDbContext(DbContextOptions options) : base(options)
        {
            Users = Set<User>();
            Wallets = Set<Domain.Entities.Wallet>();
            Transactions = Set<Transaction>();
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // User entity
            modelBuilder.Entity<User>()
                .ToTable("Users");

            modelBuilder.Entity<User>()
                .HasKey(u => u.Id);

            modelBuilder.Entity<User>()
                .HasIndex(u => u.Id);

            modelBuilder.Entity<User>()
                .Property(u => u.Username)
                .HasMaxLength(128)
                .IsRequired();

            modelBuilder.Entity<User>()
                .Property(u => u.City)
                .HasMaxLength(128);

            modelBuilder.Entity<User>()
                .Property(u => u.Address)
                .HasMaxLength(255);

            // Wallet entity

            modelBuilder.Entity<Domain.Entities.Wallet>()
                .ToTable("Wallets");

            modelBuilder.Entity<Domain.Entities.Wallet>()
                .HasKey(w => w.Id);

            modelBuilder.Entity<Domain.Entities.Wallet>()
                .HasIndex(w => w.Id);

            modelBuilder.Entity<Domain.Entities.Wallet>()
                .HasIndex(w => new { w.Id, w.UserId });

            modelBuilder.Entity<Domain.Entities.Wallet>()
                .Ignore(w => w.WalletBalance);

            modelBuilder.Entity<Domain.Entities.Wallet>()
                .Property<decimal>("Balance")
                .HasColumnName("Balance")
                .HasColumnType("decimal(18, 2)")
                .IsRequired();

            modelBuilder.Entity<Domain.Entities.Wallet>()
                .Property<Currency>("Currency")
                .HasColumnName("Currency")
                .HasConversion(
                    currency => currency.Symbol,
                    symbol => new Currency(symbol))
                .IsRequired();

            modelBuilder.Entity<Domain.Entities.Wallet>()
                .HasOne(w => w.User)
                .WithMany(u => u.Wallets)
                .HasForeignKey(w => w.UserId)
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();

            // Transaction entity

            modelBuilder.Entity<Transaction>()
                .ToTable("Transactions");

            modelBuilder.Entity<Transaction>()
                .Property(t => t.Id)
                .ValueGeneratedNever();

            modelBuilder.Entity<Transaction>()
                .HasIndex(t => t.Id);

            modelBuilder.Entity<Transaction>()
                .HasIndex(t => new { t.Id, t.UserId, t.WalletId });

            modelBuilder.Entity<Transaction>()
                .Property(t => t.CorrelationId)
                .IsRequired();

            modelBuilder.Entity<Transaction>()
                .Property(t => t.UserBalance)
                .HasColumnType("decimal(18, 2)")
                .IsRequired();

            modelBuilder.Entity<Transaction>()
                .Ignore(t => t.Price);

            modelBuilder.Entity<Transaction>()
                .Property<decimal>("Amount")
                .HasColumnName("Amount")
                .HasColumnType("decimal(18, 2)")
                .IsRequired();

            modelBuilder.Entity<Transaction>()
                .Property<Currency>("Currency")
                .HasColumnName("Currency")
                .HasConversion(
                    currency => currency.Symbol,
                    symbol => new Currency(symbol))
                .IsRequired();

            modelBuilder.Entity<Transaction>()
                .HasOne(w => w.User)
                .WithMany(u => u.Transactions)
                .HasForeignKey(w => w.UserId)
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();

            modelBuilder.Entity<Transaction>()
                .HasOne(w => w.Wallet)
                .WithMany(u => u.Transactions)
                .HasForeignKey(w => w.WalletId)
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();
        }
    }
}
