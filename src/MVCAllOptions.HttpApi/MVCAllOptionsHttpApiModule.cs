using Volo.Payment;
using Volo.Payment.Admin;
using Volo.Forms;
using Localization.Resources.AbpUi;
using MVCAllOptions.Localization;
using Volo.Abp.Account;
using Volo.Abp.SettingManagement;
using Volo.Abp.FeatureManagement;
using Volo.Abp.Identity;
using Volo.Abp.Modularity;
using Volo.Abp.PermissionManagement.HttpApi;
using Volo.Abp.Localization;
using Volo.Abp.AuditLogging;
using Volo.Abp.LanguageManagement;
using Volo.FileManagement;
using Volo.Saas.Host;
using Volo.Abp.TextTemplateManagement;
using Volo.Abp.Gdpr;
using Volo.Abp.OpenIddict;
using Volo.CmsKit;
using Volo.Chat;
using Volo.AIManagement;
using Volo.AIManagement.Client;

namespace MVCAllOptions;

 [DependsOn(
    typeof(AbpPaymentHttpApiModule),
    typeof(AbpPaymentAdminHttpApiModule),
    typeof(FormsHttpApiModule),
    typeof(MVCAllOptionsApplicationContractsModule),
    typeof(AbpPermissionManagementHttpApiModule),
    typeof(AbpSettingManagementHttpApiModule),
    typeof(AbpIdentityHttpApiModule),
    typeof(AbpAccountAdminHttpApiModule),
    typeof(TextTemplateManagementHttpApiModule),
    typeof(AbpAuditLoggingHttpApiModule),
    typeof(AbpOpenIddictProHttpApiModule),
    typeof(LanguageManagementHttpApiModule),
    typeof(FileManagementHttpApiModule),
    typeof(SaasHostHttpApiModule),
    typeof(AbpGdprHttpApiModule),
    typeof(CmsKitProHttpApiModule),
    typeof(ChatHttpApiModule),
    typeof(AIManagementHttpApiModule),
    typeof(AIManagementClientHttpApiModule),
    typeof(AbpAccountPublicHttpApiModule),
    typeof(AbpFeatureManagementHttpApiModule)
    )]
public class MVCAllOptionsHttpApiModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        ConfigureLocalization();
    }

    private void ConfigureLocalization()
    {
        Configure<AbpLocalizationOptions>(options =>
        {
            options.Resources
                .Get<MVCAllOptionsResource>()
                .AddBaseTypes(
                    typeof(AbpUiResource)
                );
        });
    }
}
