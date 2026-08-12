using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace CCAP.Infrastructure.Persistence;

/// <summary>
/// Creates AppDbContext for EF Core design-time commands such as
/// Add-Migration, Update-Database and Script-Migration.
/// It is not used by the normal API runtime when AddDbContext is registered.
/// </summary>
public sealed class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var apiPath = ResolveApiPath();

        var configuration = new ConfigurationBuilder()
            .SetBasePath(apiPath)
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
            .AddJsonFile("appsettings.Development.json", optional: true, reloadOnChange: false)
            .AddEnvironmentVariables()
            .Build();

        var connectionString = configuration.GetConnectionString("DefaultConnection");
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new InvalidOperationException(
                "Connection string 'DefaultConnection' was not found in CCAP.API configuration.");

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlServer(connectionString)
            .Options;

        return new AppDbContext(options);
    }

    private static string ResolveApiPath()
    {
        var candidates = new[]
        {
            Path.Combine(Directory.GetCurrentDirectory(), "..", "CCAP.API"),
            Path.Combine(Directory.GetCurrentDirectory(), "CCAP.API"),
            Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "CCAP.API")
        };

        foreach (var candidate in candidates)
        {
            var fullPath = Path.GetFullPath(candidate);
            if (Directory.Exists(fullPath) &&
                File.Exists(Path.Combine(fullPath, "appsettings.json")))
            {
                return fullPath;
            }
        }

        throw new DirectoryNotFoundException(
            "Could not locate the CCAP.API directory used for design-time configuration.");
    }
}
