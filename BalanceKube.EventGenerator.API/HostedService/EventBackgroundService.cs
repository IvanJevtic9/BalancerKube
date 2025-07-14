using MassTransit;
using BalanceKube.EventGenerator.API.Services;
using BalanceKube.EventGenerator.API.Entities;
using BalanceKube.EventGenerator.API.Abstraction;
using BalanceKube.EventGenerator.API.Utilities;
using System.Diagnostics;

namespace BalanceKube.EventGenerator.API.HostedService;

public class EventBackgroundService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<EventBackgroundService> _logger;
    private readonly IRepository<ThirdPartyTransaction> _transactionRepository;
    private readonly TransactionEventGeneratorService _eventGenerator;

    public EventBackgroundService(
        IServiceProvider serviceProvider,
        ILogger<EventBackgroundService> logger,
        IRepository<ThirdPartyTransaction> transactionRepository,
        TransactionEventGeneratorService eventGenerator)
    {
        _logger = logger;
        _eventGenerator = eventGenerator;
        _serviceProvider = serviceProvider;
        _transactionRepository = transactionRepository;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            using var activity = RunTimeDiagnosticConfig.Source.StartActivity("GenerateAndPublishTransaction");
            {
                using var scope = _serviceProvider.CreateScope();
                var publishEndpoint = scope.ServiceProvider.GetRequiredService<IPublishEndpoint>();

                try
                {
                    var thirdPartyTransaction = await _eventGenerator.GenerateTransactionAsync();

                    activity?.SetTag("correlationId", thirdPartyTransaction.Id);
                    activity?.SetTag("userId", thirdPartyTransaction.UserId);
                    activity?.AddEvent(new ActivityEvent("Event generated"));

                    await _transactionRepository.CreateAsync(thirdPartyTransaction);

                    activity?.AddEvent(new ActivityEvent("Transaction persisted"));

                    await publishEndpoint.Publish(
                        thirdPartyTransaction.MapToContract(),
                        context =>
                        {
                            context.CorrelationId = thirdPartyTransaction.Id;
                            context.SetRoutingKey(thirdPartyTransaction.Type.ToString());
                        },
                        stoppingToken);

                    activity?.AddEvent(new ActivityEvent("Event published"));

                    _logger.LogInformation(
                        "Transaction event generated and published. CorrelationId: {CorrelationId}, UserId: {UserId}, Amount: {Amount} {Currency}, Timestamp: {Timestamp}",
                        thirdPartyTransaction.Id,
                        thirdPartyTransaction.UserId,
                        thirdPartyTransaction.Amount,
                        thirdPartyTransaction.Currency,
                        thirdPartyTransaction.CreatedAt);
                }
                catch (Exception ex)
                {
                    activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
                    activity?.AddException(ex);

                    _logger.LogError(ex.Message);
                }
            }

            await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
        }
    }
}
