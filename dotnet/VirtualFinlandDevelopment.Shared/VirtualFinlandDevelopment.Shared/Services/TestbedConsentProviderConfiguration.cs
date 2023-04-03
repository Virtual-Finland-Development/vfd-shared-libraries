using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using VirtualFinlandDevelopment.Shared.Configuration;

namespace VirtualFinlandDevelopment.Shared.Services;

internal class TestbedConsentProviderConfiguration : IConsentProviderConfiguration
{
    private const int ConfigUrlMaxRetryCount = 5;
    private const int ConfigUrlRetryWaitTime = 3000;
    private readonly string _jwksJsonUrl;
    private readonly ILogger<TestbedConsentProviderConfiguration> _logger;
    private List<JsonWebKey> _keys = new();

    public TestbedConsentProviderConfiguration(IOptions<ConsentProviderOptions> settings,
        ILogger<TestbedConsentProviderConfiguration> logger)
    {
        _logger = logger;
        _jwksJsonUrl = settings.Value.ConsentJwksJsonUrl;
        Issuer = settings.Value.Issuer;
        ConsentVerifyUrl = settings.Value.ConsentVerifyUrl;
    }

    public string Issuer { get; }
    public string ConsentVerifyUrl { get; }

    public async Task LoadPublicKeys()
    {
        _logger.LogDebug(
            "Trying to load public keys with jwks: {Jwks}, issuer: {Issuer} and consent verify url: {ConsentVerifyUrl}",
            _jwksJsonUrl, Issuer, ConsentVerifyUrl);
        
        Console.WriteLine("höh :)");
        
        var httpClient = new HttpClient();
        
        _logger.LogDebug("Sending request to base address: {Url}", httpClient.BaseAddress);
        
        var httpResponse = await httpClient.GetAsync(_jwksJsonUrl);

        for (var retryCount = 0; retryCount < ConfigUrlMaxRetryCount; retryCount++)
        {
            if (httpResponse.IsSuccessStatusCode)
            {
                var response = JsonSerializer
                    .Deserialize<JwksJsonResponse>(await httpResponse.Content.ReadAsStringAsync());

                if (response?.Keys == null)
                    throw new InvalidOperationException("Failed to retrieve Testbed consent public key configurations");

                _keys = response.Keys.Select(k => new JsonWebKey
                {
                    Kty = k.Kty,
                    Use = k.Use,
                    Kid = k.Kid,
                    N = k.N,
                    E = k.E
                }).ToList();
            }

            await Task.Delay(ConfigUrlRetryWaitTime);
        }
    }

    public JsonWebKey? GetKey(string kid)
    {
        return _keys.FirstOrDefault(k => k.Kid == kid);
    }

    private sealed class JwksJsonResponse
    {
        [JsonPropertyName("keys")]
        public List<Jwk>? Keys { get; set; }
    }

    private sealed class Jwk
    {
        [JsonPropertyName("kty")]
        public string? Kty { get; }

        [JsonPropertyName("use")]
        public string? Use { get; }

        [JsonPropertyName("kid")]
        public string? Kid { get; }

        [JsonPropertyName("n")]
        public string? N { get; }

        [JsonPropertyName("e")]
        public string? E { get; }
    }
}

