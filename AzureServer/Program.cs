using System.IdentityModel.Tokens.Jwt;
using AzureServer;
using AzureServer.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Web;
using Microsoft.IdentityModel.Logging;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddDbContext<TodoContext>(opt =>
    opt.UseInMemoryDatabase("TodoList"));

builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "Server", Version = "v1" });
});

var app = builder.Build();

if (builder.Environment.IsDevelopment())
{
    IdentityModelEventSource.ShowPII = true;

    app.UseDeveloperExceptionPage();
}

app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "Server v1");
});

app.UseHttpsRedirection();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();
});

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

