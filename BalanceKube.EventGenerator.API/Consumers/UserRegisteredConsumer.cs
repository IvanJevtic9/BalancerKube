using MassTransit;
using BalanceKube.EventGenerator.API.Entities;
using BalancerKube.Common.Contracts;
using BalanceKube.EventGenerator.API.Abstraction;

namespace BalanceKube.EventGenerator.API.Consumers;

public class UserRegisteredConsumer : IConsumer<UserRegisteredEvent>
{
    private readonly IRepository<User> _userRepository;
    private readonly ILogger<UserRegisteredConsumer> _logger;

    public UserRegisteredConsumer(
        IRepository<User> userRepository,
        ILogger<UserRegisteredConsumer> logger)
    {
        _logger = logger;
        _userRepository = userRepository;
    }

    public async Task Consume(ConsumeContext<UserRegisteredEvent> context)
    {
        var message = context.Message;

        if (message is null)
        {
            _logger.LogWarning("Consumed message is null.");
            return;
        }

        var user = new User(message.UserId, message.Username);

        user.UpdateUsername(message.Username);

        await _userRepository.CreateAsync(user);

        _logger.LogInformation($"New user has been added, ID: {user.Id}, Username: {user.Username}.");
    }
}
