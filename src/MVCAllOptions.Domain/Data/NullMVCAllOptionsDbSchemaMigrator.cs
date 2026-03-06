using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;

namespace MVCAllOptions.Data;

/* This is used if database provider does't define
 * IMVCAllOptionsDbSchemaMigrator implementation.
 */
public class NullMVCAllOptionsDbSchemaMigrator : IMVCAllOptionsDbSchemaMigrator, ITransientDependency
{
    public Task MigrateAsync()
    {
        return Task.CompletedTask;
    }
}
