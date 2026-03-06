using Volo.Payment;
using Volo.Payment.Admin;
using Volo.Forms;
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.Account;
using Volo.Abp.Modularity;
using Volo.Abp.PermissionManagement;
using Volo.Abp.SettingManagement;
using Volo.Abp.VirtualFileSystem;
using Volo.Abp.FeatureManagement;
using Volo.Abp.Identity;
using Volo.Abp.AuditLogging;
using Volo.Abp.LanguageManagement;
using Volo.FileManagement;
using Volo.Abp.TextTemplateManagement;
using Volo.Saas.Host;
using Volo.Abp.Gdpr;
using Volo.Abp.OpenIddict;
using Volo.CmsKit;
using Volo.Chat;
using Volo.AIManagement;
using Volo.AIManagement.Client;

namespace MVCAllOptions;

[DependsOn(
    typeof(AbpPaymentHttpApiClientModule),
    typeof(AbpPaymentAdminHttpApiClientModule),
    typeof(FormsHttpApiClientModule),
    typeof(MVCAllOptionsApplicationContractsModule),
    typeof(AbpPermissionManagementHttpApiClientModule),
    typeof(AbpFeatureManagementHttpApiClientModule),
    typeof(AbpIdentityHttpApiClientModule),
    typeof(AbpAccountAdminHttpApiClientModule),
    typeof(AbpAccountPublicHttpApiClientModule),
    typeof(SaasHostHttpApiClientModule),
    typeof(AbpAuditLoggingHttpApiClientModule),
    typeof(AbpOpenIddictProHttpApiClientModule),
    typeof(TextTemplateManagementHttpApiClientModule),
    typeof(LanguageManagementHttpApiClientModule),
    typeof(FileManagementHttpApiClientModule),
    typeof(AbpGdprHttpApiClientModule),
    typeof(CmsKitProHttpApiClientModule),
    typeof(ChatHttpApiClientModule),
    typeof(AIManagementHttpApiClientModule),
    typeof(AIManagementClientHttpApiClientModule),
    typeof(AbpSettingManagementHttpApiClientModule)
)]
public class MVCAllOptionsHttpApiClientModule : AbpModule
{
    public const string RemoteServiceName = "Default";

    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddHttpClientProxies(
            typeof(MVCAllOptionsApplicationContractsModule).Assembly,
            RemoteServiceName
        );

        Configure<AbpVirtualFileSystemOptions>(options =>
        {
            options.FileSets.AddEmbedded<MVCAllOptionsHttpApiClientModule>();
        });
    }
}
