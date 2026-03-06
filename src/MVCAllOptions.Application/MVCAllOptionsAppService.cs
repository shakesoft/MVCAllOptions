using MVCAllOptions.Localization;
using Volo.Abp.Application.Services;

namespace MVCAllOptions;

/* Inherit your application services from this class.
 */
public abstract class MVCAllOptionsAppService : ApplicationService
{
    protected MVCAllOptionsAppService()
    {
        LocalizationResource = typeof(MVCAllOptionsResource);
    }
}
