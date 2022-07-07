using System.IdentityModel.Tokens.Jwt;
using AzureServer.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Web;
using Microsoft.IdentityModel.Logging;
using Microsoft.OpenApi.Models;

namespace AzureServer;

public class Startup
{
    public Startup(IConfiguration configuration)
    {
        Configuration = configuration;
    }

    public IConfiguration Configuration { get; }

    // This method gets called by the runtime. Use this method to add services to the container.
    public void ConfigureServices(IServiceCollection services)
    {
        JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddMicrosoftIdentityWebApi(Configuration);

        var tenantId = Configuration["AzureAd:TenantId"];

        services.AddControllers();

        services.AddDbContext<TodoContext>(opt =>
            opt.UseInMemoryDatabase("TodoList"));

        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo { Title = "Server", Version = "v1" });

            options.OperationFilter<SwaggerAuthorizeOperationFilter>();

            // This does work if your user has the roles assigned
            options.AddSecurityDefinition("OAuth Auth Code", new OpenApiSecurityScheme
            {
                Type = SecuritySchemeType.OAuth2,
                Flows = new OpenApiOAuthFlows
                {
                    Implicit = new OpenApiOAuthFlow
                    {
                        AuthorizationUrl = new Uri($"https://login.microsoftonline.com/{tenantId}/oauth2/v2.0/authorize"),
                        TokenUrl = new Uri($"https://login.microsoftonline.com/{tenantId}/oauth2/v2.0/token"),
                        Scopes = new Dictionary<string, string>
                        {
                            {"api://azure-ad-example-server/LocalDev", "roles"}
                        }
                    }
                }
            });
        });
    }

    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        if (env.IsDevelopment())
        {
            IdentityModelEventSource.ShowPII = true;

            app.UseDeveloperExceptionPage();
        }

        app.UseSwagger();
        app.UseSwaggerUI(options =>
        {
            options.SwaggerEndpoint("/swagger/v1/swagger.json", "Server v1");
            var serverApplicationId = Configuration["AzureAd:ClientId"];
            options.OAuthClientId(serverApplicationId);
            options.OAuthScopeSeparator(" ");
            options.OAuthUsePkce();
        });

        app.UseHttpsRedirection();

        app.UseRouting();

        app.UseAuthentication();
        app.UseAuthorization();

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();
        });
    }
}