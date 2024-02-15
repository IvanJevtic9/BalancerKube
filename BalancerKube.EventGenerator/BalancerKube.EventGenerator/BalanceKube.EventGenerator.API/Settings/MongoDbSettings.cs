namespace BalanceKube.EventGenerator.API.Settings
{
    public class MongoDbSettings
    {
        public string Name { get; set; } = null!;
        public string Host { get; set; } = null!;
        public int Port { get; set; }
        public string ConnectionString => $"mongodb://{Host}:{Port}";
    }
}
