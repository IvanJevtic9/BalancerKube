using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace BalancerKube.Wallet.API.Utilities;

public static class RunTimeDiagnosticConfig
{
    public const string ServiceName = "wallet-service";

    public static string ServiceVersion = typeof(Program).Assembly.GetName().Version?.ToString() ?? "1.0.0";

    public static string ServiceEnvironment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";

    public static string ServiceInstanceId = Environment.GetEnvironmentVariable("HOSTNAME") ?? "unknown-instance";

    public static ActivitySource Source = new(ServiceName);

    public static Meter Meter = new(ServiceName, ServiceVersion);
}
