using AzureServer;
using AzureServer.Models;

var builder = WebApplication.CreateBuilder(args);

var startup = new Startup(builder.Configuration);

startup.ConfigureServices(builder.Services);

var app = builder.Build();

startup.Configure(app, builder.Environment);

SeedDatabase();

app.Run();

void SeedDatabase()
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<TodoContext>();
    db.TodoItems.Add(new TodoItem {Name = "Todo item 1"});
    db.TodoItems.Add(new TodoItem {Name = "Todo item 2"});
    db.TodoItems.Add(new TodoItem {Name = "Todo item 3"});
    db.TodoItems.Add(new TodoItem {Name = "Todo item 4"});
    db.TodoItems.Add(new TodoItem {Name = "Todo item 5"});
    db.SaveChanges();
}

