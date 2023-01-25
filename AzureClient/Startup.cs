using Azure.Core;
using Azure.Identity;
using Microsoft.Extensions.Options;
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
        var options = new DefaultAzureCredentialOptions
        {
            ManagedIdentityClientId = managedIdentityClientId,
            VisualStudioTenantId = tenantId
        };

        services.AddSingleton<TokenCredential>(new DefaultAzureCredential(options));
        services.AddTransient(typeof(AzureIdentityAuthHandler<>));

        var serverConfigSection = Configuration.GetSection("Server");

        services.Configure<AzureAdServerApiOptions<IWeatherForecastClient>>(serverConfigSection);
        services.AddHttpClient<IWeatherForecastClient, WeatherForecastClient>()
            .ConfigureHttpClient((sp, client) =>
            {
                var serverOptions = sp.GetRequiredService<IOptions<AzureAdServerApiOptions<IWeatherForecastClient>>>();
                client.BaseAddress = new Uri(serverOptions.Value.BaseAddress);
            })
            .AddHttpMessageHandler<AzureIdentityAuthHandler<IWeatherForecastClient>>();

        services.Configure<AzureAdServerApiOptions<ITodoItemsClient>>(serverConfigSection);
        services.AddHttpClient<ITodoItemsClient, TodoItemsClient>()
            .ConfigureHttpClient((sp, client) =>
            {
                var serverOptions = sp.GetRequiredService<IOptions<AzureAdServerApiOptions<ITodoItemsClient>>>();
                client.BaseAddress = new Uri(serverOptions.Value.BaseAddress);
            })
            .AddHttpMessageHandler<AzureIdentityAuthHandler<ITodoItemsClient>>();
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