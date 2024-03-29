using BalancerKube.Wallet.Domain.Common;
using BalancerKube.Wallet.Domain.Abstraction;

namespace BalancerKube.Domain.Entities
{
    public sealed class User : AggregateRoot<int>
    {
        public string Username { get; private set; } = null!;
        public string? City { get; private set; }
        public string? Address { get; private set; }
        public ICollection<Wallet> Wallets { get; private set; } = null!;
        public ICollection<Transaction> Transactions { get; private set; } = null!;

        private User() : base(default) { } // Used by EF Core
        private User(string username, string? city = null, string? address = null) : base(default) =>
            (Username, City, Address, Wallets, Transactions) = (username, city, address, new List<Wallet>(), new List<Transaction>());

        public static User Create(string username, string? city = null, string? address = null) =>
            new User(username, city, address);

        public Transaction AddTransaction(Guid correlationId, Money transactionAmount)
        {
            Transaction transaction;

            if (Wallets.Any(x => x.WalletBalance.Currency == transactionAmount.Currency))
            {
                var existingWallet = Wallets.FirstOrDefault(x =>
                    x.WalletBalance.Currency == transactionAmount.Currency);

                transaction = Transaction.Create(this, existingWallet, transactionAmount, correlationId);

                existingWallet?.AddTransaction(transaction);

                return transaction;
            }

            var wallet = Wallet.Create(Id, new Money(0, transactionAmount.Currency));

            transaction = Transaction.Create(this, wallet, transactionAmount, correlationId);
            wallet.AddTransaction(transaction);

            return transaction;
        }
    }
}