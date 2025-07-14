using Polly;
using Serilog;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using BalancerKube.Wallet.API.Settings;
using BalancerKube.Wallet.API.Services;
using BalancerKube.Wallets.API.Persistence;
using BalancerKube.Wallet.API.Models.Request;
using BalancerKube.Common.Contracts;
using BalancerKube.Wallet.API.Entities;
using BalancerKube.Wallet.API.Abstraction;
using BalancerKube.Wallet.API.Consumers;
using BalancerKube.Wallet.API.Utilities;
using MassTransit.Logging;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using MassTransit.Monitoring;
using OpenTelemetry.Metrics;
using BalancerKube.Common.Monitoring;
using Npgsql;
using Serilog.Enrichers.Span;
using BalancerKube.Common.Telemetry;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog();
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("WalletDb")));

builder.Services.AddScoped<IWalletService, WalletService>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var telemetrySettings = builder.Configuration
    .GetSection(nameof(TelemetrySettings))
    .Get<TelemetrySettings>() ?? new();

builder.Services.AddOpenTelemetry()
    .WithTracing(tracingBuilder =>
    {
        tracingBuilder
            .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService(
                serviceName: RunTimeDiagnosticConfig.ServiceName,
                serviceVersion: RunTimeDiagnosticConfig.ServiceVersion,
                serviceInstanceId: RunTimeDiagnosticConfig.ServiceInstanceId))
            .AddSource(RunTimeDiagnosticConfig.Source.Name)
            .AddSource(DiagnosticHeaders.DefaultListenerName)
            .AddAspNetCoreInstrumentation()
            .AddHttpClientInstrumentation()
            .AddRedisInstrumentation()
            .AddNpgsql()
            .AddOtlpExporter(o => o.Endpoint = new Uri(telemetrySettings.TracesEndpoint));
    })
    .WithMetrics(metricProviderBuilder =>
    {
        metricProviderBuilder
            .AddMeter(RunTimeDiagnosticConfig.Meter.Name, InstrumentationOptions.MeterName)
            .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService(
                serviceName: RunTimeDiagnosticConfig.ServiceName,
                serviceVersion: RunTimeDiagnosticConfig.ServiceVersion,
                serviceInstanceId: RunTimeDiagnosticConfig.ServiceInstanceId))
            .AddRuntimeInstrumentation()
            .AddHttpClientInstrumentation()
            .AddAspNetCoreInstrumentation()
            .AddOtlpExporter(o => o.Endpoint = new Uri(telemetrySettings.TracesEndpoint));
    });

var rabbitMqSettings = new RabbitMQSettings();
builder.Configuration.GetSection("RabbitMQSettings").Bind(rabbitMqSettings);

builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<TransactionCommandConsumer>();

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

        cfg.ReceiveEndpoint($"{rabbitMqSettings?.EndpointName}-transaction-queue", e =>
        {
            e.UseMessageRetry(r => r.Interval(3, TimeSpan.FromSeconds(10)));

            e.Bind<DepositFundsCommand>(s =>
            {
                s.RoutingKey = "Deposit";
            });

            e.Bind<WithdrawFundsCommand>(s =>
            {
                s.RoutingKey = "Withdrawal";
            });

            e.Consumer<TransactionCommandConsumer>(context);
        });
    });

    x.AddOpenTelemetry();
});

var app = builder.Build();

// Migrate pending migrations
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

    Policy.Handle<Exception>()
        .WaitAndRetry(
            new[]
            {
                TimeSpan.FromSeconds(1),
                TimeSpan.FromSeconds(5),
                TimeSpan.FromSeconds(10)
            }
        )
        .Execute(() =>
        {
            try
            {
                context.Database.Migrate();
            }
            catch (Exception ex)
            {
                throw new Exception("Can't connect to or migrate the Database!");
            }
        });
}

app.UseSwagger();
app.UseSwaggerUI();
app.UseSerilogRequestLogging();

app.MapPost("/api/transaction", async (ProcessTransactionRequest request, IWalletService walletService) =>
{
    var result = await walletService.ProcessTransactionAsync(request);

    if (result.IsFaulted)
    {
        return Results.BadRequest(result.Exception?.Message);
    }

    return Results.Ok(result.Value);
});

app.MapPost("/api/user", async (CreateUserRequest request, ApplicationDbContext applicationDb, IPublishEndpoint publishEndpoint) =>
{
    if (string.IsNullOrWhiteSpace(request.Username))
    {
        return Results.BadRequest($"{nameof(request.Username)} is a required field.");
    }

    var user = User.Create(
        request.Username,
        request.City,
        request.Address);

    applicationDb.Users.Add(user);

    await applicationDb.SaveChangesAsync();

    await publishEndpoint.Publish(new UserRegisteredEvent(user.Id, user.Username));

    return Results.Ok(user.Id);
});

try
{
    Log.Logger = new LoggerConfiguration()
        .ReadFrom.Configuration(builder.Configuration)
        .Enrich.WithProperty(nameof(RunTimeDiagnosticConfig.ServiceName), RunTimeDiagnosticConfig.ServiceName)
        .Enrich.WithProperty(nameof(RunTimeDiagnosticConfig.ServiceInstanceId), RunTimeDiagnosticConfig.ServiceInstanceId)
        .WriteTo.Sink(new SerilogOpenTelemetrySpanSink())
        .WriteTo.OpenTelemetry(options =>
        {
            options.Endpoint = telemetrySettings.TracesEndpoint;
            options.Protocol = Serilog.Sinks.OpenTelemetry.OtlpProtocol.HttpProtobuf;
        })
        .CreateLogger();

    app.Run();

    Log.Information("Application starting up.");
}
catch (Exception ex)
{
    Log.Fatal(ex, "The application failed to start correctly");
}
finally
{
    Log.CloseAndFlush();
}