using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace PersonalDigitalVault.Api.Data;

public sealed class VaultDbContextFactory : IDesignTimeDbContextFactory<VaultDbContext>
{
    public VaultDbContext CreateDbContext(string[] args)
    {
        var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";
        var basePath = Directory.GetCurrentDirectory();

        // Supports running `dotnet ef` from either the API project folder or solution root.
        if (!File.Exists(Path.Combine(basePath, "appsettings.json")))
        {
            var apiProjectPath = Path.Combine(basePath, "src", "PersonalDigitalVault.Api");
            if (File.Exists(Path.Combine(apiProjectPath, "appsettings.json")))
                basePath = apiProjectPath;
        }

        var configuration = new ConfigurationBuilder()
            .SetBasePath(basePath)
            .AddJsonFile("appsettings.json", optional: false)
            .AddJsonFile($"appsettings.{environment}.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("ConnectionStrings:DefaultConnection is not configured.");

        var optionsBuilder = new DbContextOptionsBuilder<VaultDbContext>();
        optionsBuilder.UseSqlServer(connectionString, sqlOptions =>
            sqlOptions.MigrationsAssembly(typeof(VaultDbContext).Assembly.FullName));

        return new VaultDbContext(optionsBuilder.Options);
    }
}
