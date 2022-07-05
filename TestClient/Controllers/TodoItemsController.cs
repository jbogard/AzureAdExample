using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Shared;

namespace AzureClient.Controllers;

[Route("api/[controller]")]
[ApiController]
public class TodoItemsController : ControllerBase
{
    private readonly ITodoItemsClient _client;

    public TodoItemsController(ITodoItemsClient client)
    {
        _client = client;
    }

    [HttpGet]
    public async Task<IEnumerable<TodoItem>?> GetTodoItems()
    {
        return await _client.GetAsync();
    }

    [HttpGet("{id}")]
    public async Task<TodoItem?> GetTodoItem(long id)
    {
        return await _client.GetAsync(id);
    }

    [HttpPut("{id}")]
    public async Task PutTodoItem(long id, TodoItem todoItem)
    {
        await _client.PutAsync(id, todoItem);
    }

    [HttpPost]
    public async Task<TodoItem?> PostTodoItem(TodoItem todoItem)
    {
        return await _client.PostAsync(todoItem);
    }

    [HttpDelete("{id}")]
    public async Task DeleteTodoItem(long id)
    {
        await _client.DeleteAsync(id);
    }
}
