using AgriActifs.Web.Configuration;
using AgriActifs.Web.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace AgriActifs.Web.Data;

public class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    public ApplicationDbContext CreateDbContext(string[] args)
    {
        var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";

        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false)
            .AddJsonFile($"appsettings.{environment}.json", optional: true)
            .AddJsonFile($"appsettings.{environment}.local.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

        var dbOptions = configuration.GetSection(DatabaseOptions.SectionName).Get<DatabaseOptions>()
            ?? new DatabaseOptions();

        var schema = DatabaseExtensions.NormalizeSchema(dbOptions.Schema);
        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();

        switch (dbOptions.Provider.Trim().ToUpperInvariant())
        {
            case "SQLSERVER":
            case "MSSQL":
                optionsBuilder.UseSqlServer(connectionString, sql =>
                    sql.MigrationsHistoryTable("__EFMigrationsHistory", schema));
                break;
            default:
                optionsBuilder.UseNpgsql(connectionString, npgsql =>
                    npgsql.MigrationsHistoryTable("__EFMigrationsHistory", schema));
                break;
        }

        return new ApplicationDbContext(
            optionsBuilder.Options,
            Microsoft.Extensions.Options.Options.Create(dbOptions));
    }
}
