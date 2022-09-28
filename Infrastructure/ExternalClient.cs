using Pulumi;
using AzureAD = Pulumi.AzureAD;

public class ExternalClient
{
    public const string AppName = "external-client";

    public ExternalClient(string prefix)
    {

    }

    public Output<string> ApplicationSecretValue { get; set; }
    public Output<string> ApplicationApplicationId { get; set; }
    public Output<string> ApplicationServicePrincipalObjectId { get; set; }
}