namespace BalanceKube.EventGenerator.API.Settings
{
    public class MongoDbSettings
    {
        public string Name { get; set; } = null!;
        public string Host { get; set; } = null!;
        public int Port { get; set; }
        public string? Username { get; set; }
        public string? Password { get; set; }

        public string ConnectionString => string.IsNullOrEmpty(Username) && string.IsNullOrEmpty(Password) ?
            $"mongodb://{Host}:{Port}" :
            $"mongodb://{Username}:{Password}@{Host}:{Port}";
    }
}
