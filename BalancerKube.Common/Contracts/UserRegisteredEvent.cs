namespace BalancerKube.Common.Contracts;

public sealed record UserRegisteredEvent(int UserId, string Username);