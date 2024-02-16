using Polly;
using LanguageExt;
using Microsoft.EntityFrameworkCore;
using BalancerKube.Wallet.API.Services;
using BalancerKube.Wallets.API.Persistence;
using BalancerKube.Wallet.API.Services.Base;
using BalancerKube.Wallet.API.Models.Request;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("WalletDb")));

builder.Services.AddScoped<IWalletService, WalletService>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

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
                TimeSpan.FromSeconds(10),
                TimeSpan.FromSeconds(30),
                TimeSpan.FromSeconds(60)
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

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Map minimal API endpoint for creating a transaction
app.MapPost("/api/transaction", async (CreateTransactionRequest request, IWalletService walletService) =>
{
    var result = await walletService.ProcessTransactionAsync(request);

    if (result.IsFaulted)
    {
        var exceptionMessage = result.Match(
            Succ: res => string.Empty,
            Fail: err => err.Message);

        return Results.BadRequest(exceptionMessage);
    }

    var transactionId = result.Match(
        Succ: res => res,
        Fail: err => Guid.Empty);

    return Results.Ok(transactionId);
});

app.MapPost("/api/user", async (CreateUserRequest request, ApplicationDbContext applicationDb) =>
{
    if (string.IsNullOrWhiteSpace(request.Username))
    {
        return Results.BadRequest($"{nameof(request.Username)} is a required field.");
    }

    var user = BalancerKube.Domain.Entities.User.Create(request.Username, request.City, request.Address);

    applicationDb.Users.Add(user);
    await applicationDb.SaveChangesAsync();

    return Results.Ok(user.Id);
});

app.Run();