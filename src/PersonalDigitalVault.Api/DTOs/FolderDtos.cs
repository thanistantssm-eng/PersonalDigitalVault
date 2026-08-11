using System.ComponentModel.DataAnnotations;

namespace PersonalDigitalVault.Api.DTOs;

public sealed record FolderResponse(
    Guid Id,
    string Name,
    int DocumentCount,
    DateTime CreatedAtUtc,
    DateTime UpdatedAtUtc);

public sealed record CreateFolderRequest([Required, MaxLength(120)] string Name);
public sealed record RenameFolderRequest([Required, MaxLength(120)] string Name);
