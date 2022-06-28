using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Identity.Client;

namespace ConsoleApp1
{
    class Program
    {
        static async Task Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureServices((context, services) =>
                {
                    var clientId = "18160796-6420-467c-8883-0f7419ed0ae6";
                    var clientSecret = context.Configuration["clientSecret"];
                    var tenantId = "e56b135d-b0e0-4ad8-8faa-1ca3915fe4b2";

                    services.AddSingleton<IConfidentialClientApplication>(_ =>
                    {
                        var app = ConfidentialClientApplicationBuilder.Create(clientId)
                            .WithClientSecret(clientSecret)
                            .WithAuthority(new Uri($"https://login.microsoftonline.com/{tenantId}/"))
                            .Build();

                        return app;
                    });

                    services.AddTransient<AzureAdAuthHandler>();
                    services.AddHttpClient<IWeatherForecastClient, WeatherForecastClient>()
                        .ConfigureHttpClient(client => client.BaseAddress = new Uri("https://localhost:5001"))
                        .AddHttpMessageHandler(sp =>
                        {
                            var handler = sp.GetRequiredService<AzureAdAuthHandler>();

                            handler.ServerApplicationId = "e04a370e-582c-4745-ad1e-cb30d36c3584";

                            return handler;
                        });
                    services.AddHostedService<Worker>();
                });

    }

    public class AzureAdAuthHandler : DelegatingHandler
    {
        private readonly IConfidentialClientApplication _app;
        private readonly ILogger<AzureAdAuthHandler> _logger;

        public AzureAdAuthHandler(IConfidentialClientApplication app, ILogger<AzureAdAuthHandler> logger)
        {
            _app = app;
            _logger = logger;
        }

        public string ServerApplicationId { get; set; }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var scopes = new[] { ServerApplicationId + "/.default" };
            var result = await _app.AcquireTokenForClient(scopes)
                .ExecuteAsync(cancellationToken);

            _logger.LogInformation(result.AccessToken);

            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", result.AccessToken);

            return await base.SendAsync(request, cancellationToken);
        }
    }
}
