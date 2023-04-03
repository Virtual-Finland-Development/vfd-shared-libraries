using Microsoft.Extensions.Hosting;

namespace VirtualFinlandDevelopment.Shared.Environments;

public static class Environments
{
    public const string Local = "local";
    public const string Development = "dev";
    public const string Staging = "staging";
    public const string Production = "prod";
}

public static class EnvironmentExtensions
{
    public static bool IsLocal(this IHostEnvironment hostEnvironment)
    {
        if (hostEnvironment == null)
        {
            throw new ArgumentNullException(nameof(hostEnvironment));
        }
        
        
        return hostEnvironment.IsEnvironment(Environments.Local);
    }

    public static bool IsDevelopment(this IHostEnvironment hostEnvironment)
    {
        if (hostEnvironment == null)
        {
            throw new ArgumentNullException(nameof(hostEnvironment));
        }

        return hostEnvironment.IsEnvironment(Environments.Development);
    }

    public static bool IsStaging(this IHostEnvironment hostEnvironment)
    {
        if (hostEnvironment == null)
        {
            throw new ArgumentNullException(nameof(hostEnvironment));
        }

        return hostEnvironment.IsEnvironment(Environments.Staging);
    }

    public static bool IsProduction(this IHostEnvironment hostEnvironment)
    {
        if (hostEnvironment == null)
        {
            throw new ArgumentNullException(nameof(hostEnvironment));
        }

        return hostEnvironment.IsEnvironment(Environments.Production);
    }
}
