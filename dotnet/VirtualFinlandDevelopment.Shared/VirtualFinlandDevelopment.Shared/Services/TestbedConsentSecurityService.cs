using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.IdentityModel.Tokens;
using VirtualFinlandDevelopment.Shared.Exceptions;

namespace VirtualFinlandDevelopment.Shared.Services;

public class TestbedConsentSecurityService
{
    private const string XConsentToken = "X-Consent-Token";
    private readonly IConsentProviderConfiguration _configuration;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<TestbedConsentSecurityService> _logger;

    public TestbedConsentSecurityService(ILogger<TestbedConsentSecurityService> logger,
        IConsentProviderConfiguration configuration,
        IHttpClientFactory httpClientFactory)
    {
        _logger = logger;
        _configuration = configuration;
        _httpClientFactory = httpClientFactory;
    }

    public async Task VerifyConsentTokenRequestHeaders(IHeaderDictionary headers, string dataSourceUri)
    {
        _logger.LogInformation("Verifying consent request");
        var idToken = headers.Authorization.ToString().Replace("Bearer ", string.Empty);
        if (string.IsNullOrEmpty(idToken))
            throw new NotAuthorizedException("Authorization token is missing");

        var consentToken = headers[XConsentToken].ToString();
        if (string.IsNullOrEmpty(consentToken))
            throw new NotAuthorizedException("Consent token is missing");

        VerifyConsentToken(consentToken, idToken, dataSourceUri);
        await VerifyConsentTokenWithService(consentToken, idToken, dataSourceUri); // Checks if token has been revoked

        _logger.LogInformation("Consent request verified");
    }

    private void VerifyConsentToken(string consentTokenRaw, string idTokenRaw, string dataSourceUri)
    {
        _logger.LogInformation("Verifying consent token");

        var idToken = ParseJwtToken(idTokenRaw);
        if (idToken == null)
            throw new NotAuthorizedException("Invalid authorization token");

        var consentToken = ParseJwtToken(consentTokenRaw);
        if (consentToken == null)
            throw new NotAuthorizedException("Invalid consent token");

        var issuerPublicKey = _configuration.GetKey(consentToken.Header.Kid);
        if (issuerPublicKey == null)
            throw new NotAuthorizedException("Consent token signing key not found");

        var validationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = issuerPublicKey,
            ValidateIssuer = true,
            ValidIssuer = _configuration.Issuer,
            ValidateLifetime = true,
            ValidateAudience = false
        };

        var handler = new JwtSecurityTokenHandler();

        handler.ValidateToken(consentTokenRaw, validationParameters, out var validToken);
        if (validToken is not JwtSecurityToken validJwt) throw new NotAuthorizedException("Invalid Consent JWT Token");

        if (!validJwt.Header.Alg.Equals(SecurityAlgorithms.RsaSha256, StringComparison.Ordinal))
            throw new NotAuthorizedException("Consent token algorithm must be RS256");

        // Check rest of the consent claims
        if (consentToken.Payload.Acr != idToken.Payload.Acr)
            throw new NotAuthorizedException("Token mismatch: acr");
        if (consentToken.Payload["appiss"] as string != idToken.Payload.Iss)
            throw new NotAuthorizedException("Token mismatch: appiss");
        if (consentToken.Payload["app"] as string != idToken.Payload["aud"] as string)
            throw new NotAuthorizedException("Token mismatch: app");
        if (consentToken.Header["v"] as string != "0.2")
            throw new NotAuthorizedException("Token mismatch: v");
        if (consentToken.Payload["subiss"] as string != idToken.Payload.Iss)
            throw new NotAuthorizedException("Token mismatch: subiss");
        if (consentToken.Payload.Sub != idToken.Payload.Sub)
            throw new NotAuthorizedException("Token mismatch: sub");
        if (consentToken.Payload["dsi"] as string != dataSourceUri)
            throw new NotAuthorizedException("Token mismatch: dsi");

        _logger.LogInformation("Consent token verified");
    }


    private async Task VerifyConsentTokenWithService(string consentToken, string idToken, string dataSourceUri)
    {
        _logger.LogInformation("Verifying consent with Testbed Consent API");
        try
        {
            var httpClient = _httpClientFactory.CreateClient();
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", idToken);
            httpClient.DefaultRequestHeaders.Add(XConsentToken, consentToken);

            var payload = JsonSerializer.Serialize(new
            {
                dataSource = dataSourceUri
            });

            var httpContent = new StringContent(payload, Encoding.UTF8, "application/json");
            using var response = await httpClient.PostAsync(_configuration.ConsentVerifyUrl, httpContent);
            response.EnsureSuccessStatusCode();

            var responseData =
                await JsonSerializer.DeserializeAsync<ConsentVerifyResponse>(
                    await response.Content.ReadAsStreamAsync()) ??
                throw new NotAuthorizedException("Bad verify-response from Testbed Consent API");

            if (!responseData.Verified)
                throw new NotAuthorizedException("Not verified by Testbed Consent API");
        }
        catch (HttpRequestException e)
        {
            _logger.LogWarning("Testbed Consent API could not verify token");
            throw new NotAuthorizedException(e.Message);
        }

        _logger.LogInformation("Consent verified by Testbed Consent API");
    }


    private static JwtSecurityToken? ParseJwtToken(string token)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var canReadToken = tokenHandler.CanReadToken(token);
        return canReadToken ? tokenHandler.ReadJwtToken(token) : null;
    }

    private sealed class ConsentVerifyResponse
    {
        [JsonPropertyName("verified")]
        public bool Verified { get; set; }
    }
}
