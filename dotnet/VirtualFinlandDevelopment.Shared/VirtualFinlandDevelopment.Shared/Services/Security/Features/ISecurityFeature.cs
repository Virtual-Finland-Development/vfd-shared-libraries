using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;

namespace VirtualFinlandDevelopment.Shared.Services.Security.Features;

public interface ISecurityFeature
{
    public string? JwksOptionsUrl { get; }
    public string? Issuer { get; }
    public void BuildAuthentication(AuthenticationBuilder authenticationBuilder);
    public void BuildAuthorization(AuthorizationOptions options);
    public string GetSecurityPolicySchemeName();
    public string? ResolveTokenUserId(JwtSecurityToken jwtSecurityToken);
}
