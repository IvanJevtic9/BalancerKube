using BalancerKube.Common.Domain;
using BalancerKube.Common.Models;

namespace BalancerKube.Wallet.API.Entities;

public sealed class Transaction : Entity<Guid>
{
    public Guid ThirdPartyTransactionId { get; private set; }

    public int UserId { get; private set; }

    public User User { get; private set; } = null!;

    public Guid WalletId { get; private set; }

    public Wallet Wallet { get; private set; } = null!;

    public decimal UserBalance { get; private set; }

    public Money Value
    {
        get => new(Amount, Currency);
        private set => (Amount, Currency) = (Math.Round(value.Amount,2), value.Currency);
    }

    public string Source { get; private set; }

    private decimal Amount { get; set; } = 0;

    private Currency Currency { get; set; }

    private Transaction() : base(Guid.NewGuid()) { }

    private Transaction(User user, Wallet wallet, Money amount, Guid thirdPartyTransactionId, string source) : base(Guid.NewGuid())
    {
        User = user;
        Wallet = wallet;
        Value = amount;
        UserBalance = wallet.WalletBalance.Amount + amount.Amount;
        ThirdPartyTransactionId = thirdPartyTransactionId;
        Source = source;
    }

    internal static Transaction Create(User user, Wallet wallet, Money amount, Guid thirdPartyTransactionId, string source) =>
        new Transaction(user, wallet, amount, thirdPartyTransactionId, source);
}