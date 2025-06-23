namespace BalancerKube.Wallet.API.Models.Request
{
    public record ProcessTransactionRequest(
        Guid ThirdPartyTransactionId,
        int UserId,
        string TransactionType,
        decimal Amount,
        string Currency,
        string Source);
}
