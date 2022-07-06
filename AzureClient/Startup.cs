using Azure.Core;
using Azure.Identity;
using Microsoft.OpenApi.Models;
using Shared;

namespace AzureClient;

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
        services.AddControllers();
        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo { Title = "Azure Client", Version = "v1" });
        });

        var managedIdentityClientId = Configuration["ClientId"];
        var tenantId = Configuration["TenantId"];
        var applicationId = Configuration["Server:ApplicationId"];
        var options = new DefaultAzureCredentialOptions
        {
            ManagedIdentityClientId = managedIdentityClientId,
            VisualStudioTenantId = tenantId,
            InteractiveBrowserTenantId = tenantId,
            ExcludeInteractiveBrowserCredential = false,
            
        };

        services.AddSingleton<TokenCredential>(new DefaultAzureCredential(options));
        services.AddTransient<AzureIdentityAuthHandler>();
        services.AddHttpClient<IWeatherForecastClient, WeatherForecastClient>()
            .ConfigureHttpClient(client => client.BaseAddress = new Uri(Configuration["Server:BaseUrl"]))
            .AddHttpMessageHandler(sp =>
            {
                var handler = sp.GetRequiredService<AzureIdentityAuthHandler>();

                handler.ServerApplicationId = applicationId;

                return handler;
            });
        services.AddHttpClient<ITodoItemsClient, TodoItemsClient>()
            .ConfigureHttpClient(client => client.BaseAddress = new Uri(Configuration["Server:BaseUrl"]))
            .AddHttpMessageHandler(sp =>
            {
                var handler = sp.GetRequiredService<AzureIdentityAuthHandler>();

                handler.ServerApplicationId = applicationId;

                return handler;
            });
    }

    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }

        app.UseSwagger();
        app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Azure Client v1"));

        app.UseHttpsRedirection();

        app.UseRouting();

        app.UseAuthorization();

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();
        });
    }
}