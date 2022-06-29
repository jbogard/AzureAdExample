using System.Threading.Tasks;
using Pulumi;
using AzureAD = Pulumi.AzureAD;
using AzureNative = Pulumi.AzureNative;

class MyStack : Stack
{
    public MyStack()
    {
        // Create an Azure Resource Group
        var resourceGroup = new AzureNative.Resources.ResourceGroup("azure-ad-example");

        // Create Azure AD resources
        var userReadRoleUuid = new Pulumi.Random.RandomUuid("azure-ad-example-server-user-read-role-id");
        var userWriteRoleUuid = new Pulumi.Random.RandomUuid("azure-ad-example-server-user-read-write-id");
        var localDevScopeUuid = new Pulumi.Random.RandomUuid("azure-ad-example-server-local-dev-scope-id");
        var visualStudioAppId = "872cd9fa-d31f-45e0-9eab-6e460a02d1f1";
        var azureCliAppId = "04b07795-8ddb-461a-bbee-02f9e1bf7b46";

        var jimmyUser = Output.Create(AzureAD.GetUser.InvokeAsync(new AzureAD.GetUserArgs
        {
            UserPrincipalName = "jimmy.bogard_gmail.com#EXT#@jimmybogardgmail.onmicrosoft.com"
        }));
        var devGroup = new AzureAD.Group("azure-ad-example-localdev", new AzureAD.GroupArgs
        {
            DisplayName = "Azure AD Example Local Dev",
            SecurityEnabled = true
        });
        var jimmyDevGroupMember = new AzureAD.GroupMember("azure-ad-example-jimmy-localdev-member",
            new AzureAD.GroupMemberArgs
            {
                GroupObjectId = devGroup.ObjectId,
                MemberObjectId = jimmyUser.Apply(jimmy => jimmy.ObjectId)
            });

        var clientUserAssignedIdentity = new AzureNative.ManagedIdentity.UserAssignedIdentity("azure-ad-example-azure-client-user", new AzureNative.ManagedIdentity.UserAssignedIdentityArgs
        {
            ResourceGroupName = resourceGroup.Name
        });

        var appServicePlan = new AzureNative.Web.AppServicePlan("azure-ad-example-app-service-plan",
            new AzureNative.Web.AppServicePlanArgs
            {
                Kind = "linux",
                ResourceGroupName = resourceGroup.Name,
                Location = resourceGroup.Location,
                Sku = new AzureNative.Web.Inputs.SkuDescriptionArgs
                {
                    Capacity = 1,
                    Name = "F1",
                    Tier = "Free",
                    Size = "F1",
                    Family = "F"
                },
                Reserved = true
            });

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
                AuthorizedAppId = visualStudioAppId,
                ApplicationObjectId = serverApplication.ObjectId,
                PermissionIds =
                {
                    localDevScopeUuid.Result
                }
            });
        var azureCli = new AzureAD.ApplicationPreAuthorized("azure-ad-example-server-preauth-azurecli",
            new AzureAD.ApplicationPreAuthorizedArgs
            {
                AuthorizedAppId = azureCliAppId,
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
                PrincipalObjectId = devGroup.ObjectId,
                ResourceObjectId = serverServicePrincipal.ObjectId
            });



        ServerApplicationClientId = serverApplication.ApplicationId;
        ServerUrl = serverAppService.DefaultHostName;
    }

    [Output]
    public Output<string> ServerApplicationClientId { get; set; }

    [Output]
    public Output<string> ServerUrl { get; set; }
}
