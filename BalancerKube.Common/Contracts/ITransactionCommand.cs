namespace BalancerKube.Common.Contracts;

public interface ITransactionCommand
{
    Guid ThirdPartyTransactionId { get; }

    int UserId { get; }

    decimal Amount { get; }

    string Currency { get; }

    DateTime CreatedAt { get; }
}
