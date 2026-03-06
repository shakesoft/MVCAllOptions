using MVCAllOptions.Localization;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Localization;
using Volo.Abp.MultiTenancy;

namespace MVCAllOptions.Permissions;

public class MVCAllOptionsPermissionDefinitionProvider : PermissionDefinitionProvider
{
    public override void Define(IPermissionDefinitionContext context)
    {
        var myGroup = context.AddGroup(MVCAllOptionsPermissions.GroupName);

        myGroup.AddPermission(MVCAllOptionsPermissions.Dashboard.Host, L("Permission:Dashboard"), MultiTenancySides.Host);
        myGroup.AddPermission(MVCAllOptionsPermissions.Dashboard.Tenant, L("Permission:Dashboard"), MultiTenancySides.Tenant);

        var booksPermission = myGroup.AddPermission(MVCAllOptionsPermissions.Books.Default, L("Permission:Books"));
        booksPermission.AddChild(MVCAllOptionsPermissions.Books.Create, L("Permission:Books.Create"));
        booksPermission.AddChild(MVCAllOptionsPermissions.Books.Edit, L("Permission:Books.Edit"));
        booksPermission.AddChild(MVCAllOptionsPermissions.Books.Delete, L("Permission:Books.Delete"));
        //Define your own permissions here. Example:
        //myGroup.AddPermission(MVCAllOptionsPermissions.MyPermission1, L("Permission:MyPermission1"));
    }

    private static LocalizableString L(string name)
    {
        return LocalizableString.Create<MVCAllOptionsResource>(name);
    }
}
