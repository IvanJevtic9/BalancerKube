using MassTransit;
using BalanceKube.Contracts;
using BalancerKube.Wallet.API.Exceptions;
using BalancerKube.Wallet.API.Services.Base;
using BalancerKube.Wallet.API.Models.Common;

namespace BalancerKube.Wallet.API.Consumers
{
    public class CreateTransactionConsumer : IConsumer<CreateTransactionDto>
    {
        private readonly IWalletService _walletService;
        private readonly IPublishEndpoint _publishEndpoint;
        private readonly ILogger<CreateTransactionConsumer> _logger;

        public CreateTransactionConsumer(
            IWalletService walletService,
            IPublishEndpoint publishEndpoint,
            ILogger<CreateTransactionConsumer> logger)
        {
            _logger = logger;
            _walletService = walletService;
            _publishEndpoint = publishEndpoint;
        }

        public async Task Consume(ConsumeContext<CreateTransactionDto> context)
        {
            var message = context.Message;

            if (message is null)
            {
                LogWarning("Consumed message is null.");
                return;
            }

            var result = await ProcessTransaction(message);

            if (result.IsSuccess)
            {
                await HandleSuccess(message, result);
            }
            else
            {
                await HandleFailure(message, result);
            }
        }

        private async Task<Result<Guid>> ProcessTransaction(CreateTransactionDto message) =>
            await _walletService.ProcessTransactionAsync(new(
                    message.CorrelationId,
                    message.UserId,
                    message.TransactionType,
                    message.Amount,
                    message.Currency));

        private async Task HandleSuccess(CreateTransactionDto message, Result<Guid> result)
        {
            LogInformation($"Message consumed, correlation ID: {message.CorrelationId}. IsSuccess: {result.IsSuccess}.");
            if (result.Value != Guid.Empty)
            {
                await _publishEndpoint.Publish(new TransactionResultDto(
                    true,
                    message.CorrelationId,
                    result.Value,
                    string.Empty));
            }
        }

        private async Task HandleFailure(CreateTransactionDto message, Result<Guid> result)
        {
            switch (result.Exception)
            {
                case Exceptions.ConcurrencyException ce:
                    LogWarning($"Concurrency exception for message {message.CorrelationId}: {ce.Message}. Will retry.");
                    throw ce; // Rethrowing maintains the original stack trace.

                case ValidationException ve:
                    LogWarning($"Validation exception for message {message.CorrelationId}: {ve.Message}. Return error response");
                    await _publishEndpoint.Publish(new TransactionResultDto(
                        false,
                        message.CorrelationId,
                        Guid.Empty,
                        ve.Message));
                    break;

                default:
                    // Handle other types of exceptions or a general fault case.
                    LogError($"Unhandled exception for message {message.CorrelationId}. Exception: {result.Exception?.Message}");
                    break;
            }
        }

        private void LogInformation(string message) => _logger.LogInformation(message);
        private void LogWarning(string message) => _logger.LogWarning(message);
        private void LogError(string message) => _logger.LogError(message);
    }
}
