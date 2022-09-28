using System.Text.Json;
using Shared;

namespace ExternalClient;

public class WeatherForecastWorker : BackgroundService
{
    private readonly ILogger<WeatherForecastWorker> _logger;
    private readonly IServiceProvider _serviceProvider;

    public WeatherForecastWorker(
        ILogger<WeatherForecastWorker> logger, 
        IServiceProvider serviceProvider)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            using var scope = _serviceProvider.CreateScope();
            var client = scope.ServiceProvider.GetRequiredService<IWeatherForecastClient>();
            IEnumerable<WeatherForecast>? response;

            try
            {
                response = await client.GetAsync();
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