using MassTransit;
using System.Diagnostics;
using BalanceKube.EventGenerator.API.Services;
using BalanceKube.EventGenerator.API.Entities;
using BalanceKube.EventGenerator.API.Abstraction;

namespace BalanceKube.EventGenerator.API.HostedService;

public class EventBackgroundService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<EventBackgroundService> _logger;
    private readonly IRepository<ThirdPartyTransaction> _transactionRepository;
    private readonly TransactionEventGeneratorService _eventGenerator;

    private static readonly ActivitySource _activitySource = new("EventGenerator");

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
            //using var activity = _activitySource.StartActivity("GenerateAndPublishEvent");

            using var scope = _serviceProvider.CreateScope();
            var publishEndpoint = scope.ServiceProvider.GetRequiredService<IPublishEndpoint>();

            try
            {
                var thirdPartyTransaction = await _eventGenerator.GenerateTransactionAsync();

                //activity?.SetTag("transaction.id", thirdPartyTransaction.Id);
                //activity?.AddEvent(new ActivityEvent("Event generated"));

                await _transactionRepository.CreateAsync(thirdPartyTransaction);

                await publishEndpoint.Publish(
                    thirdPartyTransaction.MapToContract(),
                    context =>
                    {
                        context.CorrelationId = thirdPartyTransaction.Id;
                        context.SetRoutingKey(thirdPartyTransaction.Type.ToString());
                    },
                    stoppingToken);

                //activity?.AddEvent(new ActivityEvent("Event published"));
            }
            catch (Exception ex)
            {
                //activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
                //activity?.AddException(ex);
            }

            await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
        }
    }
}
