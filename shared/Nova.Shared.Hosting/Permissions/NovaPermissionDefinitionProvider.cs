using Volo.Abp.Authorization.Permissions;

namespace Nova.Shared.Hosting.Permissions;

public class NovaPermissionDefinitionProvider : PermissionDefinitionProvider
{
    public override void Define(IPermissionDefinitionContext context)
    {
        var group = context.GetGroupOrNull("Nova");
        if (group == null)
        {
            context.AddGroup("Nova");
        }
    }
}
