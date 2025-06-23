using BalanceKube.EventGenerator.API.Common;
using BalancerKube.Common.Domain;

namespace BalanceKube.EventGenerator.API.Entities;

public sealed class ThirdPartyTransaction : Entity<Guid>
{
    public ThirdPartyTransaction() : base(Guid.NewGuid())
    { }

    public int UserId { get; init; }

    public decimal Amount { get; init; }

    public string Currency { get; init; } = null!;

    public TransactionType Type { get; init; }

    public TransactionStatus Status { get; private set; } = TransactionStatus.Pending;

    public Guid? TransactionId { get; private set; }

    public string Source { get; set; } = string.Empty;

    public string? ErrorMessage { get; set; } = string.Empty;

    public DateTime CreatedAt { get; } = DateTime.Now;

    public DateTime? ProcessedAt { get; private set; }

    public void MarkAsProcessed(Guid? transactionId, string? errorMessage)
    {
        TransactionId = transactionId;
        ErrorMessage = errorMessage;
        ProcessedAt = DateTime.Now;

        Status = string.IsNullOrEmpty(errorMessage)
            ? TransactionStatus.Processed
            : TransactionStatus.Rejected;
    }
}
