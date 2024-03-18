using BalanceKube.Contracts;
using BalanceKube.EventGenerator.API.Common;
using BalanceKube.EventGenerator.API.Entities;

namespace BalanceKube.EventGenerator.API.Services
{
    public class TransactionEventGenerator
    {
        private readonly Random _random = new Random();

        private readonly string[] _currencies = [
            "EUR",
            "USD",
            "CHF"
        ];

        public TransactionEvent GenerateRandomEvent(int userId)
        {
            var transactionEvent = new TransactionEvent
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Type = GenerateRandomTransactionType(),
                Amount = GenerateRandomAmount(),
                Currency = GenerateRandomCurrency(),
                CreatedAt = DateTime.Now,
                Status = TransactionStatus.Pending
            };

            return transactionEvent;
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
