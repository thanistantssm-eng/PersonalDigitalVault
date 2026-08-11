using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PersonalDigitalVault.Api.Models;

namespace PersonalDigitalVault.Api.Data;

public static class DbInitializer
{
    public static async Task InitializeAsync(IServiceProvider services, IConfiguration configuration)
    {
        await using var scope = services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<VaultDbContext>();
        var environment = scope.ServiceProvider.GetRequiredService<IHostEnvironment>();

        // IMPORTANT: this database is owned by EF Core Migrations only.
        // Do not call EnsureCreated/EnsureCreatedAsync together with migrations.
        if (environment.IsDevelopment() && configuration.GetValue<bool>("Database:AutoMigrateInDevelopment"))
        {
            await db.Database.MigrateAsync();
        }

        if (!configuration.GetValue<bool>("SeedAdmin:Enabled")) return;

        var email = (configuration["SeedAdmin:Email"] ?? "admin@pdv.local").Trim().ToLowerInvariant();
        if (await db.Users.AnyAsync(x => x.Email == email)) return;

        var user = new VaultUser
        {
            FullName = configuration["SeedAdmin:FullName"] ?? "System Administrator",
            Email = email,
            Role = UserRole.Administrator,
            IsActive = true
        };

        var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher<VaultUser>>();
        user.PasswordHash = hasher.HashPassword(user, configuration["SeedAdmin:Password"] ?? "ChangeMe!123");
        db.Users.Add(user);
        await db.SaveChangesAsync();
    }
}
