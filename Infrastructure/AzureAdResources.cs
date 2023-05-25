using Pulumi;
using AzureAD = Pulumi.AzureAD;

public class AzureAdResources
{
    public const string LocalDevGroupName = "localdev";

    public AzureAdResources(string prefix)
    {
        #region Set Up Local Dev Group

        var config = new Config();

        var jimmyUpn = config.Require("jimmy-upn");

        var jimmyUser = Output.Create(
            AzureAD.GetUser.InvokeAsync(new AzureAD.GetUserArgs
            {
                UserPrincipalName = jimmyUpn
            }));
        var devGroup = new AzureAD.Group($"{prefix}-{LocalDevGroupName}", new AzureAD.GroupArgs
        {
            DisplayName = "Azure AD Example Local Dev",
            SecurityEnabled = true
        });
        var jimmyDevGroupMember = new AzureAD.GroupMember($"{prefix}-jimmy-{LocalDevGroupName}-member",
            new AzureAD.GroupMemberArgs
            {
                GroupObjectId = devGroup.ObjectId,
                MemberObjectId = jimmyUser.Apply(jimmy => jimmy.ObjectId)
            });

        #endregion

        #region Set Outputs
        LocalDevGroupObjectId = devGroup.ObjectId;
        #endregion
    }

    public Output<string> LocalDevGroupObjectId { get; set; }
}