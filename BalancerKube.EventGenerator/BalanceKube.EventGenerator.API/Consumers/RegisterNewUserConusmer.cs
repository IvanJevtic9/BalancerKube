﻿using MassTransit;
using BalanceKube.Contracts;
using BalanceKube.EventGenerator.API.Entities;
using BalanceKube.EventGenerator.API.Persistence.Base;

namespace BalanceKube.EventGenerator.API.Consumers
{
    public class RegisterNewUserConusmer : IConsumer<RegisteredNewUserDto>
    {
        private readonly IRepository<User> _userRepository;
        private readonly ILogger<RegisterNewUserConusmer> _logger;

        public RegisterNewUserConusmer(
            IRepository<User> userRepository,
            ILogger<RegisterNewUserConusmer> logger)
        {
            _logger = logger;
            _userRepository = userRepository;
        }

        public async Task Consume(ConsumeContext<RegisteredNewUserDto> context)
        {
            var message = context.Message;

            if (message is null)
            {
                _logger.LogWarning("Consumed message is null.");
                return;
            }

            var user = new User()
            {
                Id = Guid.NewGuid(),
                UserId = message.UserId,
                Username = message.Username
            };

            await _userRepository.CreateAsync(user);

            _logger.LogInformation($"New user has been added, ID: {user.Id}, Username: {user.Username}.");
        }
    }
}
