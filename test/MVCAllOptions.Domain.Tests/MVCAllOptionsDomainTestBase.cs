using Volo.Abp.Modularity;

namespace MVCAllOptions;

/* Inherit from this class for your domain layer tests. */
public abstract class MVCAllOptionsDomainTestBase<TStartupModule> : MVCAllOptionsTestBase<TStartupModule>
    where TStartupModule : IAbpModule
{

}
