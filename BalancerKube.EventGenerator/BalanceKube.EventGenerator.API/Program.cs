using Serilog;
using MassTransit;
using Serilog.Formatting.Json;
using BalanceKube.Contracts;
using BalanceKube.EventGenerator.API.Entities;
using BalanceKube.EventGenerator.API.Persistence;
using BalanceKube.EventGenerator.API.Services;
using BalanceKube.EventGenerator.API.Settings;
using BalanceKube.EventGenerator.API.HostedService;
using BalanceKube.EventGenerator.API.Consumers;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Host.UseSerilog();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSingleton<TransactionEventGenerator>();
builder.Services.AddMongo();
builder.Services.AddMongoRepository<TransactionEvent>("transactions");
builder.Services.AddMongoRepository<User>("users");

var rabbitMqSettings = new RabbitMQSettings();
builder.Configuration.GetSection("RabbitMQSettings").Bind(rabbitMqSettings);

builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<TransactionResultConsumer>();

    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host(rabbitMqSettings?.Host, "/", h =>
        {
            h.Username(rabbitMqSettings?.Username);
            h.Password(rabbitMqSettings?.Password);
        });

        cfg.ReceiveEndpoint($"{rabbitMqSettings?.EndpointName}-transaction-result", e =>
        {
            e.UseMessageRetry(r => r.Interval(3, TimeSpan.FromSeconds(10)));
            e.Consumer<TransactionResultConsumer>(context);
            e.Bind(ContractNames.TransactionResult);
        });

        cfg.ConfigureEndpoints(
            context,
            new KebabCaseEndpointNameFormatter(rabbitMqSettings?.EndpointName, false));
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
        .WriteTo.Console(new JsonFormatter())
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
