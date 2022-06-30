using System.Collections.Generic;
using Pulumi;
using AzureAD = Pulumi.AzureAD;
using AzureNative = Pulumi.AzureNative;

public class Client
{
    public Client(
        AzureNative.Resources.ResourceGroup resourceGroup,
        AzureNative.Web.AppServicePlan appServicePlan,
        Server server)
    {
        var clientUserAssignedIdentity = new AzureNative.ManagedIdentity.UserAssignedIdentity("azure-ad-example-azure-client-user", new AzureNative.ManagedIdentity.UserAssignedIdentityArgs
        {
            ResourceGroupName = resourceGroup.Name
        });

        var azureClientServerUserReadAssignment = new AzureAD.AppRoleAssignment(
            "azure-ad-example-azure-client-server-user-read-role-assignment", new AzureAD.AppRoleAssignmentArgs
            {
                AppRoleId = server.UserReadRoleUuid,
                PrincipalObjectId = clientUserAssignedIdentity.PrincipalId,
                ResourceObjectId = server.ServicePrincipalObjectId
            });

        var clientAppService = new AzureNative.Web.WebApp("azure-ad-example-azure-client", new AzureNative.Web.WebAppArgs
        {
            Kind = "app,linux",
            Location = resourceGroup.Location,
            ResourceGroupName = resourceGroup.Name,
            ServerFarmId = appServicePlan.Id,
            Enabled = true,
            HttpsOnly = true,
            SiteConfig = new AzureNative.Web.Inputs.SiteConfigArgs
            {
                LinuxFxVersion = "DOTNETCORE|3.1",
                AppCommandLine = "dotnet AzureClient.dll",
                AppSettings = new[]
                {
                    new AzureNative.Web.Inputs.NameValuePairArgs
                    {
                        Name = "Server__BaseUrl",
                        Value = Output.Format($"https://{server.AppServiceDefaultHostName}")
                    }
                }
            },
            Identity = new AzureNative.Web.Inputs.ManagedServiceIdentityArgs
            {
                Type = AzureNative.Web.ManagedServiceIdentityType.UserAssigned,
                UserAssignedIdentities = clientUserAssignedIdentity.Id.Apply(id =>
                {
                    var im = new Dictionary<string, object>
                    {
                        {id, new Dictionary<string, object>()}
                    };
                    return im;
                })
            }
        });

        UserAssignedIdentityClientId = clientUserAssignedIdentity.ClientId;
        AppServiceDefaultHostName = clientAppService.DefaultHostName;
    }

    public Output<string> UserAssignedIdentityClientId { get; set; }
    public Output<string> AppServiceDefaultHostName { get; set; }
}