using BalanceKube.EventGenerator.API.Common;
using BalanceKube.EventGenerator.API.Entities.Base;

namespace BalanceKube.EventGenerator.API.Entities
{
    public class TransactionEvent : IEntity
    {
        public Guid Id { get; init; }
        public int UserId { get; init; }
        public TransactionType Type { get; init; }
        public decimal Amount { get; init; }
        public TransactionStatus Status { get; init; } = TransactionStatus.Pending;
        public DateTime CreatedAt { get; init; }
        public DateTime? ProcessedAt { get; init; }
    }
}
