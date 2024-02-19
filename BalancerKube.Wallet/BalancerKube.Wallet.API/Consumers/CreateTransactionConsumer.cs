using MassTransit;
using BalanceKube.Contracts;
using BalancerKube.Wallet.API.Services.Base;

namespace BalancerKube.Wallet.API.Consumers
{
    public class CreateTransactionConsumer : IConsumer<CreateTransactionDto>
    {
        private readonly IWalletService _walletService;
        private readonly ILogger<CreateTransactionConsumer> _logger;

        public CreateTransactionConsumer(
            IWalletService walletService,
            ILogger<CreateTransactionConsumer> logger)
        {
            _logger = logger;
            _walletService = walletService;
        }

        public async Task Consume(ConsumeContext<CreateTransactionDto> context)
        {
            var message = context.Message;

            if(message is null)
            {
                _logger.LogWarning($"Consumed message of type {nameof(CreateTransactionDto)} is null.");
                return;
            }

            var result = await _walletService.ProcessTransactionAsync(new(
                message.CorrelationId,
                message.UserId,
                message.TransactionType,
                message.Amount,
                message.Currency));

            _logger.LogInformation($"Message have been consumed, correlation ID: {message.CorrelationId}. IsSuccess: {result.IsSuccess}");
            
            // TODO publish back message about result
        }
    }
}
