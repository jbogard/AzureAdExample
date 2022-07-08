using Pulumi;
using AzureAD = Pulumi.AzureAD;

public class ExternalClient
{
    public const string AppName = "external-client";

    public ExternalClient(string prefix, Server server)
    {
        var application = new AzureAD.Application($"{prefix}-{AppName}", new AzureAD.ApplicationArgs
        {
            DisplayName = "Azure AD Example External Client",
            Api = new AzureAD.Inputs.ApplicationApiArgs
            {
                RequestedAccessTokenVersion = 2,
            }
        });

        var applicationSecret = new AzureAD.ApplicationPassword(
            $"{prefix}-{AppName}-password",
            new AzureAD.ApplicationPasswordArgs
            {
                ApplicationObjectId = application.ObjectId
            }, new CustomResourceOptions
            {
                AdditionalSecretOutputs =
                {
                    "value"
                }
            });

        var servicePrincipal = new AzureAD.ServicePrincipal($"{prefix}-{AppName}-service-principal",
            new AzureAD.ServicePrincipalArgs
            {
                ApplicationId = application.ApplicationId,
            });

        ApplicationSecretValue = applicationSecret.Value;
        ApplicationApplicationId = application.ApplicationId;
        ApplicationServicePrincipalObjectId = servicePrincipal.ObjectId;
    }

    public Output<string> ApplicationSecretValue { get; set; }
    public Output<string> ApplicationApplicationId { get; set; }
    public Output<string> ApplicationServicePrincipalObjectId { get; set; }
}