namespace VirtualFinlandDevelopment.Shared.Configuration;

public class ConsentProviderOptions
{
    public string Issuer { get; set; } = null!;
    public string ConsentVerifyUrl { get; set; } = null!;
    public string ConsentJwksJsonUrl { get; set; } = null!;
}
