using MVCAllOptions.EntityFrameworkCore;
using Volo.Abp.Autofac;
using Volo.Abp.Modularity;

namespace MVCAllOptions.DbMigrator;

[DependsOn(
    typeof(AbpAutofacModule),
    typeof(MVCAllOptionsEntityFrameworkCoreModule),
    typeof(MVCAllOptionsApplicationContractsModule)
)]
public class MVCAllOptionsDbMigratorModule : AbpModule
{
}
