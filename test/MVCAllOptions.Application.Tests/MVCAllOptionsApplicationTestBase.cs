using Volo.Abp.Modularity;

namespace MVCAllOptions;

public abstract class MVCAllOptionsApplicationTestBase<TStartupModule> : MVCAllOptionsTestBase<TStartupModule>
    where TStartupModule : IAbpModule
{

}
