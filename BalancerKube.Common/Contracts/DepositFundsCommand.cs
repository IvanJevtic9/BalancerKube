namespace BalancerKube.Common.Contracts;

public sealed record DepositFundsCommand(
    Guid ThirdPartyTransactionId,
    int UserId,
    decimal Amount,
    string Currency,
    string Source,
    DateTime CreatedAt) : ITransactionCommand;
