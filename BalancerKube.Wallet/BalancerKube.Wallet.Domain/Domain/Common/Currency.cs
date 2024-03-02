namespace BalancerKube.Wallet.Domain.Common
{
    public record struct Currency
    {
        private static IReadOnlySet<string> _currencies = new HashSet<string>()
        {
            string.Empty,
            "EUR",
            "USD",
            "DIN"
        };

        public Currency(string symbol)
        {
            if(!VerifyCurrency(symbol)) 
            {
                throw new ApplicationException("Unsupported currency.");
            }

            Symbol = symbol;
        }

        public string Symbol { get; init; }

        public static Currency Empty => new(string.Empty);
        public static readonly Currency EUR = new("EUR");
        public static readonly Currency USD = new("USD");
        public static readonly Currency DIN = new("DIN");

        public static bool VerifyCurrency(string symbol) => _currencies.Contains(symbol);
        public Money Amount(decimal amount) => new(amount, this);
        public override string ToString() => Symbol;
    }
}
