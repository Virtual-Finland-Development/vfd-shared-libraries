using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Options;
using VirtualFinlandDevelopment.Shared.Services;

namespace VirtualFinlandDevelopment.Shared.Attributes;

public class VerifyConsentAuthorizationAttribute : TypeFilterAttribute
{
    public VerifyConsentAuthorizationAttribute(string dataSource) : base(typeof(VerifyConsentAuthorizationFilter))
    {
        Arguments = new object[] { dataSource };
    }
}

public class VerifyConsentAuthorizationFilter : IAuthorizationFilter
{
    private readonly TestbedConsentSecurityService _consentSecurityService;
    private readonly string? _dataProduct;
    private readonly IOptions<ConsentPortalOptions>? _options;

    public VerifyConsentAuthorizationFilter(string? dataProduct, TestbedConsentSecurityService consentSecurityService,
        IOptions<ConsentPortalOptions>? options)
    {
        _dataProduct = dataProduct;
        _consentSecurityService = consentSecurityService;
        _options = options;
    }

    public async void OnAuthorization(AuthorizationFilterContext context)
    {
        var application = _options?.Value.Application ??
                          throw new InvalidOperationException(
                              $"Missing {nameof(_options.Value.Application)} from consent portal configuration");

        var consentDomain = _options?.Value.Domain ??
                            throw new InvalidOperationException(
                                $"Missing {nameof(_options.Value.Domain)} from consent portal configuration");

        var dataProduct = _dataProduct ?? "test/lassipatanen/User/Profile";
        var consentUri = $"dpp://{application}@{consentDomain}/{dataProduct}";

        await _consentSecurityService.VerifyConsentTokenRequestHeaders(context.HttpContext.Request.Headers, consentUri);
    }
}
