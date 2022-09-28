using System.Text.Json;
using Shared;

namespace ExternalClient;

public class WeatherForecastWorker : BackgroundService
{
    private readonly ILogger<WeatherForecastWorker> _logger;
    private readonly IWeatherForecastClient _client;

    public WeatherForecastWorker(
        ILogger<WeatherForecastWorker> logger, 
        IWeatherForecastClient client)
    {
        _logger = logger;
        _client = client;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            IEnumerable<WeatherForecast>? response;

            try
            {
                response = await _client.GetAsync();
            }
            catch (HttpRequestException e)
            {
                _logger.LogError(e, "Got error making connection.");
                return;
            }

            _logger.LogInformation(JsonSerializer.Serialize(response));

            await Task.Delay(5000, stoppingToken);
        }
    }
}