using Volo.Abp.Ui.Branding;
using Volo.Abp.DependencyInjection;
using Microsoft.Extensions.Localization;
using MVCAllOptions.Localization;

namespace MVCAllOptions.Web.Public;

[Dependency(ReplaceServices = true)]
public class MVCAllOptionsBrandingProvider : DefaultBrandingProvider
{
    private IStringLocalizer<MVCAllOptionsResource> _localizer;

    public MVCAllOptionsBrandingProvider(IStringLocalizer<MVCAllOptionsResource> localizer)
    {
        _localizer = localizer;
    }

    public override string AppName => _localizer["AppName"];
}
