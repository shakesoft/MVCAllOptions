using Volo.Payment.Iyzico;
using Volo.Payment.Stripe;
using Volo.Payment.PayPal;
using Volo.Payment.TwoCheckout;
using Volo.Payment.Payu;
using Volo.Payment;
using Volo.Forms;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using MVCAllOptions.Localization;
using MVCAllOptions.MultiTenancy;
using System;
using Volo.Abp.Localization;
using Volo.Abp.Modularity;
using Volo.Abp.MultiTenancy;
using Volo.Abp.PermissionManagement.Identity;
using Volo.Abp.SettingManagement;
using Volo.Abp.BlobStoring.Database;
using Volo.Abp.Caching;
using Volo.Abp.OpenIddict;
using Volo.Abp.PermissionManagement.OpenIddict;
using Volo.Abp.AuditLogging;
using Volo.Abp.BackgroundJobs;
using Volo.Abp.Emailing;
using Volo.Abp.FeatureManagement;
using Volo.Abp.Identity;
using Volo.Abp.Commercial.SuiteTemplates;
using Volo.Abp.LanguageManagement;
using Volo.FileManagement;
using Volo.Abp.TextTemplateManagement;
using Volo.Saas;
using Volo.Abp.Gdpr;
using Volo.Chat;
using Volo.CmsKit;
using Volo.CmsKit.Contact;
using Volo.CmsKit.Newsletters;
using Microsoft.Extensions.AI;
using Volo.Abp.AI;
using Volo.AIManagement;
using Volo.AIManagement.Factory;
using Volo.AIManagement.OpenAI;
using OpenAI;
using System.ClientModel;
using OllamaSharp;

namespace MVCAllOptions;

[DependsOn(
    typeof(AbpPaymentIyzicoDomainModule),
    typeof(AbpPaymentStripeDomainModule),
    typeof(AbpPaymentPayPalDomainModule),
    typeof(AbpPaymentTwoCheckoutDomainModule),
    typeof(AbpPaymentPayuDomainModule),
    typeof(AbpPaymentDomainModule),
    typeof(FormsDomainModule),
    typeof(MVCAllOptionsDomainSharedModule),
    typeof(AbpAuditLoggingDomainModule),
    typeof(AbpCachingModule),
    typeof(AbpBackgroundJobsDomainModule),
    typeof(AbpFeatureManagementDomainModule),
    typeof(AbpPermissionManagementDomainIdentityModule),
    typeof(AbpPermissionManagementDomainOpenIddictModule),
    typeof(AbpSettingManagementDomainModule),
    typeof(AbpEmailingModule),
    typeof(AbpIdentityProDomainModule),
    typeof(AbpOpenIddictProDomainModule),
    typeof(SaasDomainModule),
    typeof(ChatDomainModule),
    typeof(TextTemplateManagementDomainModule),
    typeof(LanguageManagementDomainModule),
    typeof(FileManagementDomainModule),
    typeof(VoloAbpCommercialSuiteTemplatesModule),
    typeof(AbpGdprDomainModule),
    typeof(CmsKitProDomainModule),
    typeof(AIManagementDomainModule),
    typeof(AIManagementOpenAIModule),
    typeof(BlobStoringDatabaseDomainModule)
    )]
public class MVCAllOptionsDomainModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        Configure<AbpMultiTenancyOptions>(options =>
        {
            options.IsEnabled = MultiTenancyConsts.IsEnabled;
        });

        Configure<NewsletterOptions>(options =>
        {
            options.AddPreference(
                "Newsletter_Default",
                new NewsletterPreferenceDefinition(
                    LocalizableString.Create<MVCAllOptionsResource>("NewsletterPreference_Default"),
                    privacyPolicyConfirmation: LocalizableString.Create<MVCAllOptionsResource>("NewsletterPrivacyAcceptMessage")
                )
            );
        });

        Configure<ChatClientFactoryOptions>(options =>
        {
            options.AddFactory<OllamaChatClientFactory>("Ollama");
        });

#if DEBUG
        context.Services.Replace(ServiceDescriptor.Singleton<IEmailSender, NullEmailSender>());
#endif
    }
}
