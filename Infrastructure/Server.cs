using Pulumi;
using AzureAD = Pulumi.AzureAD;
using AzureNative = Pulumi.AzureNative;

public class Server
{
    public const string AppName = "server";

    public Server(
        string prefix,
        AzureNative.Resources.ResourceGroup resourceGroup,
        AzureNative.Web.AppServicePlan appServicePlan)
    {
        var serverAppService = new AzureNative.Web.WebApp($"{prefix}-{AppName}", new AzureNative.Web.WebAppArgs
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
                AppCommandLine = "dotnet AzureServer.dll"
            },
        });

        var localDevScopeUuid = new Pulumi.Random.RandomUuid($"{prefix}-{AppName}-local-dev-scope-id");
        var todoReadRoleUuid = new Pulumi.Random.RandomUuid($"{prefix}-{AppName}-todo-read-role-id");
        var todoWriteRoleUuid = new Pulumi.Random.RandomUuid($"{prefix}-{AppName}-todo-write-role-id");
        var serverApplication = new AzureAD.Application($"{prefix}-{AppName}", new AzureAD.ApplicationArgs
        {
            DisplayName = "Azure AD Example Server",
            IdentifierUris =
            {
                $"api://{prefix}-{AppName}"
            },
            Api = new AzureAD.Inputs.ApplicationApiArgs
            {
                RequestedAccessTokenVersion = 2,
                Oauth2PermissionScopes =
                {
                    new AzureAD.Inputs.ApplicationApiOauth2PermissionScopeArgs
                    {
                        AdminConsentDescription = "Local development",
                        AdminConsentDisplayName = "LocalDev",
                        Id = localDevScopeUuid.Result,
                        Enabled = true,
                        Value = "LocalDev",
                        Type = "User"
                    }
                }
            },
            AppRoles =
            {
                new AzureAD.Inputs.ApplicationAppRoleArgs
                {
                    AllowedMemberTypes =
                    {
                        "User",
                        "Application"
                    },
                    DisplayName = "Todo.Read",
                    Enabled = true,
                    Value = "Todo.Read",
                    Description = "Todo.Read",
                    Id = todoReadRoleUuid.Result
                },
                new AzureAD.Inputs.ApplicationAppRoleArgs
                {
                    AllowedMemberTypes =
                    {
                        "User",
                        "Application"
                    },
                    DisplayName = "Todo.Write",
                    Enabled = true,
                    Value = "Todo.Write",
                    Description = "Todo.Write",
                    Id = todoWriteRoleUuid.Result
                }
            },
            SinglePageApplication = new AzureAD.Inputs.ApplicationSinglePageApplicationArgs
            {
                RedirectUris =
                {
                    Output.Format($"https://{serverAppService.DefaultHostName}/swagger/oauth2-redirect.html"),
                    "https://localhost:5001/swagger/oauth2-redirect.html"
                }
            }
        });

        var serverServicePrincipal =
            new AzureAD.ServicePrincipal($"{prefix}-{AppName}-service-principal",
                new AzureAD.ServicePrincipalArgs
                {
                    ApplicationId = serverApplication.ApplicationId,
                });

        var visualStudio =
            new AzureAD.ApplicationPreAuthorized(
                $"{prefix}-{AppName}-preauth-visualstudio",
                new AzureAD.ApplicationPreAuthorizedArgs
                {
                    // This is the """well-known""" client ID for Visual Studio
                    AuthorizedAppId = "872cd9fa-d31f-45e0-9eab-6e460a02d1f1",
                    ApplicationObjectId = serverApplication.ObjectId,
                    PermissionIds =
                    {
                        localDevScopeUuid.Result
                    }
                });
        //var azureCli = new AzureAD.ApplicationPreAuthorized($"{prefix}-{AppName}-preauth-azurecli",
        //    new AzureAD.ApplicationPreAuthorizedArgs
        //    {
        //        // This is the """well-known""" client ID for Azure CLI
        //        AuthorizedAppId = "04b07795-8ddb-461a-bbee-02f9e1bf7b46",
        //        ApplicationObjectId = serverApplication.ObjectId,
        //        PermissionIds =
        //        {
        //            localDevScopeUuid.Result
        //        }
        //    });

        TodoReadRoleUuid = todoReadRoleUuid.Result;
        TodoWriteRoleUuid = todoWriteRoleUuid.Result;
        ServicePrincipalObjectId = serverServicePrincipal.ObjectId;
        AppServiceDefaultHostName = serverAppService.DefaultHostName;
        ApplicationApplicationId = serverApplication.ApplicationId;
    }

    public Output<string> TodoReadRoleUuid { get; set; }
    public Output<string> TodoWriteRoleUuid { get; set; }
    public Output<string> ServicePrincipalObjectId { get; set; }
    public Output<string> ApplicationApplicationId { get; set; }
    public Output<string> AppServiceDefaultHostName { get; set; }

    public void AssignRoles(string prefix, AzureAdResources azureAdResources)
    {
        AssignRead(prefix, 
            AzureAdResources.LocalDevGroupName, 
            azureAdResources.LocalDevGroupObjectId);
        AssignWrite(prefix, 
            AzureAdResources.LocalDevGroupName, 
            azureAdResources.LocalDevGroupObjectId);
    }

    public void AssignRoles(string prefix, Client client)
    {
        AssignRead(prefix, Client.AppName, client.UserAssignedIdentityPrincipalId);
        AssignWrite(prefix, Client.AppName, client.UserAssignedIdentityPrincipalId);
    }

    public void AssignRoles(string prefix, ExternalClient externalClient)
    {
        AssignRead(prefix, 
            ExternalClient.AppName, 
            externalClient.ApplicationServicePrincipalObjectId);
    }

    private AzureAD.AppRoleAssignment AssignRead(string prefix, 
        string assigneeName, 
        Output<string> principalObjectId)
    {
        return new AzureAD.AppRoleAssignment(
            $"{prefix}-{assigneeName}-{AppName}-todo-read-role-assignment", 
            new AzureAD.AppRoleAssignmentArgs
            {
                AppRoleId = TodoReadRoleUuid,
                PrincipalObjectId = principalObjectId,
                ResourceObjectId = ServicePrincipalObjectId
            });
    }

    private AzureAD.AppRoleAssignment AssignWrite(string prefix, 
        string assigneeName, 
        Output<string> principalObjectId)
    {
        return new AzureAD.AppRoleAssignment(
            $"{prefix}-{assigneeName}-{AppName}-todo-write-role-assignment", 
            new AzureAD.AppRoleAssignmentArgs
            {
                AppRoleId = TodoWriteRoleUuid,
                PrincipalObjectId = principalObjectId,
                ResourceObjectId = ServicePrincipalObjectId
            });
    }


}