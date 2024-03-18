using BalanceKube.EventGenerator.API.Common;
using BalanceKube.EventGenerator.API.Entities.Base;

namespace BalanceKube.EventGenerator.API.Entities
{
    public sealed class TransactionEvent : IEntity
    {
        public Guid Id { get; set; }
        public int UserId { get; set; }
        public TransactionType Type { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; } = null!;
        public TransactionStatus Status { get; set; } = TransactionStatus.Pending;
        public Guid TransactionId { get; set; }
        public string ErrorMessage { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime? ProcessedAt { get; set; }


    }
}
