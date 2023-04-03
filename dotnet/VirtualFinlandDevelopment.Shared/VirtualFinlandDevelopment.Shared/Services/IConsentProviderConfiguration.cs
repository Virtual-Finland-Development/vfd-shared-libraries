using Microsoft.IdentityModel.Tokens;

namespace VirtualFinlandDevelopment.Shared.Services;

public interface IConsentProviderConfiguration
{
    public string Issuer { get; }
    public string ConsentVerifyUrl { get; }

    public Task LoadPublicKeys();
    public JsonWebKey? GetKey(string kid);
}
