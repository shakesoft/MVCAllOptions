using MVCAllOptions.Localization;
using Volo.Abp.AspNetCore.Mvc;

namespace MVCAllOptions.Controllers;

/* Inherit your controllers from this class.
 */
public abstract class MVCAllOptionsController : AbpControllerBase
{
    protected MVCAllOptionsController()
    {
        LocalizationResource = typeof(MVCAllOptionsResource);
    }
}
