using MassTransit;
using BalanceKube.EventGenerator.API.Services;
using BalanceKube.EventGenerator.API.Entities;
using BalanceKube.EventGenerator.API.Persistence.Base;

namespace BalanceKube.EventGenerator.API.HostedService
{
    public class EventBackgroundService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<EventBackgroundService> _logger;
        private readonly IRepository<TransactionEvent> _transactionRepository;

        private readonly TransactionEventGenerator _eventGenerator;

        public EventBackgroundService(
            IServiceProvider serviceProvider,
            ILogger<EventBackgroundService> logger,
            IRepository<TransactionEvent> transactionRepository,
            TransactionEventGenerator eventGenerator)
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
                using (var scope = _serviceProvider.CreateScope())
                {
                    var publishEndpoint = scope.ServiceProvider.GetRequiredService<IPublishEndpoint>();

                    var transactionEvent = _eventGenerator.GenerateRandomEvent(1);

                    _logger.LogInformation($"Event has been produced with correlation ID: {transactionEvent.Id}");

                    await _transactionRepository.CreateAsync(transactionEvent);
                    await publishEndpoint.Publish(_eventGenerator.MapPublishModel(transactionEvent), stoppingToken);
                }

                await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
            }
        }
    }
}
