using Pulumi;
using AzureNative = Pulumi.AzureNative;

namespace Infrastructure;

class MyStack : Stack
{
    public MyStack()
    {
        const string prefix = "azure-ad-example";

        var resourceGroup = new AzureNative.Resources.ResourceGroup(prefix);

        var appServicePlan = CreateAppServicePlan(prefix, resourceGroup);

        var azureAdResources = new AzureAdResources(prefix);

        var server = new Server(prefix, resourceGroup, appServicePlan, azureAdResources);

        var client = new Client(prefix, resourceGroup, appServicePlan, server);

        var externalClient = new ExternalClient(prefix, server);

        server.AssignRoles(prefix, azureAdResources);
        server.AssignRoles(prefix, client);
        server.AssignRoles(prefix, externalClient);

        ExternalClientSecret = externalClient.ApplicationSecretValue;
        ExternalClientApplicationId = externalClient.ApplicationApplicationId;
        ClientManagedIdentityClientId = client.UserAssignedIdentityClientId;
        ClientDefaultHostName = client.AppServiceDefaultHostName;
        ServerApplicationClientId = server.ApplicationApplicationId;
        ServerDefaultHostName = server.AppServiceDefaultHostName;
    }

    private static AzureNative.Web.AppServicePlan CreateAppServicePlan(string prefix, 
        AzureNative.Resources.ResourceGroup resourceGroup)
    {
        return new AzureNative.Web.AppServicePlan($"{prefix}-app-service-plan",
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
    }

    [Output]
    public Output<string> ExternalClientSecret { get; set; }

    [Output]
    public Output<string> ExternalClientApplicationId { get; set; }

    [Output]
    public Output<string> ClientManagedIdentityClientId { get; set; }

    [Output]
    public Output<string> ClientDefaultHostName { get; set; }

    [Output]
    public Output<string> ServerApplicationClientId { get; set; }

    [Output]
    public Output<string> ServerDefaultHostName { get; set; }
}