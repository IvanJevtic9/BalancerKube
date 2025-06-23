using BalancerKube.Common.Models;
using BalancerKube.Wallet.API.Models.Request;

namespace BalancerKube.Wallet.API.Abstraction;

public interface IWalletService
{
    Task<Result<Guid>> ProcessTransactionAsync(ProcessTransactionRequest request);
}
