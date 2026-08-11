using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using PersonalDigitalVault.Api.Data;

#nullable disable

namespace PersonalDigitalVault.Api.Migrations;

[DbContext(typeof(VaultDbContext))]
[Migration("20260810154000_InitialMssqlSchema")]
public partial class InitialMssqlSchema : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "ApplicationSettings",
            columns: table => new
            {
                Key = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                Value = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_ApplicationSettings", x => x.Key);
            });

        migrationBuilder.CreateTable(
            name: "UploadLogs",
            columns: table => new
            {
                Id = table.Column<long>(type: "bigint", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                DocumentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                UploadedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_UploadLogs", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "Users",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                FullName = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                Email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                PasswordHash = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                PhoneNumber = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                Role = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                IsActive = table.Column<bool>(type: "bit", nullable: false),
                CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Users", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "Categories",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Categories", x => x.Id);
                table.ForeignKey(
                    name: "FK_Categories_Users_UserId",
                    column: x => x.UserId,
                    principalTable: "Users",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "CredentialRecords",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                Title = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                Category = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                UsernameCipherText = table.Column<string>(type: "nvarchar(max)", nullable: false),
                SecretCipherText = table.Column<string>(type: "nvarchar(max)", nullable: false),
                WebsiteCipherText = table.Column<string>(type: "nvarchar(max)", nullable: true),
                NotesCipherText = table.Column<string>(type: "nvarchar(max)", nullable: true),
                UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_CredentialRecords", x => x.Id);
                table.ForeignKey(
                    name: "FK_CredentialRecords_Users_UserId",
                    column: x => x.UserId,
                    principalTable: "Users",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "Folders",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                Name = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Folders", x => x.Id);
                table.ForeignKey(
                    name: "FK_Folders_Users_UserId",
                    column: x => x.UserId,
                    principalTable: "Users",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "Documents",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                OriginalFileName = table.Column<string>(type: "nvarchar(260)", maxLength: 260, nullable: false),
                StoredFileName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                RelativePath = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                ContentType = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                FileSizeBytes = table.Column<long>(type: "bigint", nullable: false),
                IntegrityHashSha256 = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                FolderId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Documents", x => x.Id);
                table.ForeignKey(
                    name: "FK_Documents_Folders_FolderId",
                    column: x => x.FolderId,
                    principalTable: "Folders",
                    principalColumn: "Id");
                table.ForeignKey(
                    name: "FK_Documents_Users_UserId",
                    column: x => x.UserId,
                    principalTable: "Users",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "IX_Categories_UserId_Name",
            table: "Categories",
            columns: new[] { "UserId", "Name" },
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_CredentialRecords_UserId_Title",
            table: "CredentialRecords",
            columns: new[] { "UserId", "Title" });

        migrationBuilder.CreateIndex(
            name: "IX_Documents_FolderId",
            table: "Documents",
            column: "FolderId");

        migrationBuilder.CreateIndex(
            name: "IX_Documents_UserId_OriginalFileName",
            table: "Documents",
            columns: new[] { "UserId", "OriginalFileName" });

        migrationBuilder.CreateIndex(
            name: "IX_Folders_UserId_Name",
            table: "Folders",
            columns: new[] { "UserId", "Name" },
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_UploadLogs_UserId",
            table: "UploadLogs",
            column: "UserId");

        migrationBuilder.CreateIndex(
            name: "IX_Users_Email",
            table: "Users",
            column: "Email",
            unique: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "ApplicationSettings");
        migrationBuilder.DropTable(name: "Categories");
        migrationBuilder.DropTable(name: "CredentialRecords");
        migrationBuilder.DropTable(name: "Documents");
        migrationBuilder.DropTable(name: "UploadLogs");
        migrationBuilder.DropTable(name: "Folders");
        migrationBuilder.DropTable(name: "Users");
    }
}
