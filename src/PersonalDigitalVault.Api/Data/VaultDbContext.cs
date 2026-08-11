using Microsoft.EntityFrameworkCore;
using PersonalDigitalVault.Api.Models;

namespace PersonalDigitalVault.Api.Data;

public sealed class VaultDbContext(DbContextOptions<VaultDbContext> options) : DbContext(options)
{
    public DbSet<VaultUser> Users => Set<VaultUser>();
    public DbSet<VaultFolder> Folders => Set<VaultFolder>();
    public DbSet<VaultCategory> Categories => Set<VaultCategory>();
    public DbSet<StoredDocument> Documents => Set<StoredDocument>();
    public DbSet<CredentialRecord> CredentialRecords => Set<CredentialRecord>();
    public DbSet<UploadLog> UploadLogs => Set<UploadLog>();
    public DbSet<ApplicationSetting> ApplicationSettings => Set<ApplicationSetting>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<VaultUser>(entity =>
        {
            entity.HasIndex(x => x.Email).IsUnique();
            entity.Property(x => x.Email).IsRequired();
            entity.Property(x => x.PasswordHash).IsRequired();
            entity.Property(x => x.FullName).IsRequired();
        });

        modelBuilder.Entity<VaultFolder>(entity =>
        {
            entity.HasIndex(x => new { x.UserId, x.Name }).IsUnique();
            entity.Property(x => x.Name).IsRequired();
            entity.HasOne(x => x.User)
                .WithMany(x => x.Folders)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<VaultCategory>(entity =>
        {
            entity.HasIndex(x => new { x.UserId, x.Name }).IsUnique();
            entity.Property(x => x.Name).IsRequired();
            entity.HasOne(x => x.User)
                .WithMany(x => x.Categories)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<StoredDocument>(entity =>
        {
            entity.HasIndex(x => new { x.UserId, x.OriginalFileName });
            entity.Property(x => x.OriginalFileName).IsRequired();
            entity.Property(x => x.RelativePath).IsRequired();
            entity.Property(x => x.IntegrityHashSha256).IsRequired();
            entity.HasOne(x => x.User)
                .WithMany(x => x.Documents)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(x => x.Folder)
                .WithMany(x => x.Documents)
                .HasForeignKey(x => x.FolderId)
                // SQL Server rejects multiple cascade paths:
                // User -> Documents and User -> Folders -> Documents.
                // Folder deletion is handled explicitly in FoldersController
                // by setting each document's FolderId to null first.
                .OnDelete(DeleteBehavior.NoAction);
        });

        modelBuilder.Entity<CredentialRecord>(entity =>
        {
            entity.HasIndex(x => new { x.UserId, x.Title });
            entity.Property(x => x.Title).IsRequired();
            entity.Property(x => x.UsernameCipherText).IsRequired();
            entity.Property(x => x.SecretCipherText).IsRequired();
            entity.HasOne(x => x.User)
                .WithMany(x => x.CredentialRecords)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<UploadLog>().HasIndex(x => x.UserId);
        modelBuilder.Entity<ApplicationSetting>().Property(x => x.Key).ValueGeneratedNever();
    }
}
