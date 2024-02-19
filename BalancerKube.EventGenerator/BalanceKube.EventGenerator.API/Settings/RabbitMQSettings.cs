namespace BalanceKube.EventGenerator.API.Settings
{
    public class RabbitMQSettings
    {
        public string Host { get; set; } = null!;
        public string Username { get; set; } = null!;
        public string Password { get; set; } = null!;
        public string EndpointName { get; set; } = null!;
    }
}
