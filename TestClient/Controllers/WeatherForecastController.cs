using Microsoft.AspNetCore.Mvc;
using Shared;

namespace AzureClient.Controllers;

[ApiController]
[Route("[controller]")]
public class WeatherForecastController : ControllerBase
{
    private readonly IWeatherForecastClient _client;

    public WeatherForecastController(IWeatherForecastClient client)
    {
        _client = client;
    }

    [HttpGet]
    public async Task<IEnumerable<WeatherForecast>> Get()
    {
        var weather = await _client.GetAsync();

        return weather;
    }
}