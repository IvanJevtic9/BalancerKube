namespace BalancerKube.Wallet.Domain.Common
{
    public record struct Currency(string Symbol)
    {
        public Currency() : this(string.Empty) { }
        public static Currency Empty => new(string.Empty);
        public static readonly Currency EUR = new("EUR");
        public static readonly Currency USD = new("USD");

        public Money Amount(decimal amount) => new(amount, this);
        public override string ToString() => Symbol;
    }
}
