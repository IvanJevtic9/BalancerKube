using BalancerKube.Wallet.Domain.ValueObjects;

namespace BalancerKube.Domain.Entities
{
    public class Wallet
    {
        public int Id { get; private set; }
        public int UserId { get; private set; }
        public User User { get; private set; } = null!;
        
        public Money WalletBalance {
            get => new Money(Balance, Currency);
            private set => (Balance, Currency) = (value.Amount, value.Currency);
        }

        private decimal Balance { get; set; } = 0; // Used by EF Core
        private Currency Currency { get; set; } // Used by EF Core

        protected Wallet() { } // Used by EF Core

        protected Wallet(int userId, Money initialBalance) =>
            (UserId, WalletBalance) = (userId, initialBalance);

        public static Wallet CreateWallet(int userId, Money initialBalance) =>  
            new Wallet(userId, initialBalance);
    }
}