using ExternalClient;
using Microsoft.Extensions.Options;
using Microsoft.Identity.Client;
using Shared;

var host = Host.CreateDefaultBuilder(args)
    .UseEnvironment(Environments.Development)
    .ConfigureAppConfiguration((_, builder) =>
    {
        builder.AddUserSecrets<Program>();
    })
    .ConfigureServices((context, services) =>
    {
        var clientId = context.Configuration["ClientId"];
        var clientSecret = context.Configuration["ClientSecret"];
        var tenantId = context.Configuration["TenantId"];

        services.AddSingleton(_ =>
        {
            var app = ConfidentialClientApplicationBuilder.Create(clientId)
                .WithClientSecret(clientSecret)
                .WithAuthority(new Uri($"https://login.microsoftonline.com/{tenantId}/"))
                .Build();

            return app;
        });

        services.Configure<AzureAdServerApiOptions<ITodoItemsClient>>(
            context.Configuration.GetSection("Server"));
        services.Configure<AzureAdServerApiOptions<IWeatherForecastClient>>(
            context.Configuration.GetSection("Server"));

        services.AddTransient<
            ConfidentialClientApplicationAuthHandler<IWeatherForecastClient>>();
        services.AddTransient<
            ConfidentialClientApplicationAuthHandler<ITodoItemsClient>>();

        services.AddHttpClient<IWeatherForecastClient, WeatherForecastClient>()
            .ConfigureHttpClient((sp, client) =>
            {
                var serverOptions = sp.GetRequiredService<
                    IOptions<AzureAdServerApiOptions<IWeatherForecastClient>>>();
                client.BaseAddress = new Uri(serverOptions.Value.BaseAddress);
            })
            .AddHttpMessageHandler<
                ConfidentialClientApplicationAuthHandler<IWeatherForecastClient>>();
        services.AddHttpClient<ITodoItemsClient, TodoItemsClient>()
            .ConfigureHttpClient((sp, client) =>
            {
                var serverOptions = sp.GetRequiredService<
                    IOptions<AzureAdServerApiOptions<ITodoItemsClient>>>();
                client.BaseAddress = new Uri(serverOptions.Value.BaseAddress);
            })
            .AddHttpMessageHandler<
                ConfidentialClientApplicationAuthHandler<IWeatherForecastClient>>();

        services.AddHostedService<WeatherForecastWorker>();
        services.AddHostedService<TodoItemsWorker>();

    })
    .Build();

await host.RunAsync();
