using BalancerKube.Common.Domain;
using BalancerKube.Common.Models;
using BalancerKube.Wallet.API.Exceptions;

namespace BalancerKube.Wallet.API.Entities;

public sealed class User : AggregateRoot<int>
{
    public string Username { get; private set; } = null!;

    public string? City { get; private set; }

    public string? Address { get; private set; }

    public ICollection<Wallet> Wallets { get; private set; } = null!;

    public ICollection<Transaction> Transactions { get; private set; } = null!;

    private User() : base(default) { }

    private User(string username, string? city = null, string? address = null) : base(default) =>
        (Username, City, Address, Wallets, Transactions) = (username, city, address, new List<Wallet>(), new List<Transaction>());

    public static User Create(string username, string? city = null, string? address = null) => new(username, city, address);

    public Result<Transaction> AddTransaction(
        Guid thirdPartyTransactionId, 
        Money transactionAmount,
        string source)
    {
        Transaction transaction;
        Wallet? wallet = Wallets.FirstOrDefault(wallet => wallet.WalletBalance.Currency == transactionAmount.Currency);

        if (wallet is not null)
        {
            if ((wallet.WalletBalance + transactionAmount) < Money.Zero)
            {
                return new Result<Transaction>(new ValidationException("Insufficient funds in the wallet."));
            }

            transaction = Transaction.Create(this, wallet, transactionAmount, thirdPartyTransactionId, source);

            wallet.AddTransaction(transaction);

            return Result<Transaction>.Success(transaction);
        }

        if (transactionAmount < Money.Zero)
        {
            return new Result<Transaction>(new ValidationException("Insufficient funds in the wallet."));
        }

        wallet = Wallet.Create(Id, new Money(0, transactionAmount.Currency));

        transaction = Transaction.Create(this, wallet, transactionAmount, thirdPartyTransactionId, source);

        wallet.AddTransaction(transaction);

        return Result<Transaction>.Success(transaction);
    }
}