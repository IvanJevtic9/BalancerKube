using BalancerKube.Wallet.Domain.Common;
using BalancerKube.Wallet.Domain.Abstraction;

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
        private Transaction(int userId, int walletId, Money amount, string? code = null) : base(Guid.NewGuid()) =>
            (UserId, WalletId, Price, Code) = (userId, walletId, amount, code);

        internal static Transaction Create(int userId, int walletId, Money amount, string? code = null) => 
            new Transaction(userId, walletId, amount, code);
    }
}