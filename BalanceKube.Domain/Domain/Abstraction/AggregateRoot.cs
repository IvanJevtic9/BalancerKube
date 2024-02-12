namespace BalancerKube.Wallet.Domain.Abstraction
{
    public abstract class AggregateRoot<TKey> : Entity<TKey>
        where TKey : struct
    {
        private readonly List<IDomainEvent> _domainEvents;

        protected AggregateRoot(TKey id) : base(id) { }

        public void ClearDomainEvents() => _domainEvents.Clear();
        protected void RaiseDomainEvent(IDomainEvent domainEvent) => _domainEvents.Add(domainEvent);
        public IReadOnlyCollection<IDomainEvent> GetDomainEvents() => _domainEvents;
    }
}
