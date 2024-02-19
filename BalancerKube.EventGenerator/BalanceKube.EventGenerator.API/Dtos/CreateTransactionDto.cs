namespace BalanceKube.Contracts
{
    public sealed record CreateTransactionDto(
        Guid CorrelationId,
        int UserId,
        string TransactionType,
        decimal Amount,
        string Currency,
        DateTime CreatedAt);
}
