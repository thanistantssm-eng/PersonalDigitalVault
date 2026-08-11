using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using PersonalDigitalVault.Api.Data;

#nullable disable

namespace PersonalDigitalVault.Api.Migrations;

[DbContext(typeof(VaultDbContext))]
partial class VaultDbContextModelSnapshot : ModelSnapshot
{
    protected override void BuildModel(ModelBuilder modelBuilder)
    {
#pragma warning disable 612, 618
        modelBuilder
            .HasAnnotation("ProductVersion", "8.0.0")
            .HasAnnotation("Relational:MaxIdentifierLength", 128);

        SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder);

        modelBuilder.Entity("PersonalDigitalVault.Api.Models.ApplicationSetting", b =>
        {
            b.Property<string>("Key").HasMaxLength(100).HasColumnType("nvarchar(100)");
            b.Property<DateTime>("UpdatedAtUtc").HasColumnType("datetime2");
            b.Property<string>("Value").IsRequired().HasMaxLength(1000).HasColumnType("nvarchar(1000)");
            b.HasKey("Key");
            b.ToTable("ApplicationSettings");
        });

        modelBuilder.Entity("PersonalDigitalVault.Api.Models.CredentialRecord", b =>
        {
            b.Property<Guid>("Id").ValueGeneratedOnAdd().HasColumnType("uniqueidentifier");
            b.Property<string>("Category").HasMaxLength(100).HasColumnType("nvarchar(100)");
            b.Property<DateTime>("CreatedAtUtc").HasColumnType("datetime2");
            b.Property<string>("NotesCipherText").HasColumnType("nvarchar(max)");
            b.Property<string>("SecretCipherText").IsRequired().HasColumnType("nvarchar(max)");
            b.Property<string>("Title").IsRequired().HasMaxLength(150).HasColumnType("nvarchar(150)");
            b.Property<DateTime>("UpdatedAtUtc").HasColumnType("datetime2");
            b.Property<Guid>("UserId").HasColumnType("uniqueidentifier");
            b.Property<string>("UsernameCipherText").IsRequired().HasColumnType("nvarchar(max)");
            b.Property<string>("WebsiteCipherText").HasColumnType("nvarchar(max)");
            b.HasKey("Id");
            b.HasIndex("UserId", "Title");
            b.ToTable("CredentialRecords");
        });

        modelBuilder.Entity("PersonalDigitalVault.Api.Models.StoredDocument", b =>
        {
            b.Property<Guid>("Id").ValueGeneratedOnAdd().HasColumnType("uniqueidentifier");
            b.Property<string>("ContentType").IsRequired().HasMaxLength(150).HasColumnType("nvarchar(150)");
            b.Property<DateTime>("CreatedAtUtc").HasColumnType("datetime2");
            b.Property<long>("FileSizeBytes").HasColumnType("bigint");
            b.Property<Guid?>("FolderId").HasColumnType("uniqueidentifier");
            b.Property<string>("IntegrityHashSha256").IsRequired().HasMaxLength(64).HasColumnType("nvarchar(64)");
            b.Property<string>("OriginalFileName").IsRequired().HasMaxLength(260).HasColumnType("nvarchar(260)");
            b.Property<string>("RelativePath").IsRequired().HasMaxLength(500).HasColumnType("nvarchar(500)");
            b.Property<string>("StoredFileName").IsRequired().HasMaxLength(100).HasColumnType("nvarchar(100)");
            b.Property<DateTime>("UpdatedAtUtc").HasColumnType("datetime2");
            b.Property<Guid>("UserId").HasColumnType("uniqueidentifier");
            b.HasKey("Id");
            b.HasIndex("FolderId");
            b.HasIndex("UserId", "OriginalFileName");
            b.ToTable("Documents");
        });

