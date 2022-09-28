using System.Text.Json;
using Shared;

namespace ExternalClient;

public class TodoItemsWorker : BackgroundService
{
    private readonly ILogger<TodoItemsWorker> _logger;
    private readonly IServiceProvider _serviceProvider;

    public TodoItemsWorker(ILogger<TodoItemsWorker> logger, IServiceProvider serviceProvider)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            using var scope = _serviceProvider.CreateScope();
            var client = scope.ServiceProvider.GetRequiredService<ITodoItemsClient>();
            try
            {
                var response = await client.GetAsync(1);

                if (response != null)
                {
                    await client.PutAsync(response.Id, response);

                    _logger.LogInformation(JsonSerializer.Serialize(response));
                }
            }
            catch (HttpRequestException e)
            {
                _logger.LogError(e, "Got error making connection.");
            }

            await Task.Delay(2000, stoppingToken);
        }
    }
}