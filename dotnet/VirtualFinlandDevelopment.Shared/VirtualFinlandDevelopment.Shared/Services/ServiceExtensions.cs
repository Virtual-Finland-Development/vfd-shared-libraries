using Microsoft.Extensions.DependencyInjection;
using VirtualFinlandDevelopment.Shared.Attributes;
using VirtualFinlandDevelopment.Shared.Configuration;

namespace VirtualFinlandDevelopment.Shared.Services;

public static class ServiceExtensions
{
    public static IServiceCollection RegisterTestbedConsentProvider(this IServiceCollection services,
        Action<ConsentProviderOptions>? options, Action<ConsentPortalOptions>? portalOptions)
    {
        services.Configure(options);
        services.Configure(portalOptions);

        services.AddSingleton<IConsentProviderConfiguration, TestbedConsentProviderConfiguration>();
        services.AddSingleton<TestbedConsentSecurityService>();

        services.AddHostedService<TestbedConsentProviderInitializer>();

        return services;
    }
}
