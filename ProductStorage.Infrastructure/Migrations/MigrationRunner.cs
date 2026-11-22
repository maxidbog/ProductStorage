using FluentMigrator.Runner;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace ProductStorage.Infrastructure.Migrations;

public static class MigrationRunner
{
    public static IHost RunMigrations(this IHost host)
    {
        var configuration = host.Services.GetRequiredService<IConfiguration>();
        var connectionString = configuration.GetConnectionString("Postgres")
            ?? throw new InvalidOperationException("Connection string 'Postgres' is not configured.");

        RunMigrations(connectionString);

        return host;
    }

    public static void RunMigrations(string connectionString)
    {
        using var serviceProvider = CreateService(connectionString);
        using var scope = serviceProvider.CreateScope();
        scope.ServiceProvider.GetRequiredService<IMigrationRunner>().MigrateUp();
    }

    private static ServiceProvider CreateService(string connectionString)
    {
        return new ServiceCollection()
            .AddFluentMigratorCore()
            .ConfigureRunner(builder => builder
                .AddPostgres()
                .WithGlobalConnectionString(connectionString)
                .ScanIn(typeof(MigrationRunner).Assembly).For.Migrations()
                .ConfigureGlobalProcessorOptions(options => options.ProviderSwitches = "Force Quote=false"))
            .AddLogging(logging => logging.AddFluentMigratorConsole())
            .BuildServiceProvider(false);
    }
}
