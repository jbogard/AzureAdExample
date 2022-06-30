using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace ConsoleApp1
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly IWeatherForecastClient _client;

        public Worker(ILogger<Worker> logger, IWeatherForecastClient client)
        {
            _logger = logger;
            _client = client;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                IEnumerable<WeatherForecast> response;

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

                await Task.Delay(2000, stoppingToken);
            }
        }
    }
}