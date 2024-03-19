using BalanceKube.Contracts;
using BalanceKube.EventGenerator.API.Common;
using BalanceKube.EventGenerator.API.Entities;
using BalanceKube.EventGenerator.API.Persistence.Base;

namespace BalanceKube.EventGenerator.API.Services
{
    public class TransactionEventGenerator
    {
        private readonly Random _random = new Random();
        private readonly IRepository<User> _userRepository;

        private readonly string[] _currencies = [
            "EUR",
            "USD",
            "CHF"
        ];

        public TransactionEventGenerator(IRepository<User> userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<TransactionEvent> GenerateRandomEvent()
        {
            var transactionEvent = new TransactionEvent
            {
                Id = Guid.NewGuid(),
                UserId = await GetRandomUserIdAsync(),
                Type = GenerateRandomTransactionType(),
                Amount = GenerateRandomAmount(),
                Currency = GenerateRandomCurrency(),
                CreatedAt = DateTime.UtcNow,
                Status = TransactionStatus.Pending
            };

            return transactionEvent;
        }

        public async Task<int> GetRandomUserIdAsync()
        {
            var users = await _userRepository.GetAllAsync();
            
            if (users == null || !users.Any())
            {
                throw new InvalidOperationException("No users available");
            }

            int index = _random.Next(users.Count);
            return users.ElementAt(index).UserId;
        }

        public CreateTransactionDto MapPublishModel(TransactionEvent tr) =>
            new CreateTransactionDto(
                tr.Id,
                tr.UserId,
                tr.Type.ToString().ToLower(),
                tr.Amount,
                tr.Currency,
                tr.CreatedAt);

        private TransactionType GenerateRandomTransactionType() =>
            _random.Next(2) == 0 ?
                TransactionType.Deposit :
                TransactionType.Withdrawal;

        private string GenerateRandomCurrency() =>
            _currencies[_random.Next(3)];

        private decimal GenerateRandomAmount() =>
            (decimal)(Math.Pow(_random.NextDouble(), 3) * 10000);
    }
}
