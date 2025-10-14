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
        #region Create App Service

        #region Create User Assigned Identity

        var userAssignedIdentity = new AzureNative.ManagedIdentity.UserAssignedIdentity($"{prefix}-{AppName}-user",
            new AzureNative.ManagedIdentity.UserAssignedIdentityArgs
            {
                ResourceGroupName = resourceGroup.Name
            });

        #endregion

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
                LinuxFxVersion = "DOTNETCORE|8.0",
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

            #region Set Managed Identity

            Identity = new AzureNative.Web.Inputs.ManagedServiceIdentityArgs
            {
                Type = AzureNative.Web.ManagedServiceIdentityType.UserAssigned,
                UserAssignedIdentities = userAssignedIdentity.Id
            }

            #endregion

        });

        #endregion

        #region Set Outputs

        UserAssignedIdentityClientId = userAssignedIdentity.ClientId;
        UserAssignedIdentityPrincipalId = userAssignedIdentity.PrincipalId;
        AppServiceDefaultHostName = webApp.DefaultHostName;

        #endregion

    }

    public Output<string> UserAssignedIdentityClientId { get; set; }
    public Output<string> UserAssignedIdentityPrincipalId { get; set; }
    public Output<string> AppServiceDefaultHostName { get; set; }
}