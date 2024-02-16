namespace BalancerKube.Wallet.API.Models.Request
{
    public record CreateUserRequest(string Username, string? City = null, string? Address = null);
}
