namespace VirtualFinlandDevelopment.Shared.Services.Security.Features;

public class SuomiFiSecurityFeatureOptions : ISecurityFeatureOptions
{
    public string AuthorizationJwksJsonUrl { get; set; } = null!;
    public string Issuer { get; set; } = null!;
    public bool IsEnabled { get; set; }
}
