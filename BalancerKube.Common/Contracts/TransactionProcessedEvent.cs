namespace BalancerKube.Common.Contracts;

public sealed record TransactionProcessedEvent(Guid? TransactionId, string? ErrorMessage = null);
