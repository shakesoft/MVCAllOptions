using Volo.Abp.Modularity;

namespace MVCAllOptions;

[DependsOn(
    typeof(MVCAllOptionsDomainModule),
    typeof(MVCAllOptionsTestBaseModule)
)]
public class MVCAllOptionsDomainTestModule : AbpModule
{

}
