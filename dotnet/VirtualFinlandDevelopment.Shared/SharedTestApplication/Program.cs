using VirtualFinlandDevelopment.Shared.Middlewares;
using VirtualFinlandDevelopment.Shared.Services;
using VirtualFinlandDevelopment.Shared.Services.Security.Features;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddHttpClient();

builder.Services.RegisterSecurityFeatures(builder.Configuration);



//builder.Services.RegisterSecurityFeatures(builder.Configuration);

builder.Services.RegisterSecurityFeatures(
    testbedOptions => { builder.Configuration.GetSection("Testbed").Bind(testbedOptions); },
    sinunaOptions => { builder.Configuration.GetSection("Sinuna").Bind(sinunaOptions); },
    suomiFiOptions => { builder.Configuration.GetSection("SuomiFi").Bind(suomiFiOptions); }
);

builder.Services.RegisterTestbedConsentProvider(
    providerOptions => { builder.Configuration.GetSection("Testbed").Bind(providerOptions); },
    portalOptions => { builder.Configuration.GetSection("ConsentPortalConfiguration").Bind(portalOptions); }
);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.UseErrorHandlerMiddleware();

app.MapControllers();

app.Run();
