using System.Collections.Generic;
using Pulumi;
using AzureNative = Pulumi.AzureNative;

public class Client
{
    public const string AppName = "azure-client";

    public Client(
        string prefix,
        AzureNative.Resources.ResourceGroup resourceGroup,
        AzureNative.Web.AppServicePlan appServicePlan,
        Server server)
    {

    }

    public Output<string> UserAssignedIdentityClientId { get; set; }
    public Output<string> UserAssignedIdentityPrincipalId { get; set; }
    public Output<string> AppServiceDefaultHostName { get; set; }
}