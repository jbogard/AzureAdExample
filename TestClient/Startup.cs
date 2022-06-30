using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Azure.Core;
using Azure.Identity;
using Microsoft.OpenApi.Models;

namespace TestClient
{
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
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "WebApplication2", Version = "v1" });
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
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseSwagger();
            app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "WebApplication2 v1"));

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }

    public class AzureIdentityAuthHandler : DelegatingHandler
    {
        private readonly TokenCredential _credential;
        private readonly ILogger<AzureIdentityAuthHandler> _logger;

        public AzureIdentityAuthHandler(TokenCredential credential, 
            ILogger<AzureIdentityAuthHandler> logger)
        {
            _credential = credential;
            _logger = logger;
        }

        public string ServerApplicationId { get; set; }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, 
            CancellationToken cancellationToken)
        {
            var scopes = new[] { ServerApplicationId + "/.default" };
            var tokenRequestContext = new TokenRequestContext(scopes);
            var result = await _credential.GetTokenAsync(tokenRequestContext, cancellationToken);

            _logger.LogInformation(result.Token);

            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", result.Token);

            return await base.SendAsync(request, cancellationToken);
        }
    }

}
