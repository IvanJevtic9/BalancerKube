using BalancerKube.Wallet.API.Models.Request;
using LanguageExt.Common;

namespace BalancerKube.Wallet.API.Services.Base
{
    public interface IWalletService
    {
        Task<Result<Guid>> ProcessTransactionAsync(CreateTransactionRequest request);
    }
}
