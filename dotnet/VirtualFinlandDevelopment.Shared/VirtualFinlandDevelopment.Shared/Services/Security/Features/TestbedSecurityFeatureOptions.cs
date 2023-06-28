namespace VirtualFinlandDevelopment.Shared.Services.Security.Features;

public class TestbedSecurityFeatureOptions : ISecurityFeatureOptions
{
    public string OpenIdConfigurationUrl { get; set; } = null!;
    public bool IsEnabled { get; set; }
}