        modelBuilder.Entity("PersonalDigitalVault.Api.Models.UploadLog", b =>
        {
            b.Property<long>("Id").ValueGeneratedOnAdd().HasColumnType("bigint");
            SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<long>("Id"));
            b.Property<Guid>("DocumentId").HasColumnType("uniqueidentifier");
            b.Property<DateTime>("UploadedAtUtc").HasColumnType("datetime2");
            b.Property<Guid>("UserId").HasColumnType("uniqueidentifier");
            b.HasKey("Id");
            b.HasIndex("UserId");
            b.ToTable("UploadLogs");
        });

        modelBuilder.Entity("PersonalDigitalVault.Api.Models.VaultCategory", b =>
        {
            b.Property<Guid>("Id").ValueGeneratedOnAdd().HasColumnType("uniqueidentifier");
            b.Property<DateTime>("CreatedAtUtc").HasColumnType("datetime2");
            b.Property<string>("Name").IsRequired().HasMaxLength(100).HasColumnType("nvarchar(100)");
            b.Property<DateTime>("UpdatedAtUtc").HasColumnType("datetime2");
            b.Property<Guid>("UserId").HasColumnType("uniqueidentifier");
            b.HasKey("Id");
            b.HasIndex("UserId", "Name").IsUnique();
            b.ToTable("Categories");
        });

        modelBuilder.Entity("PersonalDigitalVault.Api.Models.VaultFolder", b =>
        {
            b.Property<Guid>("Id").ValueGeneratedOnAdd().HasColumnType("uniqueidentifier");
            b.Property<DateTime>("CreatedAtUtc").HasColumnType("datetime2");
            b.Property<string>("Name").IsRequired().HasMaxLength(120).HasColumnType("nvarchar(120)");
            b.Property<DateTime>("UpdatedAtUtc").HasColumnType("datetime2");
            b.Property<Guid>("UserId").HasColumnType("uniqueidentifier");
            b.HasKey("Id");
            b.HasIndex("UserId", "Name").IsUnique();
            b.ToTable("Folders");
        });

        modelBuilder.Entity("PersonalDigitalVault.Api.Models.VaultUser", b =>
        {
            b.Property<Guid>("Id").ValueGeneratedOnAdd().HasColumnType("uniqueidentifier");
            b.Property<DateTime>("CreatedAtUtc").HasColumnType("datetime2");
            b.Property<string>("Email").IsRequired().HasMaxLength(256).HasColumnType("nvarchar(256)");
            b.Property<string>("FullName").IsRequired().HasMaxLength(120).HasColumnType("nvarchar(120)");
            b.Property<bool>("IsActive").HasColumnType("bit");
            b.Property<string>("PasswordHash").IsRequired().HasMaxLength(500).HasColumnType("nvarchar(500)");
            b.Property<string>("PhoneNumber").HasMaxLength(30).HasColumnType("nvarchar(30)");
            b.Property<string>("Role").IsRequired().HasMaxLength(30).HasColumnType("nvarchar(30)");
            b.Property<DateTime>("UpdatedAtUtc").HasColumnType("datetime2");
            b.HasKey("Id");
            b.HasIndex("Email").IsUnique();
            b.ToTable("Users");
        });

        modelBuilder.Entity("PersonalDigitalVault.Api.Models.CredentialRecord", b =>
        {
            b.HasOne("PersonalDigitalVault.Api.Models.VaultUser", "User")
                .WithMany("CredentialRecords")
                .HasForeignKey("UserId")
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();
            b.Navigation("User");
        });

        modelBuilder.Entity("PersonalDigitalVault.Api.Models.StoredDocument", b =>
        {
            b.HasOne("PersonalDigitalVault.Api.Models.VaultFolder", "Folder")
                .WithMany("Documents")
                .HasForeignKey("FolderId")
                .OnDelete(DeleteBehavior.NoAction);
            b.HasOne("PersonalDigitalVault.Api.Models.VaultUser", "User")
                .WithMany("Documents")
                .HasForeignKey("UserId")
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();
            b.Navigation("Folder");
            b.Navigation("User");
        });

        modelBuilder.Entity("PersonalDigitalVault.Api.Models.VaultCategory", b =>
        {
            b.HasOne("PersonalDigitalVault.Api.Models.VaultUser", "User")
                .WithMany("Categories")
                .HasForeignKey("UserId")
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();
            b.Navigation("User");
        });

        modelBuilder.Entity("PersonalDigitalVault.Api.Models.VaultFolder", b =>
        {
            b.HasOne("PersonalDigitalVault.Api.Models.VaultUser", "User")
                .WithMany("Folders")
                .HasForeignKey("UserId")
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();
            b.Navigation("User");
        });

        modelBuilder.Entity("PersonalDigitalVault.Api.Models.VaultFolder", b =>
        {
            b.Navigation("Documents");
        });

        modelBuilder.Entity("PersonalDigitalVault.Api.Models.VaultUser", b =>
        {
            b.Navigation("Categories");
            b.Navigation("CredentialRecords");
            b.Navigation("Documents");
            b.Navigation("Folders");
        });
#pragma warning restore 612, 618
    }
}
