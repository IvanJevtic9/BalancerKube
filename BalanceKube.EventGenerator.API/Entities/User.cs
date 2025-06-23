using BalancerKube.Common.Domain;

namespace BalanceKube.EventGenerator.API.Entities;

public sealed class User : Entity<Guid>
{
    public User(int userId, string username) : base(Guid.NewGuid())
    {
        UserId = userId;
        Username = username;
    }

    public int UserId { get; private init; }

    public string Username { get; private set; }

    public void UpdateUsername(string username) => Username = username;
}