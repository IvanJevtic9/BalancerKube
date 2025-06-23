using BalanceKube.EventGenerator.API.Abstraction;
using BalanceKube.EventGenerator.API.Common;
using BalanceKube.EventGenerator.API.Entities;
using Bogus;

namespace BalanceKube.EventGenerator.API.Services;

public class TransactionEventGeneratorService : ITransactionEventGeneratorService
{
    private static readonly Random _random = new();
    private readonly IRepository<User> _userRepository;

    private static readonly string[] _currencies =
    [
        "EUR",
        "USD",
        "CHF"
    ];

    private static readonly string[] _depositSources =
    [
        "Bank Transfer",
        "Credit Card",
        "PayPal",
        "Crypto Wallet",
        "Cash Deposit"
    ];

    private static readonly string[] _withdrawalPurposes =
    [
        "Bought Coffee",
        "Online Shopping",
        "Utility Bill Payment",
        "Grocery Store",
        "Restaurant",
        "Travel Booking"
    ];

    public TransactionEventGeneratorService(IRepository<User> userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<ThirdPartyTransaction> GenerateTransactionAsync()
    {
        var userId = await GetRandomUserIdAsync();
        var type = GenerateRandomTransactionType();
        var amount = GenerateRandomAmount();
        var currency = GenerateRandomCurrency();
        var source = GenerateRandomSource(type);

        var transactionEvent = new ThirdPartyTransaction
        {
            UserId = userId,
            Type = type,
            Amount = amount,
            Currency = currency,
            Source = source
        };

        return transactionEvent;
    }

    private async Task<int> GetRandomUserIdAsync()
    {
        var userIds = (await _userRepository.GetAllAsync())
            .Select(user => user.UserId)
            .ToList();

        if (userIds is null || userIds.Count == 0)
        {
            throw new InvalidOperationException("No users available");
        }

        int index = _random.Next(userIds.Count);

        return userIds.ElementAt(index);
    }

    private TransactionType GenerateRandomTransactionType() => _random.Next(2) == 0
        ? TransactionType.Deposit
        : TransactionType.Withdrawal;

    private string GenerateRandomCurrency() => _currencies[_random.Next(3)];

    private decimal GenerateRandomAmount() => (decimal)(Math.Pow(_random.NextDouble(), 3) * 10000);

    private string GenerateRandomSource(TransactionType type) => type switch
    {
        TransactionType.Deposit => _depositSources[_random.Next(_depositSources.Length)],
        TransactionType.Withdrawal => _withdrawalPurposes[_random.Next(_withdrawalPurposes.Length)],
        _ => throw new NotImplementedException()
    };
}
