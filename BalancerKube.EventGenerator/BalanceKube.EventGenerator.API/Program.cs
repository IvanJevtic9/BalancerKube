using MassTransit;
using System.Reflection;
using BalanceKube.EventGenerator.API.Entities;
using BalanceKube.EventGenerator.API.Persistence;
using BalanceKube.EventGenerator.API.Services;
using BalanceKube.EventGenerator.API.Settings;
using BalanceKube.EventGenerator.API.HostedService;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

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
    x.AddConsumers(Assembly.GetEntryAssembly());

    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host(rabbitMqSettings?.Host, "/", h =>
        {
            h.Username(rabbitMqSettings?.Username);
            h.Password(rabbitMqSettings?.Password);
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

app.UseHttpsRedirection();

app.MapControllers();

app.Run();
