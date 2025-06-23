namespace BalancerKube.Common.Contracts;

public sealed record WithdrawFundsCommand(
    Guid ThirdPartyTransactionId,
    int UserId,
    decimal Amount,
    string Currency,
    string Reason,
    DateTime CreatedAt) : ITransactionCommand;