namespace BalancerKube.Wallet.API.Exceptions;

public sealed class ConcurrencyException : Exception
{
    public ConcurrencyException(string message) : base(message)
    { }
}
