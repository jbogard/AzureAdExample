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

    })
    .Build();

await host.RunAsync();
