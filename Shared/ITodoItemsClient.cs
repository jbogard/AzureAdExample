using System.Net.Http.Json;

namespace Shared;

public interface ITodoItemsClient
{
    Task<IEnumerable<TodoItem>?> GetAsync();
    Task<TodoItem?> GetAsync(long id);
    Task PutAsync(long id, TodoItem todoItem);
    Task<TodoItem?> PostAsync(TodoItem todoItem);
    Task DeleteAsync(long id);
}

public class TodoItemsClient : ITodoItemsClient
{
    private readonly HttpClient _client;

    public TodoItemsClient(HttpClient client)
    {
        _client = client;
    }

    public async Task<IEnumerable<TodoItem>?> GetAsync()
    {
        return await _client.GetFromJsonAsync<IEnumerable<TodoItem>>("/api/todoitems");
    }

    public async Task<TodoItem?> GetAsync(long id)
    {
        return await _client.GetFromJsonAsync<TodoItem>($"/api/todoitems/{id}");
    }

    public async Task PutAsync(long id, TodoItem todoItem)
    {
        var responseMessage = await _client.PutAsJsonAsync($"/api/todoitems/{id}", todoItem);

        responseMessage.EnsureSuccessStatusCode();
    }

    public async Task<TodoItem?> PostAsync(TodoItem todoItem)
    {
        var response = await _client.PostAsJsonAsync($"/api/todoitems", todoItem);

        return await response.Content.ReadFromJsonAsync<TodoItem>();
    }

    public async Task DeleteAsync(long id)
    {
        var responseMessage = await _client.DeleteAsync($"/api/todoitems/{id}");

        responseMessage.EnsureSuccessStatusCode();
    }
}