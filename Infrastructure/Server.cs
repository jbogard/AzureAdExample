using Pulumi;
using Pulumi.Random;
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
        #region Create App Service
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
                LinuxFxVersion = "DOTNETCORE|8.0",
                AppCommandLine = "dotnet AzureServer.dll"
            },
        });
        #endregion

        #region Create Server Application

        #region Create Well-Known Role IDs

        var todoReadRoleUuid = new RandomUuid($"{prefix}-{AppName}-todo-read-role-id");
        var todoWriteRoleUuid = new RandomUuid($"{prefix}-{AppName}-todo-write-role-id");

        #endregion

        #region Create Well-Known Scope ID
        var localDevScopeUuid = new Pulumi.Random.RandomUuid($"{prefix}-{AppName}-local-dev-scope-id");
        #endregion

        var serverApplication = new AzureAD.Application($"{prefix}-{AppName}", new AzureAD.ApplicationArgs
        {
            DisplayName = "Azure AD Example Server",
            IdentifierUris =
            {
                $"api://{prefix}-{AppName}"
            },

            #region Create Roles
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
            #endregion

            #region Create API Definition
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
            #endregion

            #region Set Up OAuth2 Auth Code Flow
            SinglePageApplication = new AzureAD.Inputs.ApplicationSinglePageApplicationArgs
            {
                RedirectUris =
                {
                    Output.Format($"https://{serverAppService.DefaultHostName}/swagger/oauth2-redirect.html"),
                    "https://localhost:5001/swagger/oauth2-redirect.html"
                }
            }
            #endregion
        });
        #endregion

        #region Create Managed Application Service Principal
        var serverServicePrincipal =
            new AzureAD.ServicePrincipal($"{prefix}-{AppName}-service-principal",
                new AzureAD.ServicePrincipalArgs
                {
                    ApplicationId = serverApplication.ApplicationId,
                });
        #endregion

        #region Authorize Visual Studio Clients for Scopes
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

        var visualStudioWithNativeMsa =
            new AzureAD.ApplicationPreAuthorized(
                $"{prefix}-{AppName}-preauth-visualstudio-with-msa",
                new AzureAD.ApplicationPreAuthorizedArgs
                {
                    // This is the """well-known""" client ID for Visual Studio with MSA
                    AuthorizedAppId = "04f0c124-f2bc-4f59-8241-bf6df9866bbd",
                    ApplicationObjectId = serverApplication.ObjectId,
                    PermissionIds =
                    {
                        localDevScopeUuid.Result
                    }
                });
        
        var azureCli =
            new AzureAD.ApplicationPreAuthorized(
                $"{prefix}-{AppName}-preauth-azure-cli",
                new AzureAD.ApplicationPreAuthorizedArgs
                {
                    // This is the """well-known""" client ID for Microsoft Azure CLI
                    AuthorizedAppId = "04b07795-8ddb-461a-bbee-02f9e1bf7b46",
                    ApplicationObjectId = serverApplication.ObjectId,
                    PermissionIds =
                    {
                        localDevScopeUuid.Result
                    }
                });
        #endregion

        #region Set Outputs
        TodoReadRoleUuid = todoReadRoleUuid.Result;
        TodoWriteRoleUuid = todoWriteRoleUuid.Result;
        ServicePrincipalObjectId = serverServicePrincipal.ObjectId;
        AppServiceDefaultHostName = serverAppService.DefaultHostName;
        ApplicationApplicationId = serverApplication.ApplicationId;
        #endregion
    }

    #region Assign Roles
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

    #endregion

    #region Assign Local Dev Group Roles

    public void AssignRoles(string prefix, EntraIdResources entraIdResources)
    {
        AssignRead(prefix,
            EntraIdResources.LocalDevGroupName,
            entraIdResources.LocalDevGroupObjectId);
        AssignWrite(prefix,
            EntraIdResources.LocalDevGroupName,
            entraIdResources.LocalDevGroupObjectId);
    }

    #endregion

    #region Assign External Client Group Roles
    public void AssignRoles(string prefix, ExternalClient externalClient)
    {
        AssignRead(prefix,
            ExternalClient.AppName,
            externalClient.ApplicationServicePrincipalObjectId);
    }
    #endregion

    #region Assign Azure Client Group Roles
    public void AssignRoles(string prefix, Client client)
    {
        AssignRead(prefix, Client.AppName, client.UserAssignedIdentityPrincipalId);
        AssignWrite(prefix, Client.AppName, client.UserAssignedIdentityPrincipalId);
    }
    #endregion

    public Output<string> TodoReadRoleUuid { get; set; }
    public Output<string> TodoWriteRoleUuid { get; set; }
    public Output<string> ServicePrincipalObjectId { get; set; }
    public Output<string> ApplicationApplicationId { get; set; }
    public Output<string> AppServiceDefaultHostName { get; set; }


}