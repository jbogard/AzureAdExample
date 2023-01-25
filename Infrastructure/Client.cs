using System.Collections.Generic;
using Pulumi;
using AzureNative = Pulumi.AzureNative;

public class Client
{
    public const string AppName = "azure-client";

    public Client(
        string prefix,
        AzureNative.Resources.ResourceGroup resourceGroup,
        AzureNative.Web.AppServicePlan appServicePlan,
        Server server)
    {
        var userAssignedIdentity = new AzureNative.ManagedIdentity.UserAssignedIdentity($"{prefix}-{AppName}-user",
            new AzureNative.ManagedIdentity.UserAssignedIdentityArgs
            {
                ResourceGroupName = resourceGroup.Name
            });

        var webApp = new AzureNative.Web.WebApp($"{prefix}-{AppName}", new AzureNative.Web.WebAppArgs
        {
            Kind = "app,linux",
            Location = resourceGroup.Location,
            ResourceGroupName = resourceGroup.Name,
            ServerFarmId = appServicePlan.Id,
            Enabled = true,
            HttpsOnly = true,
            SiteConfig = new AzureNative.Web.Inputs.SiteConfigArgs
            {
                LinuxFxVersion = "DOTNETCORE|6.0",
                AppCommandLine = "dotnet AzureClient.dll",
                AppSettings = new[]
                {
                    new AzureNative.Web.Inputs.NameValuePairArgs
                    {
                        Name = "Server__BaseAddress",
                        Value = Output.Format($"https://{server.AppServiceDefaultHostName}")
                    }
                }
            },
            Identity = new AzureNative.Web.Inputs.ManagedServiceIdentityArgs
            {
                Type = AzureNative.Web.ManagedServiceIdentityType.UserAssigned,
                UserAssignedIdentities = userAssignedIdentity.Id.Apply(id =>
                {
                    var im = new Dictionary<string, object>
                    {
                        {id, new Dictionary<string, object>()}
                    };
                    return im;
                })
            }
        });

        UserAssignedIdentityClientId = userAssignedIdentity.ClientId;
        UserAssignedIdentityPrincipalId = userAssignedIdentity.PrincipalId;
        AppServiceDefaultHostName = webApp.DefaultHostName;

    }

    public Output<string> UserAssignedIdentityClientId { get; set; }
    public Output<string> UserAssignedIdentityPrincipalId { get; set; }
    public Output<string> AppServiceDefaultHostName { get; set; }
}