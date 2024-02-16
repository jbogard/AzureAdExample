using System.IdentityModel.Tokens.Jwt;
using AzureServer;
using AzureServer.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Web;
using Microsoft.IdentityModel.Logging;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddDbContext<TodoContext>(opt =>
    opt.UseInMemoryDatabase("TodoList"));

#region Configure Swagger Gen

builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "Server", Version = "v1" });
    options.OperationFilter<SwaggerAuthorizeOperationFilter>();

    var tenantId = builder.Configuration["AzureAd:TenantId"];
    options.AddSecurityDefinition("OAuth Auth Code", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.OAuth2,
        Flows = new OpenApiOAuthFlows
        {
            AuthorizationCode = new OpenApiOAuthFlow
            {
                AuthorizationUrl = new Uri($"https://login.microsoftonline.com/{tenantId}/oauth2/v2.0/authorize"),
                TokenUrl = new Uri($"https://login.microsoftonline.com/{tenantId}/oauth2/v2.0/token"),
                Scopes = new Dictionary<string, string>
                {
                    {"api://azure-ad-example-server/LocalDev", "Azure Server Web API"}
                }
            }
        }
    });
});

#endregion

#region Add Web API Authentication

JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();

builder.Services.AddMicrosoftIdentityWebApiAuthentication(builder.Configuration);
builder.Services.Configure<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme, config =>
{
    config.MapInboundClaims = false;
});

#endregion

var app = builder.Build();

if (builder.Environment.IsDevelopment())
{
    IdentityModelEventSource.ShowPII = true;

    app.UseDeveloperExceptionPage();
}

#region Configure Swagger UI

app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "Server v1");
    options.OAuthClientId(builder.Configuration["AzureAd:ClientId"]);
    options.OAuthScopeSeparator(" ");
    options.OAuthUsePkce();
});

#endregion

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

