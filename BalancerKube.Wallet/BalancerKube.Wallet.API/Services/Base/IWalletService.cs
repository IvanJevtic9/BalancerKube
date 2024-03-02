using BalancerKube.Wallet.API.Models.Common;
using BalancerKube.Wallet.API.Models.Request;

namespace BalancerKube.Wallet.API.Services.Base
{
    public interface IWalletService
    {
        Task<Result<Guid>> ProcessTransactionAsync(CreateTransactionRequest request);
    }
}
