using StackExchange.Redis;
using Microsoft.EntityFrameworkCore;
using BalancerKube.Wallet.Domain.Common;
using BalancerKube.Wallet.API.Exceptions;
using BalancerKube.Wallets.API.Persistence;
using BalancerKube.Wallet.API.Models.Common;
using BalancerKube.Wallet.API.Services.Base;
using BalancerKube.Wallet.API.Models.Request;

namespace BalancerKube.Wallet.API.Services
{
    public class WalletService : IWalletService
    {
        private readonly ConnectionMultiplexer _redis;
        private readonly ApplicationDbContext _applicationDb;

        private readonly IDatabase _db;

        public WalletService(
            ApplicationDbContext applicationDb,
            IConfiguration configuration)
        {
            _applicationDb = applicationDb;

            _redis = ConnectionMultiplexer.Connect(configuration.GetConnectionString("RedisDb") ?? string.Empty);
            _db = _redis.GetDatabase();
        }

        public async Task<Result<Guid>> ProcessTransactionAsync(CreateTransactionRequest request)
        {
            if (!IsTransactionIdempotent(request.CorrelationId))
            {
                return new Result<Guid>(Guid.Empty);
            }

            var errorMessage = ValidateRequest(request)
                .FirstOrDefault();

            if (!string.IsNullOrEmpty(errorMessage))
            {
                return new Result<Guid>(new ValidationException(errorMessage));
            }

            return await TryProcessTransaction(request);
        }

        private bool IsTransactionIdempotent(Guid correlationId) => !_applicationDb.Transactions.Any(x => x.CorrelationId == correlationId);

        private string GetLockKey(CreateTransactionRequest request) => $"wallet:lock:{request.UserId}:{request.Currency}";

        private async Task ReleaseLockAsync(string lockKey, string lockToken) => await _db.LockReleaseAsync(lockKey, lockToken);

        private async Task<Result<Guid>> TryProcessTransaction(CreateTransactionRequest request)
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
                return await PerformTransaction(request);
            }
            finally
            {
                await ReleaseLockAsync(lockKey, lockToken);
            }
        }

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

        private async Task<Result<Guid>> PerformTransaction(CreateTransactionRequest request)
        {
            var user = await _applicationDb.Users
                .AsSplitQuery()
                .Include(u => u.Wallets)
                .ThenInclude(u => u.Transactions)
                .FirstOrDefaultAsync(x => x.Id == request.UserId);

            if (user is null)
            {
                return new Result<Guid>(new ValidationException("User not found"));
            }

            var amount = request.TransactionType == "deposit" ? request.Amount : -request.Amount;
            var transaction = user.AddTransaction(request.CorrelationId, new Money(amount, new Currency(request.Currency)));

            _applicationDb.Transactions.Add(transaction);
            await _applicationDb.SaveChangesAsync();

            return new Result<Guid>(transaction.Id);
        }

        private IEnumerable<string> ValidateRequest(CreateTransactionRequest request)
        {
            if (request.Amount <= 0)
            {
                yield return $"{nameof(request.Amount)} must be greater then zero.";
            }

            if (string.IsNullOrWhiteSpace(request.TransactionType) ||
                (request.TransactionType != "deposit" && request.TransactionType != "withdrawal"))
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

            if (request.CorrelationId == Guid.Empty)
            {
                yield return $"{nameof(request.CorrelationId)} is a required field.";
            }
        }
    }
}
