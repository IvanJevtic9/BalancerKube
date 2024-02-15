using BalancerKube.Wallet.Domain.Common;
using BalancerKube.Wallet.Domain.Abstraction;

namespace BalancerKube.Domain.Entities
{
    public sealed class Transaction : Entity<Guid>
    {
        public Guid CorrelationId { get; private set; }
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
        private Transaction(int userId, int walletId, Money amount, Guid correlationId) : base(Guid.NewGuid()) =>
            (UserId, WalletId, Price, CorrelationId) = (userId, walletId, amount, correlationId);

        internal static Transaction Create(int userId, int walletId, Money amount, Guid correlationId) => 
            new Transaction(userId, walletId, amount, correlationId);
    }
}