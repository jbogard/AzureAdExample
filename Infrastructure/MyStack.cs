using Pulumi;
using AzureNative = Pulumi.AzureNative;

namespace Infrastructure;

class MyStack : Stack
{
    public MyStack()
    {
        var resourceGroup = new AzureNative.Resources.ResourceGroup("azure-ad-example");

        var appServicePlan = CreateAppServicePlan(resourceGroup);

        var azureAdResources = new AzureAdResources();

        var server = new Server(resourceGroup, appServicePlan, azureAdResources);

        var client = new Client(resourceGroup, appServicePlan, server);

        var externalClient = new ExternalClient(server);

        ExternalClientSecret = externalClient.ApplicationSecretValue;
        ExternalClientApplicationId = externalClient.ApplicationApplicationId;
        ClientManagedIdentityClientId = client.UserAssignedIdentityClientId;
        ClientDefaultHostName = client.AppServiceDefaultHostName;
        ServerApplicationClientId = server.ApplicationApplicationId;
        ServerDefaultHostName = server.AppServiceDefaultHostName;
    }

    private static AzureNative.Web.AppServicePlan CreateAppServicePlan(AzureNative.Resources.ResourceGroup resourceGroup)
    {
        return new AzureNative.Web.AppServicePlan("azure-ad-example-app-service-plan",
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