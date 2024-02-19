using BalanceKube.EventGenerator.API.Entities.Base;

namespace BalanceKube.EventGenerator.API.Entities
{
    public class User : IEntity
    {
        public Guid Id { get; init; }
        public int UserId { get; init; }
        public string Username { get; init; } = null!;
    }
}
