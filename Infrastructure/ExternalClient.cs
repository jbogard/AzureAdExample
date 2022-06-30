using Pulumi;
using AzureAD = Pulumi.AzureAD;

public class ExternalClient
{
    public ExternalClient(Server server)
    {
        var externalClientApplication = new AzureAD.Application("azure-ad-example-external-client", new AzureAD.ApplicationArgs
        {
            DisplayName = "Azure AD Example External Client",
            Api = new AzureAD.Inputs.ApplicationApiArgs
            {
                RequestedAccessTokenVersion = 2,
            }
        });

        var externalClientApplicationSecret = new AzureAD.ApplicationPassword(
            "azure-ad-example-external-client-password",
            new AzureAD.ApplicationPasswordArgs
            {
                ApplicationObjectId = externalClientApplication.ObjectId
            }, new CustomResourceOptions
            {
                AdditionalSecretOutputs =
                {
                    "value"
                }
            });

        var externalClientServicePrincipal = new AzureAD.ServicePrincipal("azure-ad-example-external-client-service-principal",
            new AzureAD.ServicePrincipalArgs
            {
                ApplicationId = externalClientApplication.ApplicationId,
            });
        var externalClientServerUserReadAssignment = new AzureAD.AppRoleAssignment(
            "azure-ad-example-external-client-server-user-read-role-assignment", new AzureAD.AppRoleAssignmentArgs
            {
                AppRoleId = server.UserReadRoleUuid,
                PrincipalObjectId = externalClientServicePrincipal.ObjectId,
                ResourceObjectId = server.ServicePrincipalObjectId
            });

        ApplicationSecretValue = externalClientApplicationSecret.Value;
        ApplicationApplicationId = externalClientApplication.ApplicationId;
    }

    public Output<string> ApplicationSecretValue { get; set; }
    public Output<string> ApplicationApplicationId { get; set; }
}