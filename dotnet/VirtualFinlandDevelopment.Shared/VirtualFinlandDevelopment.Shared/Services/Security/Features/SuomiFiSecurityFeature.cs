using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using NetDevPack.Security.JwtExtensions;

namespace VirtualFinlandDevelopment.Shared.Services.Security.Features;

public class SuomiFiSecurityFeature : ISecurityFeature
{
    private const string SecurityPolicySchemeName = "SuomiFiBearerScheme";

    public SuomiFiSecurityFeature(IOptions<SuomiFiSecurityFeatureOptions> config)
    {
        JwksOptionsUrl = config.Value.AuthorizationJwksJsonUrl;
        Issuer = config.Value.Issuer;
    }

    public SuomiFiSecurityFeature(IConfiguration configuration)
    {
        JwksOptionsUrl = configuration["SuomiFi:AuthorizationJwksJsonUrl"];
        Issuer = configuration["SuomiFi:Issuer"];
    }

    public string? JwksOptionsUrl { get; }
    public string? Issuer { get; }

    public void BuildAuthentication(AuthenticationBuilder authenticationBuilder)
    {
        authenticationBuilder.AddJwtBearer(SecurityPolicySchemeName, c =>
        {
            c.RequireHttpsMetadata =
                Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") !=
                "local"; // @TODO: Use EnvironmentExtensions
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
}
