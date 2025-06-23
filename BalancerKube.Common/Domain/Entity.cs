namespace BalancerKube.Common.Domain;

public abstract class Entity<TKey> : IEquatable<Entity<TKey>>
    where TKey : struct
{
    public TKey Id { get; private init; }

    protected Entity(TKey id) => Id = id;

    public bool Equals(Entity<TKey>? other)
    {
        return Id.Equals(other?.Id);
    }

    public override bool Equals(object? obj)
    {
        return obj is not null
            && obj.GetType() == GetType()
            && obj is Entity<TKey> entity
            && Id.Equals(entity.Id);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Id);
    }
}
