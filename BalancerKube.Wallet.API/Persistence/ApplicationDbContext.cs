using Microsoft.EntityFrameworkCore;
using BalancerKube.Wallet.API.Entities;
using BalancerKube.Common.Models;

namespace BalancerKube.Wallets.API.Persistence
{
    public sealed class ApplicationDbContext : DbContext
    {
        public DbSet<User> Users { get; init; }
        public DbSet<Wallet.API.Entities.Wallet> Wallets { get; init; }
        public DbSet<Transaction> Transactions { get; init; }

        public ApplicationDbContext(DbContextOptions options) : base(options)
        {
            Users = Set<User>();
            Wallets = Set<Wallet.API.Entities.Wallet>();
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
            modelBuilder.Entity<Wallet.API.Entities.Wallet>()
                .ToTable("Wallets");

            modelBuilder.Entity<Wallet.API.Entities.Wallet>()
                .HasKey(w => w.Id);

            modelBuilder.Entity<Wallet.API.Entities.Wallet>()
                .HasIndex(w => w.Id);

            modelBuilder.Entity<Wallet.API.Entities.Wallet>()
                .HasIndex(w => new { w.Id, w.UserId });

            modelBuilder.Entity<Wallet.API.Entities.Wallet>()
                .Ignore(w => w.WalletBalance);

            modelBuilder.Entity<Wallet.API.Entities.Wallet>()
                .Property<decimal>("Balance")
                .HasColumnName("Balance")
                .HasColumnType("decimal(18, 2)")
                .IsRequired();

            modelBuilder.Entity<Wallet.API.Entities.Wallet>()
                .Property<Currency>("Currency")
                .HasColumnName("Currency")
                .HasConversion(
                    currency => currency.Symbol,
                    symbol => new Currency(symbol))
                .IsRequired();

            modelBuilder.Entity<Wallet.API.Entities.Wallet>()
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
                .Property(t => t.ThirdPartyTransactionId)
                .IsRequired();

            modelBuilder.Entity<Transaction>()
                .Property(t => t.UserBalance)
                .HasColumnType("decimal(18, 2)")
                .IsRequired();

            modelBuilder.Entity<Transaction>()
                .Ignore(t => t.Value);

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
