using Volo.Abp.Modularity;

namespace MVCAllOptions;

[DependsOn(
    typeof(MVCAllOptionsApplicationModule),
    typeof(MVCAllOptionsDomainTestModule)
)]
public class MVCAllOptionsApplicationTestModule : AbpModule
{

}
