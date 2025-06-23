using StackExchange.Redis;
using Microsoft.EntityFrameworkCore;
using BalancerKube.Wallet.API.Exceptions;
using BalancerKube.Wallets.API.Persistence;
using BalancerKube.Wallet.API.Models.Request;
using BalancerKube.Common.Models;
using BalancerKube.Wallet.API.Abstraction;

namespace BalancerKube.Wallet.API.Services;

public class WalletService : IWalletService
{
    private readonly IDatabase _db;
    private readonly ConnectionMultiplexer _redis;
    private readonly ApplicationDbContext _dbContext;

    public WalletService(ApplicationDbContext dbContext, IConfiguration configuration)
    {
        var redisConnectionString = configuration.GetConnectionString("RedisDb");
        ArgumentNullException.ThrowIfNull(redisConnectionString);

        _redis = ConnectionMultiplexer.Connect(redisConnectionString);
        _db = _redis.GetDatabase();
        _dbContext = dbContext;
    }

    public async Task<Result<Guid>> ProcessTransactionAsync(ProcessTransactionRequest request)
    {
        if (!IsTransactionIdempotent(request.ThirdPartyTransactionId))
        {
            return new Result<Guid>(Guid.Empty);
        }

        var errorMessage = ValidateRequest(request).FirstOrDefault();

        if (!string.IsNullOrEmpty(errorMessage))
        {
            return new Result<Guid>(new ValidationException(errorMessage));
        }

        return await TryProcessTransaction(request);
    }

    private bool IsTransactionIdempotent(Guid thirdPartyTransactionId) => !_dbContext.Transactions.Any(x => x.ThirdPartyTransactionId == thirdPartyTransactionId);

    private IEnumerable<string> ValidateRequest(ProcessTransactionRequest request)
    {
        if (request.Amount <= 0)
        {
            yield return $"{nameof(request.Amount)} must be greater then zero.";
        }

        if (string.IsNullOrWhiteSpace(request.TransactionType)
            || (request.TransactionType != TransactionType.Deposit && request.TransactionType != TransactionType.Withdrawal))
        {
            yield return $"{nameof(request.TransactionType)} {request.TransactionType} is not valid.";
        }

        if (string.IsNullOrWhiteSpace(request.Currency))
        {
            yield return $"{nameof(request.Currency)} is a required field.";
        }

        if (!Currency.VerifyCurrency(request.Currency))
        {
            yield return $"{nameof(request.Currency)} {request.Currency} is not supported currency.";
        }

        if (request.ThirdPartyTransactionId == Guid.Empty)
        {
            yield return $"{nameof(request.ThirdPartyTransactionId)} is a required field.";
        }
    }

    private async Task<Result<Guid>> TryProcessTransaction(ProcessTransactionRequest request)
    {
        var lockKey = GetLockKey(request);
        var lockToken = Guid.NewGuid().ToString();
        var acquired = await AcquireLockAsync(lockKey, lockToken);

        if (!acquired)
        {
            return new Result<Guid>(new ConcurrencyException("Could not acquire lock"));
        }

        try
        {
            return await ProcessTransaction(request);
        }
        finally
        {
            await ReleaseLockAsync(lockKey, lockToken);
        }
    }

    private string GetLockKey(ProcessTransactionRequest request) => $"wallet:lock:{request.UserId}:{request.Currency}";

    private async Task ReleaseLockAsync(string lockKey, string lockToken) => await _db.LockReleaseAsync(lockKey, lockToken);

    private async Task<bool> AcquireLockAsync(string lockKey, string lockToken)
    {
        var timeout = TimeSpan.FromSeconds(30);
        var retryDelay = TimeSpan.FromSeconds(1);
        var deadline = DateTime.Now.Add(timeout);

        while (DateTime.Now < deadline)
        {
            if (await _db.LockTakeAsync(lockKey, lockToken, timeout))
            {
                return true;
            }

            await Task.Delay(retryDelay);
        }

        return false;
    }

    private async Task<Result<Guid>> ProcessTransaction(ProcessTransactionRequest request)
    {
        var user = await _dbContext.Users
            .Include(u => u.Wallets)
            .FirstOrDefaultAsync(user => user.Id == request.UserId);

        if (user is null)
        {
            return new Result<Guid>(new ValidationException("User not found."));
        }

        var amount = request.TransactionType == TransactionType.Deposit
            ? request.Amount
            : -request.Amount;

        var transactionResult = user.AddTransaction(
            request.ThirdPartyTransactionId,
            new Money(amount, new Currency(request.Currency)),
            request.Source);

        if (transactionResult.IsFaulted)
        {
            return new Result<Guid>(transactionResult.Exception!);
        }

        _dbContext.Transactions.Add(transactionResult.Value!);

        await _dbContext.SaveChangesAsync();

        return new Result<Guid>(transactionResult.Value!.Id);
    }
}
