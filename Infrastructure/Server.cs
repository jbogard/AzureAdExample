using Pulumi;
using AzureAD = Pulumi.AzureAD;
using AzureNative = Pulumi.AzureNative;

public class Server
{
    public const string AppName = "server";

    public Server(
        string prefix,
        AzureNative.Resources.ResourceGroup resourceGroup,
        AzureNative.Web.AppServicePlan appServicePlan)
    {
    }

    public Output<string> TodoReadRoleUuid { get; set; }
    public Output<string> TodoWriteRoleUuid { get; set; }
    public Output<string> ServicePrincipalObjectId { get; set; }
    public Output<string> ApplicationApplicationId { get; set; }
    public Output<string> AppServiceDefaultHostName { get; set; }


}