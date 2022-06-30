using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
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
                .UseEnvironment(Environments.Development)
                .ConfigureAppConfiguration((context, builder) =>
                {
                    builder.AddUserSecrets<Program>();
                })
                .ConfigureServices((context, services) =>
                {
                    var clientId = context.Configuration["ClientId"];
                    var clientSecret = context.Configuration["ClientSecret"];
                    var tenantId = context.Configuration["TenantId"];

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
                        .ConfigureHttpClient(client => client.BaseAddress = new Uri(context.Configuration["Server:BaseUrl"]))
                        .AddHttpMessageHandler(sp =>
                        {
                            var handler = sp.GetRequiredService<AzureAdAuthHandler>();

                            handler.ServerApplicationId = context.Configuration["Server:ApplicationId"];

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
