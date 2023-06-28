using System.IdentityModel.Tokens.Jwt;
using VirtualFinlandDevelopment.Shared.Exceptions;
using VirtualFinlandDevelopment.Shared.Services.Security.Features;

namespace VirtualFinlandDevelopment.Shared.Services.Security;

public interface IApplicationSecurity
{
}

public class ApplicationSecurity : IApplicationSecurity
{
    private readonly List<ISecurityFeature> _features;

    public ApplicationSecurity(List<ISecurityFeature> features)
    {
        _features = features;
    }

    /// <summary>
    ///     Parses the JWT token and returns the issuer and the user id
    /// </summary>
    public JwtTokenResult ParseJwtToken(string token)
    {
        if (string.IsNullOrEmpty(token)) throw new NotAuthorizedException("No token provided");

        var tokenHandler = new JwtSecurityTokenHandler();
        if (!tokenHandler.CanReadToken(token)) throw new NotAuthorizedException("The given token is not valid");
        var parsedToken = tokenHandler.ReadJwtToken(token);

        // Resolve the security feature by token issuer (must be enabled) // @TODO: ensure the security feature is loaded before this
        var tokenIssuer = parsedToken.Issuer;
        var securityFeature = _features.Find(o => o.Issuer == tokenIssuer);
        if (securityFeature == null) throw new NotAuthorizedException("The given token issuer is not valid");

        // Resolve user id
        var userId = securityFeature.ResolveTokenUserId(parsedToken);
        if (userId == null) throw new NotAuthorizedException("The given token claim is not valid");

        return new JwtTokenResult { UserId = userId, Issuer = securityFeature.Issuer };
    }
}

public class JwtTokenResult
{
    public string? UserId { get; set; }
    public string? Issuer { get; set; }
}
