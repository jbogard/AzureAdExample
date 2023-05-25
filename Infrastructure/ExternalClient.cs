using Pulumi;
using AzureAD = Pulumi.AzureAD;

public class ExternalClient
{
    public const string AppName = "external-client";

    public ExternalClient(string prefix)
    {
        #region Create External Client App Registration

        var application = new AzureAD.Application($"{prefix}-{AppName}",
            new AzureAD.ApplicationArgs
            {
                DisplayName = "Azure AD Example External Client",
                Api = new AzureAD.Inputs.ApplicationApiArgs
                {
                    RequestedAccessTokenVersion = 2,
                }
            });

        #region Create Application Secret

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

        #endregion

        #region Create Managed Application Service Principal

        var servicePrincipal = new AzureAD.ServicePrincipal(
            $"{prefix}-{AppName}-service-principal",
            new AzureAD.ServicePrincipalArgs
            {
                ApplicationId = application.ApplicationId,
            });

        #endregion

        #endregion

        #region Set Outputs

        ApplicationSecretValue = applicationSecret.Value;
        ApplicationApplicationId = application.ApplicationId;
        ApplicationServicePrincipalObjectId = servicePrincipal.ObjectId;

        #endregion
    }

    public Output<string> ApplicationSecretValue { get; set; }
    public Output<string> ApplicationApplicationId { get; set; }
    public Output<string> ApplicationServicePrincipalObjectId { get; set; }
}