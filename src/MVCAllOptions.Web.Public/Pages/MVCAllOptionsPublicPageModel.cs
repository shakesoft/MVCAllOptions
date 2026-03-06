using MVCAllOptions.Localization;
using Volo.Abp.AspNetCore.Mvc.UI.RazorPages;

namespace MVCAllOptions.Web.Public.Pages;

/* Inherit your Page Model classes from this class.
 */
public abstract class MVCAllOptionsPublicPageModel : AbpPageModel
{
    protected MVCAllOptionsPublicPageModel()
    {
        LocalizationResourceType = typeof(MVCAllOptionsResource);
    }
}
