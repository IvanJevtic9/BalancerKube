using Serilog;
using MassTransit;
using BalanceKube.EventGenerator.API.Entities;
using BalanceKube.EventGenerator.API.Persistence;
using BalanceKube.EventGenerator.API.Services;
using BalanceKube.EventGenerator.API.Settings;
using BalanceKube.EventGenerator.API.HostedService;
using BalanceKube.EventGenerator.API.Consumers;
using BalancerKube.Common.Contracts;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Host.UseSerilog();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSingleton<TransactionEventGeneratorService>();
builder.Services.AddMongo();

builder.Services.AddMongoRepository<ThirdPartyTransaction>("transactions");
builder.Services.AddMongoRepository<User>("users");

var rabbitMqSettings = new RabbitMQSettings();
builder.Configuration.GetSection("RabbitMQSettings").Bind(rabbitMqSettings);

builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<TransactionProcessedConsumer>();
    x.AddConsumer<UserRegisteredConsumer>();

    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.UseScheduledRedelivery(r => r.Intervals(
            TimeSpan.FromMinutes(1),
            TimeSpan.FromMinutes(5),
            TimeSpan.FromMinutes(10)));

        cfg.Host(rabbitMqSettings?.Host, "/", h =>
        {
            h.Username(rabbitMqSettings?.Username);
            h.Password(rabbitMqSettings?.Password);
        });

        cfg.ReceiveEndpoint($"{rabbitMqSettings?.EndpointName}-transaction-processed-queue", e =>
        {
            e.UseMessageRetry(r => r.Interval(3, TimeSpan.FromSeconds(10)));

            e.Bind<TransactionProcessedEvent>(s =>
            {
                s.RoutingKey = nameof(TransactionProcessedEvent);
            });

            e.Consumer<TransactionProcessedConsumer>(context);
        });

        cfg.ReceiveEndpoint($"{rabbitMqSettings?.EndpointName}-user-registered-queue", e =>
        {
            e.UseMessageRetry(r => r.Interval(3, TimeSpan.FromSeconds(10)));

            e.Bind<UserRegisteredEvent>(s =>
            {
                s.RoutingKey = nameof(UserRegisteredEvent);
            });

            e.Consumer<UserRegisteredConsumer>(context);
        });
    });
});

builder.Services.AddHostedService<EventBackgroundService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseSwagger();
app.UseSwaggerUI();
app.UseSerilogRequestLogging();
app.UseHttpsRedirection();

app.MapControllers();

try
{
    // Kubernetes sets the Pod name in the HOSTNAME environment variable
    Log.Logger = new LoggerConfiguration()
        .Enrich.FromLogContext()
        .Enrich.WithProperty("ServiceName", "EventGeneratorService")
        .Enrich.WithProperty("InstanceId", Environment.GetEnvironmentVariable("HOSTNAME"))
        .WriteTo.Console()
        .CreateLogger();

    Log.Information("Application starting up.");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "The application failed to start correctly");
}
finally
{
    Log.CloseAndFlush();
}
