using Serilog;
using MassTransit;
using BalanceKube.EventGenerator.API.Entities;
using BalanceKube.EventGenerator.API.Services;
using BalanceKube.EventGenerator.API.Settings;
using BalanceKube.EventGenerator.API.HostedService;
using BalanceKube.EventGenerator.API.Consumers;
using BalancerKube.Common.Contracts;
using BalanceKube.EventGenerator.API.Utilities;
using BalanceKube.EventGenerator.API.Persistence;
using BalancerKube.Common.Monitoring;
using MassTransit.Logging;
using MassTransit.Monitoring;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using OpenTelemetry.Metrics;
using Serilog.Enrichers.Span;
using BalancerKube.Common.Telemetry;

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

var rabbitMqSettings = builder.Configuration
    .GetSection(nameof(RabbitMQSettings))
    .Get<RabbitMQSettings>() ?? throw new ArgumentNullException(nameof(RabbitMQSettings));

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

    x.AddOpenTelemetry();
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
