using BalancerKube.Wallet.Domain.Common;
using BalancerKube.Wallet.Domain.Abstraction;

namespace BalancerKube.Domain.Entities
{
    public sealed class Wallet : Entity<int>
    {
        public int UserId { get; private set; }
        public User User { get; private set; } = null!;
        public ICollection<Transaction> Transactions { get; private set; } = null!;
        
        public Money WalletBalance {
            get => new Money(Balance, Currency);
            private set => (Balance, Currency) = (value.Amount, value.Currency);
        }

        private decimal Balance { get; set; } = 0; // Used by EF Core
        private Currency Currency { get; set; } // Used by EF Core

        private Wallet() : base(default) { } // Used by EF Core

        private Wallet(int userId, Money initialBalance) : base(default) =>
            (UserId, WalletBalance, Transactions) = (userId, initialBalance, new List<Transaction>());

        internal static Wallet Create(int userId, Money initialBalance) =>  
            new Wallet(userId, initialBalance);

        public void AddTransaction(Transaction transaction) 
        {
            if (transaction.Price.Currency != Currency)
                throw new InvalidOperationException("Cannot add transaction with different currency to the wallet.");

            Transactions.Add(transaction);
            Balance += transaction.Price.Amount;
        }
            
    }
}