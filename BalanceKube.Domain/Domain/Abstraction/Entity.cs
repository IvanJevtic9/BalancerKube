namespace BalancerKube.Wallet.Domain.Abstraction
{
    public abstract class Entity<TKey> : IEquatable<Entity<TKey>>
        where TKey : struct
    {
        public TKey Id { get; private init; }

        protected Entity(TKey id) => Id = id;

        public bool Equals(Entity<TKey>? other)
        {
            return Equals(other as object);
        }

        public override bool Equals(object obj)
        {
            if (obj is null) return false;
            if (obj.GetType() != GetType()) return false;
            if (obj is not Entity<TKey> entity) return false;

            return Id.Equals(entity.Id);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Id);
        }
    }
}
