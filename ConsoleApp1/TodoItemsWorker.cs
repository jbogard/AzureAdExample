using System.Text.Json;
using Shared;

namespace ExternalClient;

public class TodoItemsWorker : BackgroundService
{
    private readonly ILogger<TodoItemsWorker> _logger;
    private readonly ITodoItemsClient _client;

    public TodoItemsWorker(ILogger<TodoItemsWorker> logger, ITodoItemsClient client)
    {
        _logger = logger;
        _client = client;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var response = await _client.GetAsync(1);

                if (response != null)
                {
                    await _client.PutAsync(response.Id, response);

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