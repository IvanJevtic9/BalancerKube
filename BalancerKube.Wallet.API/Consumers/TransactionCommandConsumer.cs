using MassTransit;
using BalancerKube.Common.Contracts;
using BalancerKube.Common.Models;
using BalancerKube.Wallet.API.Abstraction;
using BalancerKube.Wallet.API.Exceptions;
using BalancerKube.Wallet.API.Models.Request;

namespace BalancerKube.Wallet.API.Consumers;

public class TransactionCommandConsumer : IConsumer<DepositFundsCommand>, IConsumer<WithdrawFundsCommand>
{
    private readonly IWalletService _walletService;
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly ILogger<TransactionCommandConsumer> _logger;

    public TransactionCommandConsumer(
        IWalletService walletService,
        IPublishEndpoint publishEndpoint,
        ILogger<TransactionCommandConsumer> logger)
    {
        _logger = logger;
        _walletService = walletService;
        _publishEndpoint = publishEndpoint;
    }

    public async Task Consume(ConsumeContext<DepositFundsCommand> context)
    {
        await Consume(context as ConsumeContext<ITransactionCommand>);
    }

    public async Task Consume(ConsumeContext<WithdrawFundsCommand> context)
    {
        await Consume(context as ConsumeContext<ITransactionCommand>);
    }

    private async Task Consume(ConsumeContext<ITransactionCommand> context)
    {
        var message = context.Message;

        if (message is null)
        {
            _logger.LogWarning("Consumed message is null.");

            return;
        }

        var result = await ProcessTransaction(message);

        _ = result.IsSuccess
            ? HandleSuccess(message, result.Value)
            : HandleFailure(message.ThirdPartyTransactionId, result.Exception!);
    }

    private async Task<Result<Guid>> ProcessTransaction(ITransactionCommand message)
    {
        var result = message switch
        {
            DepositFundsCommand depositCommand => await _walletService.ProcessTransactionAsync(
                new ProcessTransactionRequest(
                    depositCommand.ThirdPartyTransactionId,
                    depositCommand.UserId,
                    TransactionType.Deposit,
                    depositCommand.Amount,
                    depositCommand.Currency,
                    depositCommand.Source)
            ),
            WithdrawFundsCommand withdrawalCommand => await _walletService.ProcessTransactionAsync(
                new ProcessTransactionRequest(
                    withdrawalCommand.ThirdPartyTransactionId,
                    withdrawalCommand.UserId,
                    TransactionType.Withdrawal,
                    withdrawalCommand.Amount,
                    withdrawalCommand.Currency,
                    withdrawalCommand.Reason)
            ),
            _ => Result<Guid>.Failure("Unsupported transaction command type.")
        };

        return result;
    }

    private async Task HandleSuccess(ITransactionCommand command, Guid transactionId)
    {
        _logger.LogWarning(
            "Transaction processed successfully. CorrelationId: {CorrelationId}, UserId: {UserId}, Amount: {Amount} {Currency}, Timestamp: {Timestamp}",
            command.ThirdPartyTransactionId,
            command.UserId,
            command.Amount,
            command.Currency,
            command.CreatedAt);

        if (transactionId == Guid.Empty)
        {
            return;
        }

        await _publishEndpoint.Publish(
            new TransactionProcessedEvent(transactionId),
            context =>
            {
                context.CorrelationId = command.ThirdPartyTransactionId;
                context.SetRoutingKey(nameof(TransactionProcessedEvent));
            }
        );
    }

    private async Task HandleFailure(Guid thirdPartyTransactionId, Exception exception)
    {
        switch (exception)
        {
            case Exceptions.ConcurrencyException ce:
                _logger.LogWarning(
                    "Concurrency exception for transaction {CorrelationId}: {Message}. The message will be retried.",
                    thirdPartyTransactionId,
                    ce.Message);

                throw ce;

            case ValidationException ve:
                _logger.LogWarning(
                    "Validation exception for transaction {CorrelationId}: {Message}. Returning error response.",
                    thirdPartyTransactionId,
                    ve.Message);

                await _publishEndpoint.Publish(
                    new TransactionProcessedEvent(null, ve.Message),
                    context =>
                    {
                        context.CorrelationId = thirdPartyTransactionId;
                        context.SetRoutingKey(nameof(TransactionProcessedEvent));
                    });

                break;

            default:
                _logger.LogError(
                    exception,
                    "Unhandled exception for transaction {CorrelationId}. Exception: {Message}",
                    thirdPartyTransactionId, exception.Message);

                break;
        }
    }
}
