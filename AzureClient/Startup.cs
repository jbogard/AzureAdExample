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

        #region Configure HTTP Client

        #region Configure Token Aquirer-er

        var managedIdentityClientId = Configuration["ClientId"];
        var tenantId = Configuration["TenantId"];
        var options = new DefaultAzureCredentialOptions
        {
            ManagedIdentityClientId = managedIdentityClientId,
            VisualStudioTenantId = tenantId
        };

        services.AddSingleton<TokenCredential>(new DefaultAzureCredential(options));
        services.AddTransient(typeof(AzureIdentityAuthHandler<>));

        #endregion

        #region Add Http Clients

        var serverConfigSection = Configuration.GetSection("Server");

        services.Configure<EntraIdServerApiOptions<IWeatherForecastClient>>(serverConfigSection);
        services.AddHttpClient<IWeatherForecastClient, WeatherForecastClient>()
            .ConfigureHttpClient((sp, client) =>
            {
                var serverOptions = sp.GetRequiredService<IOptions<EntraIdServerApiOptions<IWeatherForecastClient>>>();
                client.BaseAddress = new Uri(serverOptions.Value.BaseAddress);
            })
            .AddHttpMessageHandler<AzureIdentityAuthHandler<IWeatherForecastClient>>();

        services.Configure<EntraIdServerApiOptions<ITodoItemsClient>>(serverConfigSection);
        services.AddHttpClient<ITodoItemsClient, TodoItemsClient>()
            .ConfigureHttpClient((sp, client) =>
            {
                var serverOptions = sp.GetRequiredService<IOptions<EntraIdServerApiOptions<ITodoItemsClient>>>();
                client.BaseAddress = new Uri(serverOptions.Value.BaseAddress);
            })
            .AddHttpMessageHandler<AzureIdentityAuthHandler<ITodoItemsClient>>();

        #endregion

        #endregion
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