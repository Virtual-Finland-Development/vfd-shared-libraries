using System.IdentityModel.Tokens.Jwt;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;
using VirtualFinlandDevelopment.Shared.Exceptions;

namespace VirtualFinlandDevelopment.Shared.Services.Security.Features;

public static class SecurityFeatureServiceExtensions
{
    private const string ResolvePolicyFromTokenIssuer = "ResolvePolicyFromTokenIssuer"; // TODO: Add constant?


    public static IServiceCollection RegisterTestbedSecurityFeature(this IServiceCollection services)
    {
        // TODO: Add default configuration to services
        services.AddSingleton<IApplicationSecurity, ApplicationSecurity>();

        // TODO: Add testbed security features to services

        // TODO: Add testbed security to list of security features in ApplicationSecurity.cs

        return services;
    }


    /// <summary>
    ///     This is maybe a bit useless way of configuring this whole thing
    /// </summary>
    /// <param name="services"></param>
    /// <param name="testbedOptions"></param>
    /// <param name="sinunaOptions"></param>
    /// <param name="suomiFiOptions"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    public static IServiceCollection RegisterSecurityFeatures(this IServiceCollection services,
        Action<TestbedSecurityFeatureOptions>? testbedOptions,
        Action<ISecurityFeatureOptions>? sinunaOptions,
        Action<ISecurityFeatureOptions>? suomiFiOptions)
    {
        // If options is nut null, register services and add service to list of security services
        if (testbedOptions != null)
        {
            services.Configure(testbedOptions);
            // TODO: We should not register these if we don't want to use them
            services.AddSingleton<ISecurityFeature, TestbedSecurityFeature>();
        }

        var features = new List<ISecurityFeature>();

        using var provider = services.BuildServiceProvider();
        using var scope = provider.CreateScope();

        var testbedFeatureOptions = scope.ServiceProvider.GetService<IOptions<TestbedSecurityFeatureOptions>>();
        if (testbedFeatureOptions != null && testbedFeatureOptions.Value.IsEnabled)
            features.Add(scope.ServiceProvider.GetService<TestbedSecurityFeature>() ??
                         throw new InvalidOperationException(
                             $"{nameof(TestbedSecurityFeature)} service was not registered correctly"));


        return services;
    }

    /// <summary>
    ///     Configure security features using appsettings.json
    /// </summary>
    /// <param name="services"></param>
    /// <param name="configuration"></param>
    /// <returns></returns>
    public static IServiceCollection RegisterSecurityFeatures(this IServiceCollection services,
        IConfiguration configuration)
    {
        var features = new List<ISecurityFeature>();

        if (IsSecurityFeatureEnabled(configuration, "Testbed")) features.Add(new TestbedSecurityFeature(configuration));
        if (IsSecurityFeatureEnabled(configuration, "Sinuna")) features.Add(new TestbedSecurityFeature(configuration));
        if (IsSecurityFeatureEnabled(configuration, "SuomiFI")) features.Add(new TestbedSecurityFeature(configuration));

        services.AddSingleton<IApplicationSecurity>(new ApplicationSecurity(features));

        var authenticationBuilder = services.AddAuthentication(options =>
        {
            options.DefaultScheme = ResolvePolicyFromTokenIssuer;
            options.DefaultChallengeScheme = ResolvePolicyFromTokenIssuer;
        });

        foreach (var securityFeature in features) securityFeature.BuildAuthentication(authenticationBuilder);

        services.AddAuthorization(options =>
        {
            foreach (var securityFeature in features) securityFeature.BuildAuthorization(options);
        });

        authenticationBuilder.AddPolicyScheme(ResolvePolicyFromTokenIssuer, ResolvePolicyFromTokenIssuer,
            options =>
            {
                options.ForwardDefaultSelector =
                    context => GetSecurityPolicySchemeName(context.Request.Headers, features);
            });

        return services;
    }

    private static string GetSecurityPolicySchemeName(IHeaderDictionary headers, IEnumerable<ISecurityFeature> features)
    {
        var authorizationHeader = headers[HeaderNames.Authorization].FirstOrDefault();

        if (string.IsNullOrEmpty(authorizationHeader) || !authorizationHeader.StartsWith("Bearer "))
            throw new NotAuthorizedException("Invalid token provided");

        var token = authorizationHeader["Bearer ".Length..].Trim();

        var jwtHandler = new JwtSecurityTokenHandler();
        if (!jwtHandler.CanReadToken(token))
            throw new NotAuthorizedException("Invalid token provided");

        var issuer = jwtHandler.ReadJwtToken(token).Issuer;

        var feature = features.SingleOrDefault(securityFeature => securityFeature.Issuer == issuer);

        if (feature is null)
            throw new NotAuthorizedException("Invalid token provided");

        return feature.GetSecurityPolicySchemeName();
    }

    private static bool IsSecurityFeatureEnabled(IConfiguration config, string featureName)
    {
        return config.GetValue<bool>($"{featureName}:IsEnabled");
    }

    private static void ConfigureSecurityFeatureOptions(this IServiceCollection services,
        Action<ISecurityFeatureOptions>? setupAction)
    {
        services.Configure(setupAction);
    }
}
