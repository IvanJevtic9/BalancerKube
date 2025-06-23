namespace BalanceKube.EventGenerator.API.Settings;

public class MongoDbSettings
{
    public string Name { get; init; } = null!;
    public string Host { get; init; } = null!;
    public int Port { get; init; }
    public string? Username { get; init; }
    public string? Password { get; init; }

    public string ConnectionString => string.IsNullOrEmpty(Username) && string.IsNullOrEmpty(Password)
        ? $"mongodb://{Host}:{Port}"
        : $"mongodb://{Username}:{Password}@{Host}:{Port}";
}
