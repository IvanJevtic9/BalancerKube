namespace BalanceKube.Contracts
{
    public sealed record RegisteredNewUserDto(
        int UserId,
        string Username);
}
