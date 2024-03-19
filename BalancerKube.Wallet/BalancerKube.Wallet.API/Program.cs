using Polly;
using Serilog;
using MassTransit;
using BalancerKube.Contracts;
using Microsoft.EntityFrameworkCore;
using BalancerKube.Wallet.API.Settings;
using BalancerKube.Wallet.API.Services;
using BalancerKube.Wallet.API.Consumers;
using BalancerKube.Wallets.API.Persistence;
using BalancerKube.Wallet.API.Services.Base;
using BalancerKube.Wallet.API.Models.Request;
using BalanceKube.Contracts;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog();
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("WalletDb")));

builder.Services.AddScoped<IWalletService, WalletService>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var rabbitMqSettings = new RabbitMQSettings();
builder.Configuration.GetSection("RabbitMQSettings").Bind(rabbitMqSettings);

builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<CreateTransactionConsumer>();

    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.UseScheduledRedelivery(r =>
            r.Intervals(
                TimeSpan.FromMinutes(1),
                TimeSpan.FromMinutes(5),
                TimeSpan.FromMinutes(10)));

        cfg.Host(rabbitMqSettings?.Host, "/", h =>
        {
            h.Username(rabbitMqSettings?.Username);
            h.Password(rabbitMqSettings?.Password);
        });

        cfg.ReceiveEndpoint($"{rabbitMqSettings?.EndpointName}-create-transaction", e =>
        {
            e.UseMessageRetry(r => r.Interval(3, TimeSpan.FromSeconds(10)));
            e.Consumer<CreateTransactionConsumer>(context);
            e.Bind(ContractNames.CreateTransaction);
        });
    });
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

// Map minimal API endpoint for creating a transaction
app.MapPost("/api/transaction", async (CreateTransactionRequest request, IWalletService walletService) =>
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

    var user = BalancerKube.Domain.Entities.User.Create(
        request.Username,
        request.City,
        request.Address);

    applicationDb.Users.Add(user);
    await applicationDb.SaveChangesAsync();

    await publishEndpoint.Publish(new RegisteredNewUserDto(
        user.Id,
        user.Username));

    return Results.Ok(user.Id);
});

try
{
    // Kubernetes sets the Pod name in the HOSTNAME environment variable
    Log.Logger = new LoggerConfiguration()
        .Enrich.FromLogContext()
        .Enrich.WithProperty("ServiceName", "WalletService")
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