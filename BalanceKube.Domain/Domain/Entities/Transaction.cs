using BalancerKube.Wallet.Domain.Abstraction;
using BalancerKube.Wallet.Domain.ValueObjects;

namespace BalancerKube.Domain.Entities
{
    public sealed class Transaction : Entity<Guid>
    {
        public string? Code { get; private set; }
        public decimal UserBalance { get; private set; }
        public int UserId { get; private set; }
        public User User { get; private set; } = null!;
        public int WalletId { get; private set; }
        public Wallet Wallet { get; private set; } = null!;

        public Money Price
        {
            get => new Money(Amount, Currency);
            private set => (Amount, Currency) = (value.Amount, value.Currency);
        }

        private decimal Amount { get; set; } = 0; // Used by EF Core
        private Currency Currency { get; set; } // Used by EF Core

        private Transaction() : base(Guid.NewGuid()) { } // Used by EF Core

        internal static Transaction Create() => default;
    }
}