namespace BalancerKube.Wallet.API.Models.Request
{
    public record CreateTransactionRequest(
        Guid CorrelationId,
        int UserId,
        string TransactionType,
        decimal Amount,
        string Currency);
}
