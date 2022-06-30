using Pulumi;
using AzureAD = Pulumi.AzureAD;
using AzureNative = Pulumi.AzureNative;

public class AzureAdResources
{
    public AzureAdResources()
    {
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

        LocalDevGroupObjectId = devGroup.ObjectId;
    }

    public Output<string> LocalDevGroupObjectId { get; set; }
}