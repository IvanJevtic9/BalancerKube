using BalanceKube.EventGenerator.API.Common;
using BalanceKube.EventGenerator.API.Entities;
using BalancerKube.Common.Contracts;

namespace BalanceKube.EventGenerator.API;

public static class Mapper
{
    public static object MapToContract(this ThirdPartyTransaction transaction)
    {
        return transaction.Type switch
        {
            TransactionType.Withdrawal => new WithdrawFundsCommand(
                ThirdPartyTransactionId: transaction.Id,
                UserId: transaction.UserId,
                Amount: transaction.Amount,
                Currency: transaction.Currency,
                Reason: transaction.Source,
                CreatedAt: transaction.CreatedAt),
            TransactionType.Deposit => new DepositFundsCommand(
                ThirdPartyTransactionId: transaction.Id,
                UserId: transaction.UserId,
                Amount: transaction.Amount,
                Currency: transaction.Currency,
                Source: transaction.Source,
                CreatedAt: transaction.CreatedAt),
            _ => throw new NotSupportedException("Unsupported request type")
        };
    }
}
