using BalanceKube.EventGenerator.API.Entities;

namespace BalanceKube.EventGenerator.API.Abstraction;

public interface ITransactionEventGeneratorService
{
    Task<ThirdPartyTransaction> GenerateTransactionAsync();
}