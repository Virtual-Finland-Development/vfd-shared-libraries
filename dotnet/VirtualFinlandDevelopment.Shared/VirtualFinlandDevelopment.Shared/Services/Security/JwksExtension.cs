using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using NetDevPack.Security.JwtExtensions;

namespace VirtualFinlandDevelopment.Shared.Services.Security;

public static class JwksExtension
{
    private const string ServerUserAgent = "UsersApi/1.0.0"; //TODO: Add constant for this

    public static void SetJwksOptions(this JwtBearerOptions options, JwkOptions jwkOptions)
    {
        var httpClient = new HttpClient(options.BackchannelHttpHandler ?? (HttpMessageHandler)new HttpClientHandler())
        {
            Timeout = options.BackchannelTimeout,
            MaxResponseContentBufferSize = 10485760
        };
        httpClient.DefaultRequestHeaders.UserAgent.ParseAdd(ServerUserAgent);

        options.ConfigurationManager = new ConfigurationManager<OpenIdConnectConfiguration>(jwkOptions.JwksUri,
            new JwksRetriever(), new HttpDocumentRetriever(httpClient)
            {
                RequireHttps = options.RequireHttpsMetadata
            });
        options.TokenValidationParameters.ValidateAudience = false;
        options.TokenValidationParameters.ValidIssuer = jwkOptions.Issuer;
        if (string.IsNullOrEmpty(jwkOptions.Audience))
            return;
        options.TokenValidationParameters.ValidateAudience = true;
        options.TokenValidationParameters.ValidAudience = jwkOptions.Audience;
    }
}
