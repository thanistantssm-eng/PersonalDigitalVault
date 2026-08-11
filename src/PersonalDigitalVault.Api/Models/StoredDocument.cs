using System.ComponentModel.DataAnnotations;

namespace PersonalDigitalVault.Api.Models;

public sealed class StoredDocument
{
    public Guid Id { get; set; } = Guid.NewGuid();

    [MaxLength(260)]
    public string OriginalFileName { get; set; } = string.Empty;

    [MaxLength(100)]
    public string StoredFileName { get; set; } = string.Empty;

    [MaxLength(500)]
    public string RelativePath { get; set; } = string.Empty;

    [MaxLength(150)]
    public string ContentType { get; set; } = "application/octet-stream";

    public long FileSizeBytes { get; set; }

    [MaxLength(64)]
    public string IntegrityHashSha256 { get; set; } = string.Empty;

    public Guid UserId { get; set; }
    public VaultUser User { get; set; } = null!;

    public Guid? FolderId { get; set; }
    public VaultFolder? Folder { get; set; }

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;
}
