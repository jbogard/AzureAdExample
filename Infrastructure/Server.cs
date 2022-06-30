using Pulumi;
using Pulumi.AzureNative.Resources;
using Pulumi.AzureNative.Web;
using AzureAD = Pulumi.AzureAD;
using AzureNative = Pulumi.AzureNative;

public class Server
{
    public Server(ResourceGroup resourceGroup,
        AppServicePlan appServicePlan, AzureAdResources azureAdResources)
    {
        var serverAppService = new AzureNative.Web.WebApp("azure-ad-example-server", new AzureNative.Web.WebAppArgs
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
                AppCommandLine = "dotnet AzureServer.dll"
            },
        });

        var localDevScopeUuid = new Pulumi.Random.RandomUuid("azure-ad-example-server-local-dev-scope-id");
        var userReadRoleUuid = new Pulumi.Random.RandomUuid("azure-ad-example-server-user-read-role-id");
        var userWriteRoleUuid = new Pulumi.Random.RandomUuid("azure-ad-example-server-user-read-write-id");
        var serverApplication = new AzureAD.Application("azure-ad-example-server", new AzureAD.ApplicationArgs
        {
            DisplayName = "Azure AD Example Server",
            IdentifierUris =
            {
                "api://azure-ad-example-server"
            },
            Api = new AzureAD.Inputs.ApplicationApiArgs
            {
                MappedClaimsEnabled = true,
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
                    DisplayName = "User.Read",
                    Enabled = true,
                    Value = "User.Read",
                    Description = "User.Read",
                    Id = userReadRoleUuid.Result
                },
                new AzureAD.Inputs.ApplicationAppRoleArgs
                {
                    AllowedMemberTypes =
                    {
                        "User",
                        "Application"
                    },
                    DisplayName = "User.Write",
                    Enabled = true,
                    Value = "User.Write",
                    Description = "User.Write",
                    Id = userWriteRoleUuid.Result
                }
            },
            SinglePageApplication = new AzureAD.Inputs.ApplicationSinglePageApplicationArgs
            {
                RedirectUris =
                {
                    Output.Format($"https://{serverAppService.DefaultHostName}/swagger/oauth2-redirect.html"),
                    "https://localhost:5001/swagger/oauth2-redirect.html"
                }
            },
            Web = new AzureAD.Inputs.ApplicationWebArgs
            {
                ImplicitGrant = new AzureAD.Inputs.ApplicationWebImplicitGrantArgs
                {
                    AccessTokenIssuanceEnabled = true,
                    IdTokenIssuanceEnabled = true
                }
            }
        });

        var serverServicePrincipal = new AzureAD.ServicePrincipal("azure-ad-example-server-service-principal",
            new AzureAD.ServicePrincipalArgs
            {
                ApplicationId = serverApplication.ApplicationId,
            });

        var visualStudio = new AzureAD.ApplicationPreAuthorized("azure-ad-example-server-preauth-visualstudio",
            new AzureAD.ApplicationPreAuthorizedArgs
            {
                AuthorizedAppId = "872cd9fa-d31f-45e0-9eab-6e460a02d1f1",
                ApplicationObjectId = serverApplication.ObjectId,
                PermissionIds =
                {
                    localDevScopeUuid.Result
                }
            });
        var azureCli = new AzureAD.ApplicationPreAuthorized("azure-ad-example-server-preauth-azurecli",
            new AzureAD.ApplicationPreAuthorizedArgs
            {
                AuthorizedAppId = "04b07795-8ddb-461a-bbee-02f9e1bf7b46",
                ApplicationObjectId = serverApplication.ObjectId,
                PermissionIds =
                {
                    localDevScopeUuid.Result
                }
            });

        var devGroupServerUserReadAssignment = new AzureAD.AppRoleAssignment(
            "azure-ad-example-localdev-server-user-read-role-assignment", new AzureAD.AppRoleAssignmentArgs
            {
                AppRoleId = userReadRoleUuid.Result,
                PrincipalObjectId = azureAdResources.LocalDevGroupObjectId,
                ResourceObjectId = serverServicePrincipal.ObjectId
            });

        UserReadRoleUuid = userReadRoleUuid.Result;
        UserWriteRoleUuid = userWriteRoleUuid.Result;
        ServicePrincipalObjectId = serverServicePrincipal.ObjectId;
        AppServiceDefaultHostName = serverAppService.DefaultHostName;
        ApplicationApplicationId = serverApplication.ApplicationId;
    }

    public Output<string> UserReadRoleUuid { get; set; }
    public Output<string> UserWriteRoleUuid { get; set; }
    public Output<string> ServicePrincipalObjectId { get; set; }
    public Output<string> ApplicationApplicationId { get; set; }
    public Output<string> AppServiceDefaultHostName { get; set; }
}