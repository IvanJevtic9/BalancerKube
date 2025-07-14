using MassTransit;
using BalanceKube.EventGenerator.API.Entities;
using BalancerKube.Common.Contracts;
using BalanceKube.EventGenerator.API.Abstraction;
using BalanceKube.EventGenerator.API.Utilities;
using System.Diagnostics;

namespace BalanceKube.EventGenerator.API.Consumers;

public sealed class TransactionProcessedConsumer : IConsumer<TransactionProcessedEvent>
{
    private readonly ILogger<TransactionProcessedConsumer> _logger;
    private readonly IRepository<ThirdPartyTransaction> _thirdPartyTransactionRepository;

    public TransactionProcessedConsumer(
        ILogger<TransactionProcessedConsumer> logger,
        IRepository<ThirdPartyTransaction> thirdPartyTransactionRepository)
    {
        _logger = logger;
        _thirdPartyTransactionRepository = thirdPartyTransactionRepository;
    }

    public async Task Consume(ConsumeContext<TransactionProcessedEvent> context)
    {
        var message = context.Message;

        if (message is null)
        {
            _logger.LogWarning("Consumed message is null.");
            return;
        }

        using var activity = RunTimeDiagnosticConfig.Source.StartActivity("TransactionProcessed");
        activity?.SetTag("correlationId", context.CorrelationId);
        activity?.AddEvent(new ActivityEvent($"Transaction with id: {context.CorrelationId} has been processed."));

        var transaction = await _thirdPartyTransactionRepository.GetAsync(context.CorrelationId!.Value);

        transaction.MarkAsProcessed(message.TransactionId, message.ErrorMessage);

        await _thirdPartyTransactionRepository.UpdateAsync(transaction);

        _logger.LogInformation($"Transaction with id: {transaction.Id} has been processed.");
    }
}
