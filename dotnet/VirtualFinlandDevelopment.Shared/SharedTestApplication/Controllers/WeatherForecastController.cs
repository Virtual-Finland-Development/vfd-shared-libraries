using Microsoft.AspNetCore.Mvc;
using VirtualFinlandDevelopment.Shared.Services;

namespace SharedTestApplication.Controllers;

[ApiController]
[Route("[controller]")]
public class WeatherForecastController : ControllerBase
{
    private static readonly string[] Summaries = new[]
    {
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    };

    private readonly ILogger<WeatherForecastController> _logger;
    private readonly TestbedConsentSecurityService _securityService;

    public WeatherForecastController(ILogger<WeatherForecastController> logger, TestbedConsentSecurityService securityService)
    {
        _logger = logger;
        _securityService = securityService;
    }

    [HttpGet(Name = "GetWeatherForecast")]
    public async Task<IEnumerable<WeatherForecast>> Get()
    {
        await _securityService.VerifyConsentTokenRequestHeaders(Request.Headers, "test/lassipatanen/User/Profile");
        
        return Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateTime.Now.AddDays(index),
                TemperatureC = Random.Shared.Next(-20, 55),
                Summary = Summaries[Random.Shared.Next(Summaries.Length)]
            })
            .ToArray();
    }
}
