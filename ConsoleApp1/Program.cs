using ExternalClient;
using Microsoft.Identity.Client;
using Shared;

var host = Host.CreateDefaultBuilder(args)
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
        services.AddHttpClient<ITodoItemsClient, TodoItemsClient>()
            .ConfigureHttpClient(client => client.BaseAddress = new Uri(context.Configuration["Server:BaseUrl"]))
            .AddHttpMessageHandler(sp =>
            {
                var handler = sp.GetRequiredService<AzureAdAuthHandler>();

                handler.ServerApplicationId = context.Configuration["Server:ApplicationId"];

                return handler;
            });
        services.AddHostedService<WeatherForecastWorker>();
        services.AddHostedService<TodoItemsWorker>();
    })
    .Build();

await host.RunAsync();
