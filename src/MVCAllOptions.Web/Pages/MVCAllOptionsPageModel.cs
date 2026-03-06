using MVCAllOptions.Localization;
using Volo.Abp.AspNetCore.Mvc.UI.RazorPages;

namespace MVCAllOptions.Web.Pages;

public abstract class MVCAllOptionsPageModel : AbpPageModel
{
    protected MVCAllOptionsPageModel()
    {
        LocalizationResourceType = typeof(MVCAllOptionsResource);
    }
}
