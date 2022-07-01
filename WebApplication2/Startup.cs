using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Identity.Web;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Logging;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace AzureServer
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
            JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();

            services.AddAuthentication("Bearer")
                .AddMicrosoftIdentityWebApi(Configuration);

            var tenantId = Configuration["AzureAd:TenantId"];
            var serverApplicationId = Configuration["AzureAd:ClientId"];

            services.AddControllers();
            services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo { Title = "WebApplication2", Version = "v1" });

                var sp = services.BuildServiceProvider();
                var httpContextAccessor = sp.GetService<IHttpContextAccessor>();
                var host = httpContextAccessor.HttpContext.Request.Host.Host;
                var scheme = "https";

                if (host.StartsWith("localhost", StringComparison.OrdinalIgnoreCase) || host.StartsWith("127.0.0.1"))
                {
                    scheme = httpContextAccessor.HttpContext.Request.Scheme;

                    if (httpContextAccessor.HttpContext.Request.Host.Port.HasValue)
                    {
                        host = $"{host}:{httpContextAccessor.HttpContext.Request.Host.Port.Value}";
                    }
                }

                var openApiServer = new OpenApiServer { Url = $"{scheme}://{host}" };
                options.AddServer(openApiServer);

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
                options.SwaggerEndpoint("/swagger/v1/swagger.json", "WebApplication2 v1");
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

    public class SwaggerAuthorizeOperationFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            var controllerAuthorize = context.MethodInfo.DeclaringType.GetCustomAttribute<AuthorizeAttribute>();
            var methodAuthorize = context.MethodInfo.GetCustomAttribute<AuthorizeAttribute>();
            var hasAuthorize =
                controllerAuthorize != null 
                || methodAuthorize != null;

            if (hasAuthorize)
            {
                operation.Responses.Add("401", new OpenApiResponse { Description = "Unauthorized" });
                operation.Responses.Add("403", new OpenApiResponse { Description = "Forbidden" });

                var roles = (controllerAuthorize?.Roles != null
                    ? controllerAuthorize.Roles.Split(",").Select(s => s.Trim())
                        : Enumerable.Empty<string>())
                        .Concat(methodAuthorize?.Roles != null
                            ? methodAuthorize.Roles.Split(",").Select(s => s.Trim())
                            : Enumerable.Empty<string>())
                        .ToArray();

                operation.Security = new List<OpenApiSecurityRequirement>
                {
                    new OpenApiSecurityRequirement
                    {
                        [
                            new OpenApiSecurityScheme
                            {
                                Reference = new OpenApiReference
                                {
                                    Type = ReferenceType.SecurityScheme,
                                    Id = "OAuth Auth Code"
                                }
                            }
                        ] = roles
                    }
                };
            }
        }
    }

}
