using Pulumi;
using AzureNative = Pulumi.AzureNative;

namespace Infrastructure;

class MyStack : Stack
{
    public MyStack()
    {
        const string prefix = "entra-id-example";

        var resourceGroup = new AzureNative.Resources.ResourceGroup(prefix);

        var appServicePlan = CreateAppServicePlan(prefix, resourceGroup);

        var entraIdResources = new EntraIdResources(prefix);

        var server = new Server(prefix, resourceGroup, appServicePlan);

        var client = new Client(prefix, resourceGroup, appServicePlan, server);

        var externalClient = new ExternalClient(prefix);

        server.AssignRoles(prefix, entraIdResources);

        server.AssignRoles(prefix, externalClient);

        server.AssignRoles(prefix, client);

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
                    Name = "B1",
                    Tier = "Basic",
                    Size = "B1"
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