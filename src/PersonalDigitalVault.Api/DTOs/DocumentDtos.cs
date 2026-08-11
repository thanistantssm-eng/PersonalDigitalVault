using System.ComponentModel.DataAnnotations;

namespace PersonalDigitalVault.Api.DTOs;

public sealed class UploadDocumentRequest
{
    [Required]
    public IFormFile File { get; set; } = null!;
    public Guid? FolderId { get; set; }
}

public sealed record DocumentResponse(
    Guid Id,
    string FileName,
    string ContentType,
    long FileSizeBytes,
    string IntegrityHashSha256,
    Guid? FolderId,
    string? FolderName,
    DateTime CreatedAtUtc,
    DateTime UpdatedAtUtc);

public sealed record RenameDocumentRequest([Required, MaxLength(220)] string NewFileName);
public sealed record IntegrityResponse(Guid DocumentId, bool IsValid, string StoredHash, string CurrentHash);
