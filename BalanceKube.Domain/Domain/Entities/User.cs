using BalancerKube.Wallet.Domain.Abstraction;
using BalancerKube.Wallet.Domain.ValueObjects;

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

        public Transaction AddTransaction(string code, Money transactionAmount) =>

    }
}