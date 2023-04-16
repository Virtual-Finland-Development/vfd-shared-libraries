# Shared library for  VFD development

## .NET

### VirtualFinlandDevelopment.Shared

Package contains 
- #### ErrorHandlerMiddleware
- #### Exceptions
    - BadRequestException
    - NotAuthorizedException
    - NotFoundException
- #### TestbedConsentAuthorizationAttribute
To use testbed consent authorization attribute, place `[VerifyConsentAuthorization]` above controller method.

For example:

```
[Route("/user/profile")]
[VerifyConsentAuthorization("data/product/name")]
public async Task<IActionResult> GetMovies(){
  ...
}
```

In additions to attribute, a service has to be registered on Program.cs

```
builder.Services.RegisterTestbedConsentProvider(
    providerOptions => { builder.Configuration.GetSection("Testbed").Bind(providerOptions); },
    portalOptions => { builder.Configuration.GetSection("ConsentPortalConfiguration").Bind(portalOptions); }
);
```

Where Testbed and ConsentPortalConfiguration are sections in appsettings.json with correct values

```
"Testbed": {
  "Issuer": "",
  "ConsentJwksJsonUrl": "",
  "ConsentVerifyUrl": ""
}
```

and

```
"ConsentPortalOptions": {
  "Application": "",
  "Domain": ""
}
```

#### Developing project

For developing this package, create `$home/_dev/packages` folder under users home directory.
Building the project will create new nuget package under `$home/_dev/packages` which can be 
added as Nuget package source on users computer. After creating this package, Nuget 
restore has to be run in order for the local Nuget cache to update.
