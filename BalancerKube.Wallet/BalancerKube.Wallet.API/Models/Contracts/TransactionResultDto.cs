namespace BalanceKube.Contracts
{
    public sealed record TransactionResultDto(
        bool isSuccess,
        Guid CorrelationId,
        Guid? TransactionId,
        string? ErrorMessage);
}
