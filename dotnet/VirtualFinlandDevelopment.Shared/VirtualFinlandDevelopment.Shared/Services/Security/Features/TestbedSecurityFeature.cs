using System.IdentityModel.Tokens.Jwt;
using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using NetDevPack.Security.JwtExtensions;

namespace VirtualFinlandDevelopment.Shared.Services.Security.Features;

public class TestbedSecurityFeature : ISecurityFeature
{
    private const string SecurityPolicySchemeName = "TestBedBearerScheme";

    private const int ConfigUrlMaxRetryCount = 5;
    private const int ConfigUrlRetryWaitTime = 3000;
    private readonly string? _openIdConfigurationUrl;

    public TestbedSecurityFeature(IOptions<TestbedSecurityFeatureOptions> config)
    {
        _openIdConfigurationUrl = config.Value.OpenIdConfigurationUrl;
    }

    public TestbedSecurityFeature(IConfiguration configuration)
    {
        _openIdConfigurationUrl = configuration["Testbed:OpenIDConfigurationURL"];
    }

    public string? JwksOptionsUrl { get; private set; }
    public string? Issuer { get; private set; }

    public void BuildAuthentication(AuthenticationBuilder authenticationBuilder)
    {
        LoadOpenIdConfigUrl();

        authenticationBuilder.AddJwtBearer(SecurityPolicySchemeName, c =>
        {
            c.SetJwksOptions(new JwkOptions(JwksOptionsUrl));

            c.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateActor = false,
                ValidateAudience = false,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = Issuer
            };
        });
    }

    public void BuildAuthorization(AuthorizationOptions options)
    {
        options.AddPolicy(SecurityPolicySchemeName, policy =>
        {
            policy.AuthenticationSchemes.Add(SecurityPolicySchemeName);
            policy.RequireAuthenticatedUser();
        });
    }

    public string GetSecurityPolicySchemeName()
    {
        return SecurityPolicySchemeName;
    }

    public string? ResolveTokenUserId(JwtSecurityToken jwtSecurityToken)
    {
        return jwtSecurityToken.Subject; // the "sub" claim
    }

    private async void LoadOpenIdConfigUrl()
    {
        var httpClient = new HttpClient();
        var httpResponse = await httpClient.GetAsync(_openIdConfigurationUrl);

        for (var retryCount = 0; retryCount < ConfigUrlMaxRetryCount; retryCount++)
        {
            if (httpResponse.IsSuccessStatusCode)
            {
                var jsonData = JsonNode.Parse(await httpResponse.Content.ReadAsStringAsync());
                Issuer = jsonData?["issuer"]?.ToString();
                JwksOptionsUrl = jsonData?["jwks_uri"]?.ToString();

                if (!string.IsNullOrEmpty(Issuer) && !string.IsNullOrEmpty(JwksOptionsUrl)) break;
            }

            await Task.Delay(ConfigUrlRetryWaitTime);
        }

        // If all retries fail, then send an exception since the security information is critical to the functionality of the backend
        if (string.IsNullOrEmpty(Issuer) || string.IsNullOrEmpty(JwksOptionsUrl))
            throw new InvalidOperationException("Failed to retrieve TestBed OpenID configurations.");
    }
}
