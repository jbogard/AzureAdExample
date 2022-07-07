using System.Reflection;
using Microsoft.AspNetCore.Authorization;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace AzureServer;

public class SwaggerAuthorizeOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var controllerAuthorize = context.MethodInfo.DeclaringType?.GetCustomAttribute<AuthorizeAttribute>();
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
                new()
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