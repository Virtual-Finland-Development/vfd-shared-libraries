using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace VirtualFinlandDevelopment.Shared.Services;

public class TestbedConsentProviderInitializer : IHostedService
{
    private readonly IServiceProvider _serviceProvider;

    public TestbedConsentProviderInitializer(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var consentProvider = scope.ServiceProvider.GetRequiredService<IConsentProviderConfiguration>();
        await consentProvider.LoadPublicKeys(); // To await or not to await?
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
