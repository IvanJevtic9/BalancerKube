using Microsoft.EntityFrameworkCore;
using BalancerKube.Wallets.API.Persistence;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("WalletDb")));

var app = builder.Build();

app.MapGet("/", () => "Hello World!");

app.Run();
