using System;
using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace MVCAllOptions.EntityFrameworkCore;

/* This class is needed for EF Core console commands
 * (like Add-Migration and Update-Database commands) */
public class MVCAllOptionsDbContextFactory : IDesignTimeDbContextFactory<MVCAllOptionsDbContext>
{
    public MVCAllOptionsDbContext CreateDbContext(string[] args)
    {
        var configuration = BuildConfiguration();
        
        MVCAllOptionsEfCoreEntityExtensionMappings.Configure();

        var builder = new DbContextOptionsBuilder<MVCAllOptionsDbContext>()
            .UseSqlServer(configuration.GetConnectionString("Default"));
        
        return new MVCAllOptionsDbContext(builder.Options);
    }

    private static IConfigurationRoot BuildConfiguration()
    {
        var builder = new ConfigurationBuilder()
            .SetBasePath(Path.Combine(Directory.GetCurrentDirectory(), "../MVCAllOptions.DbMigrator/"))
            .AddJsonFile("appsettings.json", optional: false)
            .AddEnvironmentVariables();

        return builder.Build();
    }
}
