using BalanceKube.Contracts;
using BalanceKube.EventGenerator.API.Common;
using BalanceKube.EventGenerator.API.Entities;
using BalanceKube.EventGenerator.API.Persistence.Base;
using MassTransit;

namespace BalanceKube.EventGenerator.API.Consumers
{
    public class TransactionResultConsumer : IConsumer<TransactionResultDto>
    {
        private readonly ILogger<TransactionResultConsumer> _logger;
        private readonly IRepository<TransactionEvent> _transactionRepository;

        public TransactionResultConsumer(
            ILogger<TransactionResultConsumer> logger,
            IRepository<TransactionEvent> transactionRepository)
        {
            _logger = logger;
            _transactionRepository = transactionRepository;
        }

        public async Task Consume(ConsumeContext<TransactionResultDto> context)
        {
            var message = context.Message;

            if (message is null)
            {
                _logger.LogWarning("Consumed message is null.");
                return;
            }

            var transactionEvent = await _transactionRepository.GetAsync(message.CorrelationId);

            transactionEvent.Status = message.isSuccess ?
                TransactionStatus.Processed :
                TransactionStatus.Failed;

            transactionEvent.TransactionId = message.TransactionId ?? Guid.Empty;
            transactionEvent.ErrorMessage = message.ErrorMessage ?? string.Empty;

            await _transactionRepository.UpdateAsync(transactionEvent);
        }
    }
}
