using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Threading.Tasks;
using Azure.Core;
using Azure.Identity;

namespace TestClient.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        private readonly ILogger<WeatherForecastController> _logger;
        private readonly IWeatherForecastClient _client;

        public WeatherForecastController(ILogger<WeatherForecastController> logger, IWeatherForecastClient client)
        {
            _logger = logger;
            _client = client;
        }

        [HttpGet]
        public async Task<IEnumerable<WeatherForecast>> Get()
        {
            var weather = await _client.GetAsync();

            return weather;
                //var serverApplicationId = "e04a370e-582c-4745-ad1e-cb30d36c3584";
                //var scopes = new[] { serverApplicationId + "/.default" };

                //var credential = new DefaultAzureCredential(new DefaultAzureCredentialOptions
                //{
                //    ManagedIdentityClientId = "25a915cb-66e3-4029-b93b-c53dd050472f",
                //    VisualStudioTenantId = "e56b135d-b0e0-4ad8-8faa-1ca3915fe4b2"
                //});

                //var accessToken = await credential.GetTokenAsync(new TokenRequestContext(scopes));

                //var handler = new JwtSecurityTokenHandler();
                //var token = handler.ReadJwtToken(accessToken.Token);
                //// This token does NOT have the roles claim

                //return token.Claims.Select(c => $"{c.Type}:{c.Value}").ToArray();
        }
    }
}
