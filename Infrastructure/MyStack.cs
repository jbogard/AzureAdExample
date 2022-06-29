using System.Threading.Tasks;
using Pulumi;
using AzureAD = Pulumi.AzureAD;
using Pulumi.AzureNative.Resources;
using Pulumi.AzureNative.Storage;

class MyStack : Stack
{
    public MyStack()
    {
        // Create an Azure Resource Group
        var resourceGroup = new ResourceGroup("azure-ad-example", new ResourceGroupArgs
        {
            ResourceGroupName = "azure-ad-example"
        });

        // Create Azure AD resources
        var userReadRoleUuid = new Pulumi.Random.RandomUuid("azure-ad-example-server-user-read-role-id");
        var userWriteRoleUuid = new Pulumi.Random.RandomUuid("azure-ad-example-server-user-read-write-id");
        var localDevScopeUuid = new Pulumi.Random.RandomUuid("azure-ad-example-server-local-dev-scope-id");
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
                RedirectUris = "https://localhost:5001/swagger/oauth2-redirect.html"
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
        var visualStudioAppId = "872cd9fa-d31f-45e0-9eab-6e460a02d1f1";
        var azureCliAppId = "04b07795-8ddb-461a-bbee-02f9e1bf7b46";

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

        var devGroupServerUserReadAssignment = new AzureAD.AppRoleAssignment(
            "azure-ad-example-localdev-server-user-read-role-assignment", new AzureAD.AppRoleAssignmentArgs
            {
                AppRoleId = userReadRoleUuid.Result,
                PrincipalObjectId = devGroup.ObjectId,
                ResourceObjectId = serverServicePrincipal.ObjectId
            });

        //// Create an Azure resource (Storage Account)
        //var storageAccount = new StorageAccount("sa", new StorageAccountArgs
        //{
        //    ResourceGroupName = resourceGroup.Name,
        //    Sku = new SkuArgs
        //    {
        //        Name = SkuName.Standard_LRS
        //    },
        //    Kind = Kind.StorageV2
        //});

        //// Export the primary key of the Storage Account
        //this.PrimaryStorageKey = Output.Tuple(resourceGroup.Name, storageAccount.Name).Apply(names =>
        //    Output.CreateSecret(GetStorageAccountPrimaryKey(names.Item1, names.Item2)));

        ServerApplicationClientId = serverApplication.ApplicationId;
    }

    [Output]
    public Output<string> ServerApplicationClientId { get; set; }

    private static async Task<string> GetStorageAccountPrimaryKey(string resourceGroupName, string accountName)
    {
        var accountKeys = await ListStorageAccountKeys.InvokeAsync(new ListStorageAccountKeysArgs
        {
            ResourceGroupName = resourceGroupName,
            AccountName = accountName
        });
        return accountKeys.Keys[0].Value;
    }
}
