using System.Threading.Tasks;

namespace MVCAllOptions.Data;

public interface IMVCAllOptionsDbSchemaMigrator
{
    Task MigrateAsync();
}
